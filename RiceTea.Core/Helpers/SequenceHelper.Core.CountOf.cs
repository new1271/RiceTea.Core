using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using InlineMethod;

#pragma warning disable CS8500

namespace RiceTea.Core.Helpers;

partial class SequenceHelper
{
    unsafe partial class FastCore<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint CountOf(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, ref length, value, CompareMethod.Include);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint CountOfExclude(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, ref length, value, CompareMethod.Exclude);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint CountOfGreaterThan(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, ref length, value, CompareMethod.GreaterThan);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint CountOfLessThan(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, ref length, value, CompareMethod.LessThan);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint CountOfGreaterThanOrEquals(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, ref length, value, CompareMethod.GreaterThanOrEquals);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint CountOfLessThanOrEquals(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, ref length, value, CompareMethod.LessThanOrEquals);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static nuint VectorizedCountOf(T* ptr, nuint length, T value)
            => VectorizedCountOfCore(ref ptr, ref length, value, CompareMethod.Include);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static nuint VectorizedCountOfExclude(T* ptr, nuint length, T value)
            => VectorizedCountOfCore(ref ptr, ref length, value, CompareMethod.Exclude);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static nuint VectorizedCountOfGreaterThan(T* ptr, nuint length, T value)
            => VectorizedCountOfCore(ref ptr, ref length, value, CompareMethod.GreaterThan);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static nuint VectorizedCountOfLessThan(T* ptr, nuint length, T value)
            => VectorizedCountOfCore(ref ptr, ref length, value, CompareMethod.LessThan);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static nuint VectorizedCountOfGreaterThanOrEquals(T* ptr, nuint length, T value)
            => VectorizedCountOfCore(ref ptr, ref length, value, CompareMethod.GreaterThanOrEquals);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static nuint VectorizedCountOfLessThanOrEquals(T* ptr, nuint length, T value)
            => VectorizedCountOfCore(ref ptr, ref length, value, CompareMethod.LessThanOrEquals);

        [Inline(InlineBehavior.Remove)]
        private static nuint CountOfCore(ref T* ptr, ref nuint length, T value, [InlineParameter] CompareMethod method)
        {
            if (Limits.CheckTypeCanBeVectorized<T>() && length > Limits.GetLimitForVectorizing<T>())
                return method switch
                {
                    CompareMethod.Include => VectorizedCountOf(ptr, length, value),
                    CompareMethod.Exclude => VectorizedCountOfExclude(ptr, length, value),
                    CompareMethod.GreaterThan => VectorizedCountOfGreaterThan(ptr, length, value),
                    CompareMethod.GreaterThanOrEquals => VectorizedCountOfGreaterThanOrEquals(ptr, length, value),
                    CompareMethod.LessThan => VectorizedCountOfLessThan(ptr, length, value),
                    CompareMethod.LessThanOrEquals => VectorizedCountOfLessThanOrEquals(ptr, length, value),
                    _ => ArgumentOutOfRangeException.Throw<nuint>(nameof(method))
                };

            return ScalarizedCountOfCore(ref ptr, ref length, value, method);
        }

        [SkipLocalsInit]
        [Inline(InlineBehavior.Remove)]
        private static partial nuint VectorizedCountOfCore(ref T* ptr, ref nuint length, T value, [InlineParameter] CompareMethod method);

        [Inline(InlineBehavior.Remove)]
        private static nuint ScalarizedCountOfCore(ref T* ptr, ref nuint length, T value, [InlineParameter] CompareMethod method)
        {
            nuint counter = 0;
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                counter +=
                    MathHelper.BooleanToNativeUnsigned(ScalarizedCompare(ptr[0], value, method)) +
                    MathHelper.BooleanToNativeUnsigned(ScalarizedCompare(ptr[1], value, method)) +
                    MathHelper.BooleanToNativeUnsigned(ScalarizedCompare(ptr[2], value, method)) +
                    MathHelper.BooleanToNativeUnsigned(ScalarizedCompare(ptr[3], value, method));
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto Result;
            counter += MathHelper.BooleanToNativeUnsigned(ScalarizedCompare(*ptr, value, method));
            ptr++;
            if (ptr >= ptrEnd)
                goto Result;
            counter += MathHelper.BooleanToNativeUnsigned(ScalarizedCompare(*ptr, value, method));
            ptr++;
            if (ptr >= ptrEnd)
                goto Result;
            counter += MathHelper.BooleanToNativeUnsigned(ScalarizedCompare(*ptr, value, method));

        Result:
            return counter;
        }
    }

    unsafe partial class SlowCore<T>
    {
        public static nuint CountOf(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, length, value, CompareMethod.Include);

        public static nuint CountOfExclude(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, length, value, CompareMethod.Exclude);

        public static nuint CountOfGreaterThan(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, length, value, CompareMethod.GreaterThan);

        public static nuint CountOfLessThan(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, length, value, CompareMethod.LessThan);

        public static nuint CountOfGreaterThanOrEquals(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, length, value, CompareMethod.GreaterThanOrEquals);

        public static nuint CountOfLessThanOrEquals(T* ptr, nuint length, T value)
            => CountOfCore(ref ptr, length, value, CompareMethod.LessThanOrEquals);

        [Inline(InlineBehavior.Remove)]
        private static nuint CountOfCore(ref T* ptr, nuint length, T value, [InlineParameter] CompareMethod method)
        {
            nuint counter = 0;
            if (method == CompareMethod.Include || method == CompareMethod.Exclude)
            {
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                for (nuint i = 0; i < length; i++, ptr++)
                {
                    if (Compare(comparer, *ptr, value, method))
                        counter++;
                }
            }
            else
            {
                Comparer<T> comparer = Comparer<T>.Default;
                for (nuint i = 0; i < length; i++, ptr++)
                {
                    if (Compare(comparer, *ptr, value, method))
                        counter++;
                }
            }
            return counter;
        }
    }
}
