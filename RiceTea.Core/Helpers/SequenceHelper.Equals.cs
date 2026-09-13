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
    public static bool Equals(string? a, string? b, int length, StringComparison comparisonType)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        return comparisonType switch
        {
            StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.Compare(a, 0, length, b, 0, length, CompareOptions.None) == 0,
            StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.Compare(a, 0, length, b, 0, length, CompareOptions.IgnoreCase) == 0,
            StringComparison.InvariantCulture => CultureInfo.InvariantCulture.CompareInfo.Compare(a, 0, length, b, 0, length, CompareOptions.None) == 0,
            StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture.CompareInfo.Compare(a, 0, length, b, 0, length, CompareOptions.IgnoreCase) == 0,
            StringComparison.OrdinalIgnoreCase => EqualsIgnoreCaseCore(a, b, length),
            StringComparison.Ordinal => EqualsCore(a, b, length),
            _ => ArgumentOutOfRangeException.Throw<bool>(nameof(comparisonType))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(string? a, int indexA, string? b, int indexB, int length, StringComparison comparisonType)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        return comparisonType switch
        {
            StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.Compare(a, indexA, length, b, indexB, length, CompareOptions.None) == 0,
            StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.Compare(a, indexA, length, b, indexB, length, CompareOptions.IgnoreCase) == 0,
            StringComparison.InvariantCulture => CultureInfo.InvariantCulture.CompareInfo.Compare(a, indexA, length, b, indexB, length, CompareOptions.None) == 0,
            StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture.CompareInfo.Compare(a, indexA, length, b, indexB, length, CompareOptions.IgnoreCase) == 0,
            StringComparison.OrdinalIgnoreCase => EqualsIgnoreCaseCore(a, indexA, b, indexB, length),
            StringComparison.Ordinal => EqualsCore(a, indexA, b, indexB, length),
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
    private static bool EqualsCore(string a, string b)
    {
        int length = a.Length;
        if (length != b.Length)
            return false;
        fixed (char* ptr = a, ptr2 = b)
            return EqualsCore<char>(ptr, ptr2, MathHelper.MakeUnsigned(length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsCore(string a, string b, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, a.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, b.Length);

        fixed (char* ptr = a, ptr2 = b)
            return EqualsCore<char>(ptr, ptr2, MathHelper.MakeUnsigned(length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsCore(string a, int indexA, string b, int indexB, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(indexA);
        ArgumentOutOfRangeException.ThrowIfNegative(indexB);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (indexA + length > a.Length)
            ArgumentOutOfRangeException.Throw(indexA >= a.Length ? nameof(indexA) : nameof(length));
        if (indexB + length > b.Length)
            ArgumentOutOfRangeException.Throw(indexB >= b.Length ? nameof(indexB) : nameof(length));

        fixed (char* ptr = a, ptr2 = b)
            return EqualsCore<char>(ptr, ptr2, MathHelper.MakeUnsigned(length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsIgnoreCaseCore(string a, string b)
    {
        int length = a.Length;
        if (length != b.Length)
            return false;
        if (ContainsGreaterThan(a, '\u007F'))
            return ContainsGreaterThan(b, '\u007F') && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        if (ContainsGreaterThan(b, '\u007F'))
            return false;
        fixed (char* ptr = a, ptr2 = b)
            return FastCore<ushort>.RangedAddAndEquals((ushort*)ptr, (ushort*)ptr2, MathHelper.MakeUnsigned(length), 'A', 'Z', 'a' - 'A');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsIgnoreCaseCore(string a, string b, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, a.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, b.Length);

        if (ContainsGreaterThan(a, '\u007F'))
            return ContainsGreaterThan(b, '\u007F') && string.Compare(a, 0, b, 0, length, StringComparison.OrdinalIgnoreCase) == 0;
        if (ContainsGreaterThan(b, '\u007F'))
            return false;
        fixed (char* ptr = a, ptr2 = b)
            return FastCore<ushort>.RangedAddAndEquals((ushort*)ptr, (ushort*)ptr2, (uint)length, 'A', 'Z', 'a' - 'A');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsIgnoreCaseCore(string a, int indexA, string b, int indexB, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(indexA);
        ArgumentOutOfRangeException.ThrowIfNegative(indexB);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (indexA + length > a.Length)
            ArgumentOutOfRangeException.Throw(indexA >= a.Length ? nameof(indexA) : nameof(length));
        if (indexB + length > b.Length)
            ArgumentOutOfRangeException.Throw(indexB >= b.Length ? nameof(indexB) : nameof(length));

        if (ContainsGreaterThan(a, '\u007F'))
            return ContainsGreaterThan(b, '\u007F') && string.Compare(a, indexA, b, indexB, length, StringComparison.OrdinalIgnoreCase) == 0;
        if (ContainsGreaterThan(b, '\u007F'))
            return false;
        fixed (char* ptr = a, ptr2 = b)
            return FastCore<ushort>.RangedAddAndEquals((ushort*)ptr + indexA, (ushort*)ptr2 + indexB, (uint)length, 'A', 'Z', 'a' - 'A');
    }

    [Inline(InlineBehavior.Remove)]
    private static bool EqualsCore(byte* ptr, byte* ptrEnd, byte* ptr2) => FastCore.Equals(ptr, ptr2, unchecked((nuint)(ptrEnd - ptr)));

    [Inline(InlineBehavior.Remove)]
    private static bool EqualsCore(void* ptr, void* ptr2, nuint length) => FastCore.Equals((byte*)ptr, (byte*)ptr2, length);

    [Inline(InlineBehavior.Remove)]
    private static bool EqualsCore<T>(void* ptr, void* ptr2, nuint length) => FastCore.Equals((byte*)ptr, (byte*)ptr2, length * UnsafeHelper.SizeOf<T>());
}
