using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Text;

partial class StringWrapper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringWrapper Create(string str) => Create(str, RTCore.StringCreateOptions);

    public static unsafe StringWrapper Create(string str, StringCreateOptions options)
    {
        int length = str.Length;
        if (length <= 0)
            return Empty;

        string? internedString = string.IsInterned(str);
        if (internedString is null)
            goto DoCompression;

        if ((options & StringCreateOptions._Force_Flag) == StringCreateOptions._Force_Flag)
        {
            str = internedString;
            goto DoCompression;
        }

        goto JustWrap;

    DoCompression:
        if ((options & StringCreateOptions.UseAsciiCompression) == StringCreateOptions.UseAsciiCompression)
        {
            fixed (char* ptr = str)
            {
                if (AsciiString.TryCreate(ptr, unchecked((nuint)length), options, out AsciiString? asciiString))
                    return asciiString;
            }
        }
        if ((options & StringCreateOptions.UseLatin1Compression) == StringCreateOptions.UseLatin1Compression)
        {
            fixed (char* ptr = str)
            {
                if (Latin1String.TryCreate(ptr, unchecked((nuint)length), options, out StringWrapper? latin1String))
                    return latin1String;
            }
        }
        if ((options & StringCreateOptions.UseUtf8Compression) == StringCreateOptions.UseUtf8Compression)
        {
            fixed (char* ptr = str)
            {
                if (Utf8String.TryCreate(ptr, unchecked((nuint)length), options, out StringWrapper? utf8String))
                    return utf8String;
            }
        }

    JustWrap:
        return Utf16String.Create(str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper Create(char* ptr) => Create(ptr, RTCore.StringCreateOptions);

    public static unsafe StringWrapper Create(char* ptr, StringCreateOptions options)
    {
        nuint length = FindLength(ptr);
        if (length == 0)
            return Empty;

        return CreateCoreUnsafe(ptr, length, options);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper Create(char* ptr, int startIndex, int count) => Create(ptr, startIndex, count, RTCore.StringCreateOptions);

    public static unsafe StringWrapper Create(char* ptr, int startIndex, int count, StringCreateOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0)
            return Empty;

        return CreateCore(ptr + startIndex, unchecked((nuint)count), options);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper Create(char* ptr, nuint startIndex, nuint count) => Create(ptr, startIndex, count, RTCore.StringCreateOptions);

    public static unsafe StringWrapper Create(char* ptr, nuint startIndex, nuint count, StringCreateOptions options)
    {
        if (count == 0)
            return Empty;

        return CreateCore(ptr + startIndex, count, options);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe StringWrapper CreateCore(char* ptr, nuint length, StringCreateOptions options)
    {
        if (length > unchecked((nuint)Limits.MaxStringLength))
            OutOfMemoryException.Throw();

        return CreateCoreUnsafe(ptr, length, options);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe StringWrapper CreateCoreUnsafe(char* ptr, nuint length, StringCreateOptions options)
    {
        if ((options & StringCreateOptions.UseAsciiCompression) == StringCreateOptions.UseAsciiCompression)
        {
            if (AsciiString.TryCreate(ptr, length, options, out AsciiString? asciiString))
                return asciiString;
        }
        if ((options & StringCreateOptions.UseLatin1Compression) == StringCreateOptions.UseLatin1Compression)
        {
            if (Latin1String.TryCreate(ptr, length, options, out StringWrapper? latin1String))
                return latin1String;
        }
        if ((options & StringCreateOptions.UseUtf8Compression) == StringCreateOptions.UseUtf8Compression)
        {
            if (Utf8String.TryCreate(ptr, length, options, out StringWrapper? utf8String))
                return utf8String;
        }
        return Utf16String.Create(ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringWrapper CreateUtf16String(string str)
    {
        int length = str.Length;
        if (length <= 0)
            return Empty;
        return Utf16String.Create(str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringWrapper CreateUtf16String(char character, int length)
    {
        if (length <= 0)
            return Empty;
        return Utf16String.Create(character, (nuint)length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringWrapper CreateUtf16String(char character, nuint length)
    {
        if (length <= 0)
            return Empty;
        return Utf16String.Create(character, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateUtf16String(char* ptr)
    {
        nuint length = FindLength(ptr);
        if (length == 0)
            return Empty;
        return Utf16String.Create(ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateUtf16String(char* ptr, int startIndex, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return CreateUtf16String(ptr, unchecked((nuint)startIndex), unchecked((nuint)count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateUtf16String(char* ptr, nuint startIndex, nuint count)
    {
        if (count == 0)
            return Empty;
        return Utf16String.Create(ptr + startIndex, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringWrapper CreateLatin1String(byte character, int length)
    {
        if (length <= 0)
            return Empty;
        return Latin1String.Create(character, (nuint)length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringWrapper CreateLatin1String(byte character, nuint length)
    {
        if (length <= 0)
            return Empty;
        return Latin1String.Create(character, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateLatin1String(byte* ptr)
    {
        nuint length = FindLength(ptr);
        if (length == 0)
            return Empty;
        return Latin1String.Create(ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateLatin1String(byte* ptr, int startIndex, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return CreateLatin1String(ptr, unchecked((nuint)startIndex), unchecked((nuint)count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateLatin1String(byte* ptr, nuint startIndex, nuint count)
    {
        if (count == 0)
            return Empty;
        return Latin1String.Create(ptr + startIndex, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringWrapper CreateAsciiString(byte character, int length)
    {
        if (length <= 0)
            return Empty;
        return AsciiString.Create(character, (nuint)length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringWrapper CreateAsciiString(byte character, nuint length)
    {
        if (length <= 0)
            return Empty;
        return AsciiString.Create(character, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateAsciiString(byte* ptr)
    {
        nuint length = FindLength(ptr);
        if (length == 0)
            return Empty;
        return AsciiString.Create(ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateAsciiString(byte* ptr, int startIndex, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return CreateAsciiString(ptr, unchecked((nuint)startIndex), unchecked((nuint)count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateAsciiString(byte* ptr, nuint startIndex, nuint count)
    {
        if (count == 0)
            return Empty;
        return AsciiString.Create(ptr + startIndex, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateUtf8String(byte* ptr)
    {
        nuint length = FindLength(ptr);
        if (length == 0)
            return Empty;
        return Utf8String.Create(ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateUtf8String(byte* ptr, int startIndex, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return CreateUtf8String(ptr, unchecked((nuint)startIndex), unchecked((nuint)count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper CreateUtf8String(byte* ptr, nuint startIndex, nuint count)
    {
        if (count == 0)
            return Empty;
        return Utf8String.Create(ptr + startIndex, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] StringWrapper? str) => str is null || str.Length <= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] StringWrapper? str) => str is null || str.IsFullyWhitespaced();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe nuint FindLength<T>(T* ptr) where T : unmanaged
    {
        if (UnsafeHelper.Equals(*ptr, default))
            return 0;
        nuint limit = unchecked((nuint)Limits.MaxStringLength);
        for (nuint i = 1; i < limit; i++)
        {
            if (UnsafeHelper.Equals(ptr[i], default))
                return i;
        }
        return limit;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(str))]
    public static implicit operator StringWrapper?(string? str) => str is null ? null : CreateUtf16String(str);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(str))]
    public static explicit operator string?(StringWrapper? str) => str?.ToString();

    public static bool operator ==(StringWrapper? left, StringWrapper? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(StringWrapper? left, StringWrapper? right)
    {
        if (ReferenceEquals(left, right))
            return false;
        if (left is null || right is null)
            return true;
        return !left.Equals(right);
    }
}
