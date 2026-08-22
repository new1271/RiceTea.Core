using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using InlineMethod;

namespace RiceTea.Core.Helpers;

partial class SequenceHelper
{
#pragma warning disable CS0162
#pragma warning disable CS8500
    unsafe partial class FastCore
    {
        [Inline(InlineBehavior.Remove)]
        public static void Replace(nint* ptr, nuint length, nint filter, nint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(int):
                    FastCore<int>.Replace((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                    return;
                case sizeof(long):
                    FastCore<long>.Replace((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(int):
                            FastCore<int>.Replace((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                            return;
                        case sizeof(long):
                            FastCore<long>.Replace((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceExclude(nint* ptr, nuint length, nint filter, nint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(int):
                    FastCore<int>.ReplaceExclude((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                    return;
                case sizeof(long):
                    FastCore<long>.ReplaceExclude((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(int):
                            FastCore<int>.ReplaceExclude((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                            return;
                        case sizeof(long):
                            FastCore<long>.ReplaceExclude((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceGreaterThan(nint* ptr, nuint length, nint filter, nint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(int):
                    FastCore<int>.ReplaceGreaterThan((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                    return;
                case sizeof(long):
                    FastCore<long>.ReplaceGreaterThan((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(int):
                            FastCore<int>.ReplaceGreaterThan((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                            return;
                        case sizeof(long):
                            FastCore<long>.ReplaceGreaterThan((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceGreaterThanOrEquals(nint* ptr, nuint length, nint filter, nint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(int):
                    FastCore<int>.ReplaceGreaterThanOrEquals((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                    return;
                case sizeof(long):
                    FastCore<long>.ReplaceGreaterThanOrEquals((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(int):
                            FastCore<int>.ReplaceGreaterThanOrEquals((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                            return;
                        case sizeof(long):
                            FastCore<long>.ReplaceGreaterThanOrEquals((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceLessThan(nint* ptr, nuint length, nint filter, nint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(int):
                    FastCore<int>.ReplaceLessThan((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                    return;
                case sizeof(long):
                    FastCore<long>.ReplaceLessThan((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(int):
                            FastCore<int>.ReplaceLessThan((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                            return;
                        case sizeof(long):
                            FastCore<long>.ReplaceLessThan((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceLessThanOrEquals(nint* ptr, nuint length, nint filter, nint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(int):
                    FastCore<int>.ReplaceLessThanOrEquals((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                    return;
                case sizeof(long):
                    FastCore<long>.ReplaceLessThanOrEquals((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(int):
                            FastCore<int>.ReplaceLessThanOrEquals((int*)ptr, length, *(int*)&filter, *(int*)&replacement);
                            return;
                        case sizeof(long):
                            FastCore<long>.ReplaceLessThanOrEquals((long*)ptr, length, *(long*)&filter, *(long*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void Replace(nuint* ptr, nuint length, nuint filter, nuint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(uint):
                    FastCore<uint>.Replace((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                    return;
                case sizeof(ulong):
                    FastCore<ulong>.Replace((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(uint):
                            FastCore<uint>.Replace((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                            return;
                        case sizeof(ulong):
                            FastCore<ulong>.Replace((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceExclude(nuint* ptr, nuint length, nuint filter, nuint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(uint):
                    FastCore<uint>.ReplaceExclude((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                    return;
                case sizeof(ulong):
                    FastCore<ulong>.ReplaceExclude((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(uint):
                            FastCore<uint>.ReplaceExclude((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                            return;
                        case sizeof(ulong):
                            FastCore<ulong>.ReplaceExclude((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceGreaterThan(nuint* ptr, nuint length, nuint filter, nuint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(uint):
                    FastCore<uint>.ReplaceGreaterThan((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                    return;
                case sizeof(ulong):
                    FastCore<ulong>.ReplaceGreaterThan((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(uint):
                            FastCore<uint>.ReplaceGreaterThan((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                            return;
                        case sizeof(ulong):
                            FastCore<ulong>.ReplaceGreaterThan((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceGreaterThanOrEquals(nuint* ptr, nuint length, nuint filter, nuint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(uint):
                    FastCore<uint>.ReplaceGreaterThanOrEquals((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                    return;
                case sizeof(ulong):
                    FastCore<ulong>.ReplaceGreaterThanOrEquals((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(uint):
                            FastCore<uint>.ReplaceGreaterThanOrEquals((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                            return;
                        case sizeof(ulong):
                            FastCore<ulong>.ReplaceGreaterThanOrEquals((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceLessThan(nuint* ptr, nuint length, nuint filter, nuint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(uint):
                    FastCore<uint>.ReplaceLessThan((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                    return;
                case sizeof(ulong):
                    FastCore<ulong>.ReplaceLessThan((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(uint):
                            FastCore<uint>.ReplaceLessThan((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                            return;
                        case sizeof(ulong):
                            FastCore<ulong>.ReplaceLessThan((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public static void ReplaceLessThanOrEquals(nuint* ptr, nuint length, nuint filter, nuint replacement)
        {
            switch (UnsafeHelper.PointerSizeConstant)
            {
                case sizeof(uint):
                    FastCore<uint>.ReplaceLessThanOrEquals((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                    return;
                case sizeof(ulong):
                    FastCore<ulong>.ReplaceLessThanOrEquals((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                    return;
                case UnsafeHelper.PointerSizeConstant_Indeterminate:
                    switch (UnsafeHelper.PointerSize)
                    {
                        case sizeof(uint):
                            FastCore<uint>.ReplaceLessThanOrEquals((uint*)ptr, length, *(uint*)&filter, *(uint*)&replacement);
                            return;
                        case sizeof(ulong):
                            FastCore<ulong>.ReplaceLessThanOrEquals((ulong*)ptr, length, *(ulong*)&filter, *(ulong*)&replacement);
                            return;
                        default:
                            break;
                    }
                    goto default;
                default:
                    throw new NotImplementedException();
            }
        }
    }
#pragma warning restore CS0162

    unsafe partial class FastCore<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.Include);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReplaceExclude(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.Exclude);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReplaceGreaterThan(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.GreaterThan);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReplaceLessThan(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.LessThan);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReplaceGreaterThanOrEquals(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.GreaterThanOrEquals);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReplaceLessThanOrEquals(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.LessThanOrEquals);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void VectorizedReplace(T* ptr, nuint length, T filter, T replacement)
            => VectorizedReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.Include);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void VectorizedReplaceExclude(T* ptr, nuint length, T filter, T replacement)
            => VectorizedReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.Exclude);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void VectorizedReplaceGreaterThan(T* ptr, nuint length, T filter, T replacement)
            => VectorizedReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.GreaterThan);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void VectorizedReplaceLessThan(T* ptr, nuint length, T filter, T replacement)
            => VectorizedReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.LessThan);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void VectorizedReplaceGreaterThanOrEquals(T* ptr, nuint length, T filter, T replacement)
            => VectorizedReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.GreaterThanOrEquals);

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void VectorizedReplaceLessThanOrEquals(T* ptr, nuint length, T filter, T replacement)
            => VectorizedReplaceCore(ref ptr, ref length, filter, replacement, CompareMethod.LessThanOrEquals);

        [Inline(InlineBehavior.Remove)]
        private static void ReplaceCore(ref T* ptr, ref nuint length, T filter, T replacement, [InlineParameter] CompareMethod method)
        {
            if (Limits.CheckTypeCanBeVectorized<T>() && length > Limits.GetLimitForVectorizing<T>())
            {
                switch (method)
                {
                    case CompareMethod.Include:
                        VectorizedReplace(ptr, length, filter, replacement);
                        break;
                    case CompareMethod.Exclude:
                        VectorizedReplaceExclude(ptr, length, filter, replacement);
                        break;
                    case CompareMethod.GreaterThan:
                        VectorizedReplaceGreaterThan(ptr, length, filter, replacement);
                        break;
                    case CompareMethod.GreaterThanOrEquals:
                        VectorizedReplaceGreaterThanOrEquals(ptr, length, filter, replacement);
                        break;
                    case CompareMethod.LessThan:
                        VectorizedReplaceLessThan(ptr, length, filter, replacement);
                        break;
                    case CompareMethod.LessThanOrEquals:
                        VectorizedReplaceLessThanOrEquals(ptr, length, filter, replacement);
                        break;
                    default:
                        ArgumentOutOfRangeException.Throw(nameof(method));
                        break;
                }
            }

            ScalarizedReplaceCore(ref ptr, ref length, filter, replacement, method);
        }

        [Inline(InlineBehavior.Remove)]
        private static partial void VectorizedReplaceCore(ref T* ptr, ref nuint length, T filter, T replacement, [InlineParameter] CompareMethod method);

        [Inline(InlineBehavior.Remove)]
        private static void ScalarizedReplaceCore(ref T* ptr, ref nuint length, T filter, T replacement, [InlineParameter] CompareMethod method)
        {
            for (; length >= 4; length -= 4, ptr += 4) // 4x 展開
            {
                if (ScalarizedCompare(ptr[0], filter, method))
                    ptr[0] = replacement;
                if (ScalarizedCompare(ptr[1], filter, method))
                    ptr[1] = replacement;
                if (ScalarizedCompare(ptr[2], filter, method))
                    ptr[2] = replacement;
                if (ScalarizedCompare(ptr[3], filter, method))
                    ptr[3] = replacement;
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                return;
            if (ScalarizedCompare(*ptr, filter, method))
                *ptr = replacement;
            ptr++;
            if (ptr >= ptrEnd)
                return;
            if (ScalarizedCompare(*ptr, filter, method))
                *ptr = replacement;
            ptr++;
            if (ptr >= ptrEnd)
                return;
            if (ScalarizedCompare(*ptr, filter, method))
                *ptr = replacement;
        }
    }

    unsafe partial class SlowCore<T>
    {
        public static void Replace(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, length, filter, replacement, CompareMethod.Include);

        public static void ReplaceExclude(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, length, filter, replacement, CompareMethod.Exclude);

        public static void ReplaceGreaterThan(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, length, filter, replacement, CompareMethod.GreaterThan);

        public static void ReplaceLessThan(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, length, filter, replacement, CompareMethod.LessThan);

        public static void ReplaceGreaterThanOrEquals(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, length, filter, replacement, CompareMethod.GreaterThanOrEquals);

        public static void ReplaceLessThanOrEquals(T* ptr, nuint length, T filter, T replacement)
            => ReplaceCore(ref ptr, length, filter, replacement, CompareMethod.LessThanOrEquals);

        [Inline(InlineBehavior.Remove)]
        private static void ReplaceCore(ref T* ptr, nuint length, T filter, T replacement, [InlineParameter] CompareMethod method)
        {
            if (method == CompareMethod.Include || method == CompareMethod.Exclude)
            {
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                for (nuint i = 0; i < length; i++, ptr++)
                {
                    if (Compare(comparer, *ptr, filter, method))
                        *ptr = replacement;
                }
            }
            else
            {
                Comparer<T> comparer = Comparer<T>.Default;
                for (nuint i = 0; i < length; i++, ptr++)
                {
                    if (Compare(comparer, *ptr, filter, method))
                        *ptr = replacement;
                }
            }
        }
    }
}
