using System.Collections.Generic;
using System.Runtime.CompilerServices;

using InlineMethod;

namespace RiceTea.Core.Helpers;

#pragma warning disable CS8500
partial class SequenceHelper
{
    unsafe partial class FastCore
    {
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(byte* ptr, byte* ptr2, nuint length)
        {
            if (length > Limits.GetLimitForVectorizing<byte>())
                return VectorizedEquals(ptr, ptr2, length);
            return ScalarizedEquals(ref ptr, ref ptr2, ref length);
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static partial bool VectorizedEquals(byte* ptr, byte* ptr2, nuint length);

        [Inline(InlineBehavior.Remove)]
        private static bool ScalarizedEquals(ref byte* ptr, ref byte* ptr2, ref nuint length)
        {
            if (length >= (nuint)sizeof(nuint))
                return ScalarizedBulkEquals<nuint>(ref ptr, ref ptr2, ref length);
            if (sizeof(nuint) > sizeof(uint) && length >= sizeof(uint))
                return ScalarizedBulkEquals<uint>(ref ptr, ref ptr2, ref length);
            byte* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                return true;
            if (*ptr++ != *ptr2++)
                return false;
            if (ptr >= ptrEnd)
                return true;
            if (*ptr++ != *ptr2++)
                return false;
            if (ptr >= ptrEnd)
                return true;
            return *ptr == *ptr2;
        }

        [Inline(InlineBehavior.Remove)]
        private static bool ScalarizedBulkEquals<T>(ref byte* ptr, ref byte* ptr2, ref nuint length) where T : unmanaged
        {
            for (; length >= UnsafeHelper.SizeOf<T>(); length -= UnsafeHelper.SizeOf<T>(),
                ptr += UnsafeHelper.SizeOf<T>(), ptr2 += UnsafeHelper.SizeOf<T>()) // SWAR-Native 展開
            {
                if (UnsafeHelper.NotEquals(*(T*)ptr, *(T*)ptr2))
                    return false;
            }
            if (length == 0)
                return true;
            ptr -= UnsafeHelper.SizeOf<T>() - length;
            ptr2 -= UnsafeHelper.SizeOf<T>() - length;
            return UnsafeHelper.Equals(*(T*)ptr, *(T*)ptr2);
        }
    }

    unsafe partial class FastCore<T>
    {
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RangedAddAndEquals(T* ptr, T* ptr2, nuint length, T lowerBound, T higherBound, T valueToAddInRange)
        {
            if (Limits.CheckTypeCanBeVectorized<T>() && length > Limits.GetLimitForVectorizing<T>())
                return VectorizedRangedAddAndEquals(ptr, ptr2, length, lowerBound, higherBound, valueToAddInRange);
            return ScalarizedRangedAddAndEquals(ref ptr, ref ptr2, ref length, lowerBound, higherBound, valueToAddInRange);
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static partial bool VectorizedRangedAddAndEquals(T* ptr, T* ptr2, nuint length, T lowerBound, T higherBound, T valueToAddInRange);

        [Inline(InlineBehavior.Remove)]
        private static bool ScalarizedRangedAddAndEquals(ref T* ptr, ref T* ptr2, ref nuint length, T lowerBound, T higherBound, T valueToAddInRange)
        {
            for (; length >= 4; length -= 4, ptr += 4, ptr2 += 4) // 4x 展開
            {
                if (UnsafeHelper.NotEquals(
                    a: ScalarRangedAdd(ptr[0], valueToAddInRange, lowerBound, higherBound),
                    b: ScalarRangedAdd(ptr2[0], valueToAddInRange, lowerBound, higherBound)))
                    return false;
                if (UnsafeHelper.NotEquals(
                    a: ScalarRangedAdd(ptr[1], valueToAddInRange, lowerBound, higherBound),
                    b: ScalarRangedAdd(ptr2[1], valueToAddInRange, lowerBound, higherBound)))
                    return false;
                if (UnsafeHelper.NotEquals(
                    a: ScalarRangedAdd(ptr[2], valueToAddInRange, lowerBound, higherBound),
                    b: ScalarRangedAdd(ptr2[2], valueToAddInRange, lowerBound, higherBound)))
                    return false;
                if (UnsafeHelper.NotEquals(
                    a: ScalarRangedAdd(ptr[3], valueToAddInRange, lowerBound, higherBound),
                    b: ScalarRangedAdd(ptr2[3], valueToAddInRange, lowerBound, higherBound)))
                    return false;
            }
            T* ptrEnd = ptr + length;
            if (ptr >= ptrEnd)
                return true;
            if (UnsafeHelper.NotEquals(
                a: ScalarRangedAdd(*ptr, valueToAddInRange, lowerBound, higherBound),
                b: ScalarRangedAdd(*ptr2, valueToAddInRange, lowerBound, higherBound)))
                return false;
            ptr++;
            ptr2++;
            if (ptr >= ptrEnd)
                return true;
            if (UnsafeHelper.NotEquals(
                a: ScalarRangedAdd(*ptr, valueToAddInRange, lowerBound, higherBound),
                b: ScalarRangedAdd(*ptr2, valueToAddInRange, lowerBound, higherBound)))
                return false;
            ptr++;
            ptr2++;
            if (ptr >= ptrEnd)
                return true;
            return UnsafeHelper.Equals(
                a: ScalarRangedAdd(*ptr, valueToAddInRange, lowerBound, higherBound),
                b: ScalarRangedAdd(*ptr2, valueToAddInRange, lowerBound, higherBound));
        }

        [Inline(InlineBehavior.Remove)]
        private static T ScalarRangedAdd(T source, T valueToAdd, T lowerBound, T higherBound)
        {
            if (IsGreaterOrEquals(source, lowerBound) && IsLessOrEquals(source, higherBound))
                return UnsafeHelper.Add(source, valueToAdd);
            return source;
        }

        [Inline(InlineBehavior.Remove)]
        private static bool IsGreaterOrEquals(T a, T b)
        {
            if (UnsafeHelper.IsUnsignedIntegerType<T>())
                return UnsafeHelper.IsGreaterThanOrEqualsUnsigned(a, b);
            return UnsafeHelper.IsGreaterThanOrEquals(a, b);
        }

        [Inline(InlineBehavior.Remove)]
        private static bool IsLessOrEquals(T a, T b)
        {
            if (UnsafeHelper.IsUnsignedIntegerType<T>())
                return UnsafeHelper.IsLessThanOrEqualsUnsigned(a, b);
            return UnsafeHelper.IsLessThanOrEquals(a, b);
        }
    }
}

unsafe partial class SlowCore<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(T* ptr, T* ptr2, nuint length)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (nuint i = 0; i < length; i++)
        {
            if (comparer.Equals(ptr[i], ptr2[i]))
                continue;
            return false;
        }
        return true;
    }
}
