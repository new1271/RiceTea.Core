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
        public static Unit Right(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Or(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Or);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit And(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.And);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Xor(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Xor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Add(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Add);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Subtract(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Subtract);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Multiply(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Multiply);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Divide(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Divide);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Min(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Min);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Max(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Max);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedRight(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Right);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedOr(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Or);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedAnd(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.And);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedXor(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Xor);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedAdd(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Add);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedSubtract(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Subtract);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedMultiply(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Multiply);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedDivide(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Divide);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedMin(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Min);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedMax(T* ptr, nuint length, T value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Max);

        [Inline(InlineBehavior.Remove)]
        private static Unit BinaryOperationCore(ref T* ptr, ref nuint length, T value, [InlineParameter] BinaryOperatorType method)
        {
            if (method == BinaryOperatorType.Divide)
            {
                if (UnsafeHelper.IsIntegerType<T>() && UnsafeHelper.Equals(value, default))
                    return FastCore.ThrowDivideByZeroException();
            }
            if (Limits.CheckTypeCanBeVectorized<T>() && length > Limits.GetLimitForVectorizing<T>())
            {
                return method switch
                {
                    BinaryOperatorType.Right => VectorizedRight(ptr, length, value),
                    BinaryOperatorType.Or => VectorizedOr(ptr, length, value),
                    BinaryOperatorType.And => VectorizedAnd(ptr, length, value),
                    BinaryOperatorType.Xor => VectorizedXor(ptr, length, value),
                    BinaryOperatorType.Add => VectorizedAdd(ptr, length, value),
                    BinaryOperatorType.Subtract => VectorizedSubtract(ptr, length, value),
                    BinaryOperatorType.Multiply => VectorizedMultiply(ptr, length, value),
                    BinaryOperatorType.Divide => VectorizedDivide(ptr, length, value),
                    BinaryOperatorType.Min => VectorizedMin(ptr, length, value),
                    BinaryOperatorType.Max => VectorizedMax(ptr, length, value),
                    _ => ArgumentOutOfRangeException.Throw<Unit>(nameof(method)),
                };
            }
            return ScalarizedBinaryOperationCore(ref ptr, ref length, value, method);
        }

        [Inline(InlineBehavior.Remove)]
        private static partial Unit VectorizedBinaryOperationCore(ref T* ptr, ref nuint length, T value, [InlineParameter] BinaryOperatorType type);

        [Inline(InlineBehavior.Remove)]
        private static Unit ScalarizedBinaryOperationCore(ref T* ptr, ref nuint length, T value, [InlineParameter] BinaryOperatorType method)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                ptr[0] = DoOperation(ptr[0], value, method);
                ptr[1] = DoOperation(ptr[1], value, method);
                ptr[2] = DoOperation(ptr[2], value, method);
                ptr[3] = DoOperation(ptr[3], value, method);
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, value, method);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, value, method);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, value, method);

        Return:
            return Unit.Default;

            [Inline(InlineBehavior.Remove)]
            static T DoOperation(T item, T value, [InlineParameter] BinaryOperatorType method)
                => method switch
                {
                    BinaryOperatorType.Left => item,
                    BinaryOperatorType.Right => value,
                    BinaryOperatorType.Or => UnsafeHelper.Or(item, value),
                    BinaryOperatorType.And => UnsafeHelper.And(item, value),
                    BinaryOperatorType.Xor => UnsafeHelper.Xor(item, value),
                    BinaryOperatorType.Add => UnsafeHelper.Add(item, value),
                    BinaryOperatorType.Subtract => UnsafeHelper.Subtract(item, value),
                    BinaryOperatorType.Multiply => UnsafeHelper.Multiply(item, value),
                    BinaryOperatorType.Divide => UnsafeHelper.IsUnsignedIntegerType<T>() ? UnsafeHelper.DivideUnsigned(item, value) : UnsafeHelper.Divide(item, value),
                    BinaryOperatorType.Min => UnsafeHelper.Min(item, value),
                    BinaryOperatorType.Max => UnsafeHelper.Max(item, value),
                    _ => ArgumentOutOfRangeException.Throw<T>(nameof(method)),
                };
        }
    }

    unsafe partial class FastCoreOfBoolean
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Left(bool* ptr, nuint length, bool value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Left);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Right(bool* ptr, nuint length, bool value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Or(bool* ptr, nuint length, bool value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Or);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit And(bool* ptr, nuint length, bool value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.And);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Xor(bool* ptr, nuint length, bool value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Xor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Divide(bool* ptr, nuint length, bool value)
            => value ? Left(ptr, length, value) : FastCore.ThrowDivideByZeroException();

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedLeft(bool* ptr, nuint length, bool value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Left);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedRight(bool* ptr, nuint length, bool value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Right);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedOr(bool* ptr, nuint length, bool value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Or);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedAnd(bool* ptr, nuint length, bool value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.And);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Unit VectorizedXor(bool* ptr, nuint length, bool value)
            => VectorizedBinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Xor);

        [Inline(InlineBehavior.Remove)]
        private static Unit BinaryOperationCore(ref bool* ptr, ref nuint length, bool value, [InlineParameter] BinaryOperatorType method)
        {
            value = UnsafeHelper.And(value, true); // Normalize value
            if (Limits.CheckTypeCanBeVectorized<byte>() && length > Limits.GetLimitForVectorizing<byte>())
            {
                return method switch
                {
                    BinaryOperatorType.Left => VectorizedLeft(ptr, length, value),
                    BinaryOperatorType.Right => VectorizedRight(ptr, length, value),
                    BinaryOperatorType.Or => VectorizedOr(ptr, length, value),
                    BinaryOperatorType.And => VectorizedAnd(ptr, length, value),
                    BinaryOperatorType.Xor => VectorizedXor(ptr, length, value),
                    _ => ArgumentOutOfRangeException.Throw<Unit>(nameof(method)),
                };

            }
            return ScalarizedBinaryOperationCore(ref ptr, ref length, value, method);
        }

        [Inline(InlineBehavior.Remove)]
        private static partial Unit VectorizedBinaryOperationCore(ref bool* ptr, ref nuint length, bool value, [InlineParameter] BinaryOperatorType type);

        [Inline(InlineBehavior.Remove)]
        private static Unit ScalarizedBinaryOperationCore(ref bool* ptr, ref nuint length, bool value, [InlineParameter] BinaryOperatorType method)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                ptr[0] = DoOperation(ptr[0], value, method);
                ptr[1] = DoOperation(ptr[1], value, method);
                ptr[2] = DoOperation(ptr[2], value, method);
                ptr[3] = DoOperation(ptr[3], value, method);
            }
            bool* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, value, method);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, value, method);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = DoOperation(*ptr, value, method);

        Return:
            return Unit.Default;

            [Inline(InlineBehavior.Remove)]
            static bool Normalize(bool item) => UnsafeHelper.And(item, true);

            [Inline(InlineBehavior.Remove)]
            static bool DoOperation(bool item, bool value, [InlineParameter] BinaryOperatorType method)
            {
                DebugHelper.ThrowIf(method == BinaryOperatorType.Divide && !value);
                return method switch
                {
                    BinaryOperatorType.Left or BinaryOperatorType.Divide => Normalize(item),
                    BinaryOperatorType.Right => value,
                    BinaryOperatorType.Or or BinaryOperatorType.Max => UnsafeHelper.Or(Normalize(item), value),
                    BinaryOperatorType.And or BinaryOperatorType.Multiply or BinaryOperatorType.Min => UnsafeHelper.And(item, value),
                    BinaryOperatorType.Xor or BinaryOperatorType.Add or BinaryOperatorType.Subtract => UnsafeHelper.Xor(Normalize(item), value),
                    _ => ArgumentOutOfRangeException.Throw<bool>(nameof(method)),
                };
            }
        }
    }

    unsafe partial class SlowCore<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Right(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Or(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Or);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit And(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.And);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Xor(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Xor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Add(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Add);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Subtract(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Subtract);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Multiply(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Multiply);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Divide(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Divide);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Min(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Min);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit Max(T* ptr, nuint length, T value)
            => BinaryOperationCore(ref ptr, ref length, value, BinaryOperatorType.Max);

        [Inline(InlineBehavior.Remove)]
        private static Unit BinaryOperationCore(ref T* ptr, ref nuint length, T value, [InlineParameter] BinaryOperatorType operatorType)
        {
            if (operatorType == BinaryOperatorType.Left)
                return Unit.Default;
            return BinaryOperationCore(ptr, length, value, operatorType switch
            {
                BinaryOperatorType.Left => BinaryOperator<T>.Left,
                BinaryOperatorType.Right => BinaryOperator<T>.Right,
                BinaryOperatorType.Or => BinaryOperator<T>.Or,
                BinaryOperatorType.And => BinaryOperator<T>.And,
                BinaryOperatorType.Xor => BinaryOperator<T>.Xor,
                BinaryOperatorType.Add => BinaryOperator<T>.Add,
                BinaryOperatorType.Subtract => BinaryOperator<T>.Subtract,
                BinaryOperatorType.Multiply => BinaryOperator<T>.Multiply,
                BinaryOperatorType.Divide => BinaryOperator<T>.Divide,
                BinaryOperatorType.Min => BinaryOperator<T>.Min,
                BinaryOperatorType.Max => BinaryOperator<T>.Max,
                _ => ArgumentOutOfRangeException.Throw<BinaryOperator<T>>(nameof(operatorType))
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit BinaryOperationCore(T* ptr, nuint length, T value, IBinaryOperator<T> @operator)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                ptr[0] = @operator.Operate(ptr[0], value);
                ptr[1] = @operator.Operate(ptr[1], value);
                ptr[2] = @operator.Operate(ptr[2], value);
                ptr[3] = @operator.Operate(ptr[3], value);
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = @operator.Operate(*ptr, value);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = @operator.Operate(*ptr, value);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = @operator.Operate(*ptr, value);

        Return:
            return Unit.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Unit BinaryOperationCore(T* ptr, nuint length, T value, BinaryOperation<T> operation)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                ptr[0] = operation.Invoke(ptr[0], value);
                ptr[1] = operation.Invoke(ptr[1], value);
                ptr[2] = operation.Invoke(ptr[2], value);
                ptr[3] = operation.Invoke(ptr[3], value);
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = operation.Invoke(*ptr, value);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = operation.Invoke(*ptr, value);
            ptr++;
            if (ptr >= ptrEnd)
                goto Return;
            *ptr = operation.Invoke(*ptr, value);

        Return:
            return Unit.Default;
        }
    }
}
