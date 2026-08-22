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
        public static bool Contains(T* ptr, nuint length, T value)
            => MathHelper.ToBooleanUnsafe(PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.Include, accurateResult: false));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsExclude(T* ptr, nuint length, T value)
            => MathHelper.ToBooleanUnsafe(PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.Exclude, accurateResult: false));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsGreaterThan(T* ptr, nuint length, T value)
            => MathHelper.ToBooleanUnsafe(PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.GreaterThan, accurateResult: false));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsLessThan(T* ptr, nuint length, T value)
            => MathHelper.ToBooleanUnsafe(PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.LessThan, accurateResult: false));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsGreaterThanOrEquals(T* ptr, nuint length, T value)
            => MathHelper.ToBooleanUnsafe(PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.GreaterThanOrEquals, accurateResult: false));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsLessThanOrEquals(T* ptr, nuint length, T value)
            => MathHelper.ToBooleanUnsafe(PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.LessThanOrEquals, accurateResult: false));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* PointerIndexOf(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.Include, accurateResult: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* PointerIndexOfExclude(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.Exclude, accurateResult: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* PointerIndexOfGreaterThan(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.GreaterThan, accurateResult: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* PointerIndexOfLessThan(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.LessThan, accurateResult: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* PointerIndexOfGreaterThanOrEquals(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.GreaterThanOrEquals, accurateResult: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* PointerIndexOfLessThanOrEquals(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, ref length, value, CompareMethod.LessThanOrEquals, accurateResult: true);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedContains(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.Include, accurateResult: false);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedContainsExclude(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.Exclude, accurateResult: false);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedContainsGreaterThan(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.GreaterThan, accurateResult: false);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedContainsLessThan(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.LessThan, accurateResult: false);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedContainsGreaterThanOrEquals(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.GreaterThanOrEquals, accurateResult: false);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedContainsLessThanOrEquals(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.LessThanOrEquals, accurateResult: false);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedPointerIndexOf(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.Include, accurateResult: true);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedPointerIndexOfExclude(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.Exclude, accurateResult: true);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedPointerIndexOfGreaterThan(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.GreaterThan, accurateResult: true);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedPointerIndexOfLessThan(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.LessThan, accurateResult: true);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedPointerIndexOfGreaterThanOrEquals(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.GreaterThanOrEquals, accurateResult: true);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T* VectorizedPointerIndexOfLessThanOrEquals(T* ptr, nuint length, T value)
            => VectorizedPointerIndexOfCore(ref ptr, ref length, value, CompareMethod.LessThanOrEquals, accurateResult: true);

        [Inline(InlineBehavior.Remove)]
        private static T* PointerIndexOfCore(ref T* ptr, ref nuint length, T value, [InlineParameter] CompareMethod method, [InlineParameter] bool accurateResult)
        {
            if (Limits.CheckTypeCanBeVectorized<T>() && length > Limits.GetLimitForVectorizing<T>())
                return method switch
                {
                    CompareMethod.Include => accurateResult ? VectorizedPointerIndexOf(ptr, length, value) : VectorizedContains(ptr, length, value),
                    CompareMethod.Exclude => accurateResult ? VectorizedPointerIndexOfExclude(ptr, length, value) : VectorizedContainsExclude(ptr, length, value),
                    CompareMethod.GreaterThan => accurateResult ? VectorizedPointerIndexOfGreaterThan(ptr, length, value) : VectorizedContainsGreaterThan(ptr, length, value),
                    CompareMethod.GreaterThanOrEquals => accurateResult ? VectorizedPointerIndexOfGreaterThanOrEquals(ptr, length, value) : VectorizedContainsGreaterThanOrEquals(ptr, length, value),
                    CompareMethod.LessThan => accurateResult ? VectorizedPointerIndexOfLessThan(ptr, length, value) : VectorizedContainsLessThan(ptr, length, value),
                    CompareMethod.LessThanOrEquals => accurateResult ? VectorizedPointerIndexOfLessThanOrEquals(ptr, length, value) : VectorizedContainsLessThanOrEquals(ptr, length, value),
                    _ => (T*)ArgumentOutOfRangeException.Throw<nuint>(nameof(method))
                };

            return ScalarizedPointerIndexOfCore(ref ptr, ref length, value, method, accurateResult);
        }

        [SkipLocalsInit]
        [Inline(InlineBehavior.Remove)]
        private static partial T* VectorizedPointerIndexOfCore(ref T* ptr, ref nuint length, T value, [InlineParameter] CompareMethod method, [InlineParameter] bool accurateResult);

        [Inline(InlineBehavior.Remove)]
        private static T* ScalarizedPointerIndexOfCore(ref T* ptr, ref nuint length, T value, [InlineParameter] CompareMethod method, [InlineParameter] bool accurateResult)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                if (ScalarizedCompare(ptr[0], value, method))
                    return accurateResult ? ptr : (T*)Booleans.TrueNative;
                if (ScalarizedCompare(ptr[1], value, method))
                    return accurateResult ? ptr + 1 : (T*)Booleans.TrueNative;
                if (ScalarizedCompare(ptr[2], value, method))
                    return accurateResult ? ptr + 2 : (T*)Booleans.TrueNative;
                if (ScalarizedCompare(ptr[3], value, method))
                    return accurateResult ? ptr + 3 : (T*)Booleans.TrueNative;
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                goto NotFound;
            if (ScalarizedCompare(*ptr, value, method))
                return accurateResult ? ptr : (T*)Booleans.TrueNative;
            ptr++;
            if (ptr >= ptrEnd)
                goto NotFound;
            if (ScalarizedCompare(*ptr, value, method))
                return accurateResult ? ptr : (T*)Booleans.TrueNative;
            ptr++;
            if (ptr >= ptrEnd)
                goto NotFound;
            if (ScalarizedCompare(*ptr, value, method))
                return accurateResult ? ptr : (T*)Booleans.TrueNative;

        NotFound:
            return null;
        }
    }

    unsafe partial class SlowCore<T>
    {
        public static bool Contains(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.Include) != null;

        public static bool ContainsExclude(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.Exclude) != null;

        public static bool ContainsGreaterThan(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.GreaterThan) != null;

        public static bool ContainsLessThan(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.LessThan) != null;

        public static bool ContainsGreaterThanOrEquals(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.GreaterThanOrEquals) != null;

        public static bool ContainsLessThanOrEquals(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.LessThanOrEquals) != null;

        public static T* PointerIndexOf(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.Include);

        public static T* PointerIndexOfExclude(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.Exclude);

        public static T* PointerIndexOfGreaterThan(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.GreaterThan);

        public static T* PointerIndexOfLessThan(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.LessThan);

        public static T* PointerIndexOfGreaterThanOrEquals(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.GreaterThanOrEquals);

        public static T* PointerIndexOfLessThanOrEquals(T* ptr, nuint length, T value)
            => PointerIndexOfCore(ref ptr, length, value, CompareMethod.LessThanOrEquals);

        [Inline(InlineBehavior.Remove)]
        private static T* PointerIndexOfCore(ref T* ptr, nuint length, T value, [InlineParameter] CompareMethod method)
        {
            if (method == CompareMethod.Include || method == CompareMethod.Exclude)
            {
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                for (nuint i = 0; i < length; i++, ptr++)
                {
                    if (Compare(comparer, *ptr, value, method))
                        return ptr;
                }
            }
            else
            {
                Comparer<T> comparer = Comparer<T>.Default;
                for (nuint i = 0; i < length; i++, ptr++)
                {
                    if (Compare(comparer, *ptr, value, method))
                        return ptr;
                }
            }
            return null;
        }
    }
}
