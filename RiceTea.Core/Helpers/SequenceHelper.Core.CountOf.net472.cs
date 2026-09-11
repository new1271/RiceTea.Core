#if NET472_OR_GREATER
using System.Numerics;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Extensions;

namespace RiceTea.Core.Helpers;

partial class SequenceHelper
{
    unsafe partial class FastCore<T>
    {
        private static partial nuint VectorizedCountOfCore(ref T* ptr, ref nuint length, T value, CompareMethod method)
        {
            nuint counter = 0;

            Vector<T> valueVector = new Vector<T>(value);

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector<T>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                Vector<T> sourceVector = UnsafeHelper.ReadUnaligned<Vector<T>>(ptr);
                Vector<T> resultVector = VectorizedCompare(sourceVector, valueVector, method);
                if (length > (nuint)Vector<T>.Count * 2)
                {
                    headRemainder = (UnsafeHelper.SizeOf<Vector<T>>() - headRemainder) / UnsafeHelper.SizeOf<T>(); // 取得數量
                    CountOf_CollectResult(in resultVector, ref counter, headRemainder, isFullyCheck: false, fromMostIndex: true);
                    ptr += headRemainder;
                    length -= headRemainder;
                    goto VectorizedLoop;
                }
                else
                {
                    CountOf_CollectResult(in resultVector, ref counter, length, isFullyCheck: true, fromMostIndex: false);
                    ptr += (nuint)Vector<T>.Count;
                    length -= (nuint)Vector<T>.Count;
                    goto TailProcess;
                }
            }

        VectorizedLoop:
            do
            {
                Vector<T> sourceVector = UnsafeHelper.Read<Vector<T>>(ptr);
                Vector<T> resultVector = VectorizedCompare(sourceVector, valueVector, method);
                CountOf_CollectResult(in resultVector, ref counter, 0, isFullyCheck: true, fromMostIndex: false);
                ptr += (nuint)Vector<T>.Count;
                length -= (nuint)Vector<T>.Count;
            } while (length >= (nuint)Vector<T>.Count);
            goto TailProcess;

        TailProcess:
            if (length > 0)
            {
                ptr = ptr + length - (nuint)Vector<T>.Count;
                Vector<T> sourceVector = UnsafeHelper.ReadUnaligned<Vector<T>>(ptr);
                Vector<T> resultVector = VectorizedCompare(sourceVector, valueVector, method);
                CountOf_CollectResult(in resultVector, ref counter, length, isFullyCheck: false, fromMostIndex: true);
            }
            return counter;
        }

        [Inline(InlineBehavior.Remove)]
        private static void CountOf_CollectResult(in Vector<T> sourceVector, ref nuint counter, nuint offset, [InlineParameter] bool isFullyCheck, [InlineParameter] bool fromMostIndex)
        {
            counter += isFullyCheck ? 
                FullyCheck(in sourceVector, offset) : 
                OptionalCheck(in sourceVector, offset, fromMostIndex ? (nuint)Vector<T>.Count - offset : 0);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static nuint FullyCheck(in Vector<T> sourceVector, nuint offset)
            {
                return Vector<T>.Count switch
                {
                    4 => _4(sourceVector, offset),
                    2 => _2(sourceVector, offset),
                    1 => _1(sourceVector, offset),
                    _ => _Default(sourceVector, offset),
                };

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static nuint _4(in Vector<T> sourceVector, nuint offset)
                {
                    ref readonly T reference = ref UnsafeHelper.As<Vector<T>, T>(ref UnsafeHelper.AsRefIn(in sourceVector));
                    T allBitSet = UnsafeHelper.GetAllBitsSetValue<T>();
                    return MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(reference, allBitSet)) +
                        MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, 1), allBitSet)) +
                        MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, 2), allBitSet)) +
                        MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, 3), allBitSet));
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static nuint _2(in Vector<T> sourceVector, nuint offset)
                {
                    ref readonly T reference = ref UnsafeHelper.As<Vector<T>, T>(ref UnsafeHelper.AsRefIn(in sourceVector));
                    T allBitSet = UnsafeHelper.GetAllBitsSetValue<T>();
                    return MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(reference, allBitSet)) +
                         MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, 1), allBitSet));
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static nuint _1(in Vector<T> sourceVector, nuint offset)
                {
                    ref readonly T reference = ref UnsafeHelper.As<Vector<T>, T>(ref UnsafeHelper.AsRefIn(in sourceVector));
                    T allBitSet = UnsafeHelper.GetAllBitsSetValue<T>();
                    return MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(reference, allBitSet));
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static nuint _Default(in Vector<T> sourceVector, nuint offset)
                {
                    nuint counter = 0;

                    ref readonly T reference = ref UnsafeHelper.As<Vector<T>, T>(ref UnsafeHelper.AsRefIn(in sourceVector));
                    T allBitSet = UnsafeHelper.GetAllBitsSetValue<T>();
                    for (nuint i = 0; i < (nuint)Vector<T>.Count; i += 4)
                    {
                        counter +=
                            MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, i), allBitSet)) +
                            MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, i + 1), allBitSet)) +
                            MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, i + 2), allBitSet)) +
                            MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, i + 3), allBitSet));
                    }

                    return counter;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static nuint OptionalCheck(in Vector<T> sourceVector, nuint index, nuint offset)
            {
                nuint counter = 0;

                ref readonly T reference = ref UnsafeHelper.As<Vector<T>, T>(ref UnsafeHelper.AsRefIn(in sourceVector));
                T allBitSet = UnsafeHelper.GetAllBitsSetValue<T>();
                for (; offset >= 4; offset -= 4, index += 4) // 4x 展開
                {
                    counter +=
                        MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, index), allBitSet)) +
                        MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, index + 1), allBitSet)) +
                        MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, index + 2), allBitSet)) +
                        MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, index + 3), allBitSet));
                }
                if (offset <= 0)
                    goto Tail;
                counter += MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, index), allBitSet));
                index++;
                if (offset <= 1)
                    goto Tail;
                counter += MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, index + 1), allBitSet));
                if (offset <= 2)
                    goto Tail;
                counter += MathHelper.BooleanToNativeUnsigned(UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, index + 2), allBitSet));

            Tail:
                return counter;
            }
        }
    }
}
#endif