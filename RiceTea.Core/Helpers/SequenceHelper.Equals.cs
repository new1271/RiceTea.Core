using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

using InlineMethod;

namespace RiceTea.Core.Helpers;

#pragma warning disable CS8500

unsafe partial class SequenceHelper
{
    /// <inheritdoc cref="string.Equals(string?, string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(string? a, string? b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        return EqualsCore(a, b);
    }

    /// <inheritdoc cref="string.Equals(string?, string?, StringComparison)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(string? a, string? b, StringComparison comparisonType)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        return comparisonType switch
        {
            StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0,
            StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0,
            StringComparison.InvariantCulture => CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0,
            StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0,
            StringComparison.OrdinalIgnoreCase => EqualsIgnoreCaseCore(a, b),
            StringComparison.Ordinal => EqualsCore(a, b),
            _ => ArgumentOutOfRangeException.Throw<bool>(nameof(comparisonType))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals<T>(T[]? a, T[]? b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        int length = a.Length;
        if (length != b.Length)
            return false;
        if (UnsafeHelper.IsPrimitiveType<T>())
        {
            fixed (T* ptr = a, ptr2 = b)
                return EqualsCore<T>(ptr, ptr2, MathHelper.MakeUnsigned(length));
        }
        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < length; i++)
        {
            if (!comparer.Equals(a[i], b[i]))
                return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(void* ptrStart, void* ptrEnd, void* ptr2)
    {
        if (ptrStart == ptr2 || ptrEnd <= ptrStart)
            return true;
        return EqualsCore((byte*)ptrStart, (byte*)ptrEnd, (byte*)ptr2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(void* ptr, void* ptr2, int length)
    {
        if (ptr == ptr2 || length <= 0)
            return true;
        return EqualsCore(ptr, ptr2, unchecked((nuint)length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(2)]
    public static bool Equals(void* ptr, void* ptr2, nuint length)
        => EqualsCore(ptr, ptr2, length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(1)]
    public static bool Equals<T>(T* ptr, T* ptr2, nuint length)
        => EqualsCore<T>(ptr, ptr2, length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsCore(string str1, string str2)
    {
        int length = str1.Length;
        if (length != str2.Length)
            return false;
        fixed (char* ptr = str1, ptr2 = str2)
            return EqualsCore<char>(ptr, ptr2, MathHelper.MakeUnsigned(length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsIgnoreCaseCore(string str1, string str2)
    {
        int length = str1.Length;
        if (length != str2.Length)
            return false;
        if (ContainsGreaterThan(str1, '\u007F'))
            return ContainsGreaterThan(str2, '\u007F') && string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        if (ContainsGreaterThan(str2, '\u007F'))
            return false;
        fixed (char* ptr = str1, ptr2 = str2)
            return FastCore<ushort>.RangedAddAndEquals((ushort*)ptr, (ushort*)ptr2, MathHelper.MakeUnsigned(length), 'A', 'Z', 'a' - 'A');
    }

    [Inline(InlineBehavior.Remove)]
    private static bool EqualsCore(byte* ptr, byte* ptrEnd, byte* ptr2) => FastCore.Equals(ptr, ptr2, unchecked((nuint)(ptrEnd - ptr)));

    [Inline(InlineBehavior.Remove)]
    private static bool EqualsCore(void* ptr, void* ptr2, nuint length) => FastCore.Equals((byte*)ptr, (byte*)ptr2, length);

    [Inline(InlineBehavior.Remove)]
    private static bool EqualsCore<T>(void* ptr, void* ptr2, nuint length) => FastCore.Equals((byte*)ptr, (byte*)ptr2, length * UnsafeHelper.SizeOf<T>());
}
