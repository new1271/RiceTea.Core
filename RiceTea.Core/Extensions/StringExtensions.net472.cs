#if NET472_OR_GREATER
using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;
using RiceTea.Core.Text;

namespace RiceTea.Core.Extensions;

public static partial class StringExtensions
{
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains(this string obj, char value) => StringHelper.Contains(obj, value);

    private const int SplitPathLength = 256;

    /// <inheritdoc cref="string.Split(char[])"/>
    [Inline(InlineBehavior.Remove)]
    public static string[] Split(this string _this, char separator) => Split(_this, separator, StringSplitOptions.None);

    /// <inheritdoc cref="string.Split(char[], StringSplitOptions)"/>>
    public static unsafe string[] Split(this string _this, char separator, StringSplitOptions options)
    {
        fixed (char* ptr = _this)
            return SplitCore(_this, ptr, _this.Length, separator, options);
    }

    /// <inheritdoc cref="string.Split(string[])"/>
    [Inline(InlineBehavior.Remove)]
    public static string[] Split(this string _this, string separator) => Split(_this, separator, StringSplitOptions.None);

    /// <inheritdoc cref="string.Split(char[], StringSplitOptions)"/>>
    public static unsafe string[] Split(this string _this, string separator, StringSplitOptions options)
    {
        fixed (char* ptr = _this, ptrSeparator = separator)
            return SplitCore(_this, ptr, _this.Length, ptrSeparator, separator.Length, options);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe string[] SplitCore(string _this, char* ptr, int length, char separator, StringSplitOptions options)
    {
        char** paths = stackalloc char*[SplitPathLength];

        if (length <= 0)
            return NeedRemoveEmptyEntries(options) ? Array.Empty<string>() : [_this];

        char* ptrEnd = ptr + length;
        nuint count = GetSplitCount(ptr, ptrEnd, paths, separator);
        if (count == 0)
            return [_this];
        if ((options & StringSplitOptions.RemoveEmptyEntries) == StringSplitOptions.RemoveEmptyEntries)
        {
            ArrayPool<string?> pool = ArrayPool<string?>.Shared;
            string?[] buffer = pool.Rent(count + 1);
            if (count > SplitPathLength)
                CopySplitStringIntoBuffer(ptr, ptrEnd, paths, separator, buffer);
            else
                CopySplitStringIntoBuffer(ptr, ptrEnd, paths, count, separatorLength: 1u, buffer);
            return CollectAndRestoreBuffer(pool, buffer, count);
        }
        string[] result = new string[count + 1];
        if (count > SplitPathLength)
            CopySplitStringIntoBuffer(ptr, ptrEnd, paths, separator, result);
        else
            CopySplitStringIntoBuffer(ptr, ptrEnd, paths, count, separatorLength: 1u, result);
        return result;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe string[] SplitCore(string _this, char* ptr, int length, char* separator, int separatorLength, StringSplitOptions options)
    {
        char** paths = stackalloc char*[SplitPathLength];

        if (length <= 0)
            return NeedRemoveEmptyEntries(options) ? Array.Empty<string>() : [_this];
        if (separatorLength < 0)
            return [_this];

        char* ptrEnd = ptr + length;
        nuint castedSeparatorLength = unchecked((nuint)separatorLength);
        nuint count = GetSplitCount(ptr, ptrEnd, paths, separator, castedSeparatorLength);
        if (count == 0)
            return [_this];
        if ((options & StringSplitOptions.RemoveEmptyEntries) == StringSplitOptions.RemoveEmptyEntries)
        {
            ArrayPool<string?> pool = ArrayPool<string?>.Shared;
            string?[] buffer = pool.Rent(count + 1);
            if (count > SplitPathLength)
                CopySplitStringIntoBuffer(ptr, ptrEnd, paths, separator, castedSeparatorLength, buffer);
            else
                CopySplitStringIntoBuffer(ptr, ptrEnd, paths, count, castedSeparatorLength, buffer);
            return CollectAndRestoreBuffer(pool, buffer, count);
        }
        string[] result = new string[count + 1];
        if (count > SplitPathLength)
            CopySplitStringIntoBuffer(ptr, ptrEnd, paths, separator, castedSeparatorLength, result);
        else
            CopySplitStringIntoBuffer(ptr, ptrEnd, paths, count, castedSeparatorLength, result);
        return result;
    }

    [Inline(InlineBehavior.Remove)]
    private static unsafe nuint GetSplitCount(char* ptr, char* ptrEnd, char** paths, char separator)
    {
        nuint i = 0;
        while ((ptr = SequenceHelper.PointerIndexOf(ptr, ptrEnd, separator)) != null)
        {
            paths[i++] = ptr;
            ptr++;
            if (i >= SplitPathLength)
            {
                CollectSplitCount(ref i, ref ptr, ptrEnd, separator);
                break;
            }
        }
        return i;
    }

    [Inline(InlineBehavior.Remove)]
    private static unsafe nuint GetSplitCount(char* ptr, char* ptrEnd, char** paths, char* separator, nuint separatorLength)
    {
        nuint i = 0;
        while ((ptr = InternalSequenceHelper.PointerIndexOf(ptr, ptrEnd, separator, separatorLength)) != null)
        {
            paths[i++] = ptr;
            ptr += separatorLength;
            if (i >= SplitPathLength)
            {
                CollectSplitCount(ref i, ref ptr, ptrEnd, separator, separatorLength);
                break;
            }
        }
        return i;
    }

    [Inline(InlineBehavior.Remove)]
    private static unsafe void CollectSplitCount(ref nuint i, ref char* ptr, char* ptrEnd, char separator)
    {
        while ((ptr = SequenceHelper.PointerIndexOf(ptr, ptrEnd, separator)) != null)
        {
            i++;
            ptr++;
        }
    }

    [Inline(InlineBehavior.Remove)]
    private static unsafe void CollectSplitCount(ref nuint i, ref char* ptr, char* ptrEnd, char* separator, nuint separatorLength)
    {
        while ((ptr = InternalSequenceHelper.PointerIndexOf(ptr, ptrEnd, separator, separatorLength)) != null)
        {
            i++;
            ptr += separatorLength;
        }
    }

    private static unsafe void CopySplitStringIntoBuffer(char* ptr, char* ptrEnd, char** paths, nuint pathCount, nuint separatorLength, string?[] buffer)
    {
        for (nuint i = 0; i < pathCount; i++)
        {
            char* path = paths[i];
            buffer[i] = new string(ptr, 0, unchecked((int)(path - ptr)));
            ptr = path + separatorLength;
        }
        buffer[pathCount] = new string(ptr, 0, unchecked((int)(ptrEnd - ptr)));
    }

    private static unsafe void CopySplitStringIntoBuffer(char* ptr, char* ptrEnd, char** paths, char separator, string?[] buffer)
    {
        nuint i;
        for (i = 0; i < SplitPathLength; i++)
        {
            char* path = paths[i];
            buffer[i] = new string(ptr, 0, unchecked((int)(path - ptr)));
            ptr = path + 1;
        }
        char* splitEnd;
        while ((splitEnd = SequenceHelper.PointerIndexOf(ptr, ptrEnd, separator)) != null)
        {
            buffer[i++] = new string(ptr, 0, unchecked((int)(splitEnd - ptr)));
            ptr = splitEnd + 1;
        }
        buffer[i] = new string(ptr, 0, unchecked((int)(ptrEnd - ptr)));
    }

    private static unsafe void CopySplitStringIntoBuffer(char* ptr, char* ptrEnd, char** paths, char* separator, nuint separatorLength, string?[] buffer)
    {
        nuint i;
        for (i = 0; i < SplitPathLength; i++)
        {
            char* path = paths[i];
            buffer[i] = new string(ptr, 0, unchecked((int)(path - ptr)));
            ptr = path + separatorLength;
        }
        char* splitEnd;
        while ((splitEnd = InternalSequenceHelper.PointerIndexOf(ptr, ptrEnd, separator, separatorLength)) != null)
        {
            buffer[i++] = new string(ptr, 0, unchecked((int)(splitEnd - ptr)));
            ptr = splitEnd + separatorLength;
        }
        buffer[i] = new string(ptr, 0, unchecked((int)(ptrEnd - ptr)));
    }

    private static string[] CollectAndRestoreBuffer(ArrayPool<string?> pool, string?[] buffer, nuint count)
    {
        nuint newCount = count;
        for (nuint i = 0; i < count; i++)
        {
            if (string.IsNullOrEmpty(buffer[i]))
            {
                buffer[i] = null;
                newCount--;
            }
        }
        if (newCount <= 0)
        {
            pool.Return(buffer);
            return Array.Empty<string>();
        }
        string[] result = new string[newCount];
        for (nuint i = 0, j = 0; i < count; i++)
        {
            string? item = buffer[i];
            if (item is null)
                continue;
            result[j++] = item;
        }
        pool.Return(buffer);
        return result;
    }

    [Inline(InlineBehavior.Remove)]
    private static bool NeedRemoveEmptyEntries(StringSplitOptions options)
        => (options & StringSplitOptions.RemoveEmptyEntries) == StringSplitOptions.RemoveEmptyEntries;
}
#endif