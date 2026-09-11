#if NET472_OR_GREATER
using System.Numerics;
using System.Runtime.CompilerServices;

using InlineMethod;

namespace RiceTea.Core.Helpers;

partial class SequenceHelper
{
    unsafe partial class FastCore<T>
    {
        private static partial T* VectorizedPointerIndexOfCore(ref T* ptr, ref nuint length, T value, CompareMethod method, bool accurateResult)
        {
            Vector<T> valueVector = new Vector<T>(value);

            nuint headRemainder = (nuint)ptr % UnsafeHelper.SizeOf<Vector<T>>();
            if (headRemainder == 0)
                goto VectorizedLoop;
            else
            {
                Vector<T> sourceVector = UnsafeHelper.ReadUnaligned<Vector<T>>(ptr);
                Vector<T> resultVector = VectorizedCompare(sourceVector, valueVector, method);
                if (resultVector == Vector<T>.Zero)
                {
                    if (length > (nuint)Vector<T>.Count * 2)
                    {
                        headRemainder = (UnsafeHelper.SizeOf<Vector<T>>() - headRemainder) / UnsafeHelper.SizeOf<T>(); // 取得數量
                        ptr += headRemainder;
                        length -= headRemainder;
                        goto VectorizedLoop;
                    }
                    else
                    {
                        ptr += (nuint)Vector<T>.Count;
                        length -= (nuint)Vector<T>.Count;
                        goto TailProcess;
                    }
                }
                return accurateResult ? IndexOf_FindResult(in resultVector, ref ptr) : (T*)Booleans.TrueNative;
            }

        VectorizedLoop:
            do
            {
                Vector<T> sourceVector = UnsafeHelper.Read<Vector<T>>(ptr);
                Vector<T> resultVector = VectorizedCompare(sourceVector, valueVector, method);
                if (resultVector == Vector<T>.Zero)
                {
                    ptr += (nuint)Vector<T>.Count;
                    length -= (nuint)Vector<T>.Count;
                    continue;
                }
                return accurateResult ? IndexOf_FindResult(in resultVector, ref ptr) : (T*)Booleans.TrueNative;
            } while (length >= (nuint)Vector<T>.Count);
            goto TailProcess;

        TailProcess:
            if (length > 0)
            {
                ptr = ptr + length - (nuint)Vector<T>.Count;
                Vector<T> sourceVector = UnsafeHelper.ReadUnaligned<Vector<T>>(ptr);
                Vector<T> resultVector = VectorizedCompare(sourceVector, valueVector, method);
                if (resultVector == Vector<T>.Zero)
                    return null;
                return accurateResult ? IndexOf_FindResult(in resultVector, ref ptr) : (T*)Booleans.TrueNative;
            }
            else
                return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T* IndexOf_FindResult(in Vector<T> sourceVector, ref T* sourcePointer)
        {
            return Vector<T>.Count switch
            {
                4 => _4(in sourceVector, ref sourcePointer),
                2 => _2(in sourceVector, ref sourcePointer),
                1 => _1(in sourceVector, ref sourcePointer),
                _ => _Default(in sourceVector, ref sourcePointer)
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static T* _4(in Vector<T> sourceVector, ref T* sourcePointer)
            {
                ref readonly T reference = ref UnsafeHelper.As<Vector<T>, T>(ref UnsafeHelper.AsRefIn(in sourceVector));
                T allBitSet = UnsafeHelper.GetAllBitsSetValue<T>();

                if (UnsafeHelper.Equals(reference, allBitSet))
                    return sourcePointer + 0;
                if (UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, 1), allBitSet))
                    return sourcePointer + 1;
                if (UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, 2), allBitSet))
                    return sourcePointer + 2;
                return sourcePointer + 3;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static T* _2(in Vector<T> sourceVector, ref T* sourcePointer)
            {
                ref readonly T reference = ref UnsafeHelper.As<Vector<T>, T>(ref UnsafeHelper.AsRefIn(in sourceVector));
                T allBitSet = UnsafeHelper.GetAllBitsSetValue<T>();

                return sourcePointer + MathHelper.BooleanToNativeUnsigned(UnsafeHelper.NotEquals(reference, allBitSet));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static T* _1(in Vector<T> sourceVector, ref T* sourcePointer)
                => sourcePointer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static T* _Default(in Vector<T> sourceVector, ref T* sourcePointer)
            {
                ref readonly T reference = ref UnsafeHelper.As<Vector<T>, T>(ref UnsafeHelper.AsRefIn(in sourceVector));
                T allBitSet = UnsafeHelper.GetAllBitsSetValue<T>();

                for (nuint i = 0; i < (nuint)Vector<T>.Count; i += 4, sourcePointer += 4)
                {
                    if (UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, i), allBitSet))
                        return sourcePointer + 0;
                    if (UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, i + 1), allBitSet))
                        return sourcePointer + 1;
                    if (UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, i + 2), allBitSet))
                        return sourcePointer + 2;
                    if (UnsafeHelper.Equals(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, i + 3), allBitSet))
                        return sourcePointer + 3;
                }

                return null;
            }
        }
    }
}
#endif