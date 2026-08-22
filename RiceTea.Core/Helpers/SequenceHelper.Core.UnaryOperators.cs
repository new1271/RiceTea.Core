using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Structures;

#pragma warning disable CS8500

namespace RiceTea.Core.Helpers;

partial class SequenceHelper
{
    unsafe partial class FastCore<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Identity(T* ptr, nuint length)
            => UnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Not(T* ptr, nuint length)
            => UnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Not);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedIdentity(T* ptr, nuint length)
            => VectorizedUnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Identity);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedOr(T* ptr, nuint length)
            => VectorizedUnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Not);

        [Inline(InlineBehavior.Remove)]
        private static Unit UnaryOperationCore(ref T* ptr, ref nuint length, [InlineParameter] UnaryOperatorType method)
        {
            if (Limits.CheckTypeCanBeVectorized<T>() && length > Limits.GetLimitForVectorizing<T>())
            {
                return method switch
                {
                    UnaryOperatorType.Identity => VectorizedIdentity(ptr, length),
                    UnaryOperatorType.Not => VectorizedOr(ptr, length),
                    _ => ArgumentOutOfRangeException.Throw<Unit>(nameof(method)),
                };
            }
            return ScalarizedUnaryOperationCore(ref ptr, ref length, method);
        }

        [Inline(InlineBehavior.Remove)]
        private static partial Unit VectorizedUnaryOperationCore(ref T* ptr, ref nuint length, [InlineParameter] UnaryOperatorType type);

        [Inline(InlineBehavior.Remove)]
        private static Unit ScalarizedUnaryOperationCore(ref T* ptr, ref nuint length, [InlineParameter] UnaryOperatorType method)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                ptr[0] = DoOperation(ptr[0], method);
                ptr[1] = DoOperation(ptr[1], method);
                ptr[2] = DoOperation(ptr[2], method);
                ptr[3] = DoOperation(ptr[3], method);
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, method);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, method);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, method);

        Return:
            return Unit.Default;

            [Inline(InlineBehavior.Remove)]
            static T DoOperation(T item, [InlineParameter] UnaryOperatorType method)
                => method switch
                {
                    UnaryOperatorType.Identity => item,
                    UnaryOperatorType.Not => UnsafeHelper.Not(item),
                    _ => ArgumentOutOfRangeException.Throw<T>(nameof(method)),
                };
        }
    }

    unsafe partial class FastCoreOfBoolean
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Identity(bool* ptr, nuint length)
            => UnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Not(bool* ptr, nuint length)
            => UnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Not);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedIdentity(bool* ptr, nuint length)
            => VectorizedUnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Identity);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedNot(bool* ptr, nuint length)
            => VectorizedUnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Not);

        [Inline(InlineBehavior.Remove)]
        private static Unit UnaryOperationCore(ref bool* ptr, ref nuint length, [InlineParameter] UnaryOperatorType method)
        {
            if (Limits.CheckTypeCanBeVectorized<byte>() && length > Limits.GetLimitForVectorizing<byte>())
            {
                return method switch
                {
                    UnaryOperatorType.Identity => VectorizedIdentity(ptr, length),
                    UnaryOperatorType.Not => VectorizedNot(ptr, length),
                    _ => ArgumentOutOfRangeException.Throw<Unit>(nameof(method)),
                };

            }
            return ScalarizedUnaryOperationCore(ref ptr, ref length, method);
        }

        [Inline(InlineBehavior.Remove)]
        private static partial Unit VectorizedUnaryOperationCore(ref bool* ptr, ref nuint length, [InlineParameter] UnaryOperatorType type);

        [Inline(InlineBehavior.Remove)]
        private static Unit ScalarizedUnaryOperationCore(ref bool* ptr, ref nuint length, [InlineParameter] UnaryOperatorType method)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                ptr[0] = DoOperation(ptr[0], method);
                ptr[1] = DoOperation(ptr[1], method);
                ptr[2] = DoOperation(ptr[2], method);
                ptr[3] = DoOperation(ptr[3], method);
            }
            bool* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, method);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, method);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, method);

        Return:
            return Unit.Default;

            [Inline(InlineBehavior.Remove)]
            static bool Normalize(bool item) => UnsafeHelper.And(item, true);

            [Inline(InlineBehavior.Remove)]
            static bool DoOperation(bool item, [InlineParameter] UnaryOperatorType method)
            {
                return method switch
                {
                    UnaryOperatorType.Identity => Normalize(item),
                    UnaryOperatorType.Not => item == default,
                    _ => ArgumentOutOfRangeException.Throw<bool>(nameof(method)),
                };
            }
        }
    }

    unsafe partial class SlowCore<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Identity(T* ptr, nuint length)
            => UnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Not(T* ptr, nuint length)
            => UnaryOperationCore(ref ptr, ref length, UnaryOperatorType.Not);

        [Inline(InlineBehavior.Remove)]
        private static Unit UnaryOperationCore(ref T* ptr, ref nuint length, [InlineParameter] UnaryOperatorType operatorType)
        {
            if (operatorType == UnaryOperatorType.Identity)
                return Unit.Default;
            return UnaryOperationCore(ptr, length, operatorType switch
            {
                UnaryOperatorType.Identity => UnaryOperator<T>.Identity,
                UnaryOperatorType.Not => UnaryOperator<T>.Not,
                _ => ArgumentOutOfRangeException.Throw<UnaryOperator<T>>(nameof(operatorType))
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit UnaryOperationCore(T* ptr, nuint length, IUnaryOperator<T> @operator)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                ptr[0] = @operator.Operate(ptr[0]);
                ptr[1] = @operator.Operate(ptr[1]);
                ptr[2] = @operator.Operate(ptr[2]);
                ptr[3] = @operator.Operate(ptr[3]);
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = @operator.Operate(*ptr);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = @operator.Operate(*ptr);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = @operator.Operate(*ptr);

        Return:
            return Unit.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit UnaryOperationCore(T* ptr, nuint length, UnaryOperation<T> operation)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                ptr[0] = operation.Invoke(ptr[0]);
                ptr[1] = operation.Invoke(ptr[1]);
                ptr[2] = operation.Invoke(ptr[2]);
                ptr[3] = operation.Invoke(ptr[3]);
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = operation.Invoke(*ptr);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = operation.Invoke(*ptr);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = operation.Invoke(*ptr);

        Return:
            return Unit.Default;
        }
    }
}
