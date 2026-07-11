using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

using InlineIL;

using InlineMethod;

namespace RiceTea.Core.Helpers;

public static unsafe class StringHelper
{
    private static readonly void* _fastAllocateStringFuncPointer;

    public static IEqualityComparer<string> OrdinalEqualityComparer => OridinalEqualityComparerImpl.Instance;

    public static IEqualityComparer<string> OrdinalIgnoreCaseEqualityComparer => OridinalIgnoreCaseEqualityComparerImpl.Instance;

    static StringHelper()
    {
        nint methodPointer = ReflectionHelper.GetMethodPointer(typeof(string), "FastAllocateString", [typeof(int)], typeof(string),
           BindingFlags.Static | BindingFlags.NonPublic);
        if (methodPointer == default)
        {
            _fastAllocateStringFuncPointer = (delegate* managed<int, string>)&LegacyAllocateRawString;
            Debug.WriteLine("Cannot find string.FastAllocateString method!, fallback to new string()");
        }
        else
        {
            _fastAllocateStringFuncPointer = (void*)methodPointer;
        }
    }

    [Inline(InlineBehavior.Keep, export: true)]
    public static int GetStringLengthByHeadPointer(char* headPointer)
    {
        return *((int*)headPointer - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AllocateRawString(int length)
    {
        IL.Push(length);
        IL.Push(_fastAllocateStringFuncPointer);
        IL.Emit.Calli(new StandAloneMethodSig(CallingConventions.Standard, typeof(string), typeof(int)));
        return IL.Return<string>();
    }

    private static string LegacyAllocateRawString(int length) => new string('\0', length);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] string? str)
        => str is null || str.Length <= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] string? str)
    {
        if (IsNullOrEmpty(str))
            return true;
        foreach (char c in str)
        {
            if (!CharHelper.IsWhiteSpace(c))
                return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetStringFromUtf32Character([InlineParameter] uint unicodeValue)
    {
        string result;
        if (unicodeValue < 0x10000)
        {
            result = AllocateRawString(1);
            fixed (char* ptr = result)
                *ptr = unchecked((char)unicodeValue);
            return result;
        }
        result = AllocateRawString(2);
        fixed (char* ptr = result)
            WriteUtf32CharacterToUtf16Buffer_Unchecked(ptr, unicodeValue);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteUtf32CharacterToUtf16Buffer(char* buffer, [InlineParameter] uint unicodeValue)
    {
        if (unicodeValue < 0x10000)
        {
            *buffer = unchecked((char)unicodeValue);
            return 1;
        }
        WriteUtf32CharacterToUtf16Buffer_Unchecked(buffer, unicodeValue);
        return 2;
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUtf32CharacterToUtf16Buffer_Unchecked(char* buffer, uint unicodeValue)
    {
        unicodeValue -= 0x10000;
        buffer[0] = unchecked((char)((unicodeValue >> 10) + 0xD800)); //High surrogate
        buffer[1] = unchecked((char)((unicodeValue & 0x3FF) + 0xDC00)); //Low surrogate
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ParseOrDefault(string? str, int defaultValue)
    {
        if (str is null)
            return defaultValue;
        return int.TryParse(str, out int result) ? result : defaultValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWith(string str, string value)
    {
        int length = value.Length;
        if (value.Length < length)
            return false;
        fixed (char* ptr = str, ptr2 = value)
            return SequenceHelper.Equals(ptr, ptr + length, ptr2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWith(string str, string value, StringComparison comparison)
    {
        int length = value.Length;
        if (value.Length < length)
            return false;
        if (comparison == StringComparison.Ordinal)
        {
            fixed (char* ptr = str, ptr2 = value)
                return SequenceHelper.Equals(ptr, ptr + length, ptr2);
        }
        return str.StartsWith(value, comparison);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWith(string str, string value)
    {
        int aLength = str.Length;
        int bLength = value.Length;
        if (aLength < bLength)
            return false;
        fixed (char* ptr = str, ptr2 = value)
            return SequenceHelper.Equals(ptr + aLength - bLength, ptr + aLength, ptr2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWith(string str, string value, StringComparison comparison)
    {
        int aLength = str.Length;
        int bLength = value.Length;
        if (aLength < bLength)
            return false;
        if (comparison == StringComparison.Ordinal)
        {
            fixed (char* ptr = str, ptr2 = value)
                return SequenceHelper.Equals(ptr + aLength - bLength, ptr + aLength, ptr2);
        }
        return str.EndsWith(value, comparison);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(char separator, params string[] values)
    {
#if NET5_0_OR_GREATER
        return string.Join(separator, values);
#else
        return FilteredJoin(separator, str => true, values);
#endif
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(string separator, params string[] values)
    {
        return string.Join(separator, values);
    }

    public static string FilteredJoin(char separator, Func<string?, bool>? filter, params string?[] values)
    {
        if (filter is null)
            return string.Join(separator.ToString(), values);

        return Core(separator, filter, values) ?? string.Empty;
       
        static string? Core(char separator, Func<string?, bool> filter, string?[] values)
        {
            int length = values.Length;
            switch (length)
            {
                case 0:
                    return null;
                case 1:
                    {
                        string? str = values[0];
                        return filter(str) ? str : null;
                    }
                default:
                    if (length < 0)
                        return null;
                    bool needAppendSeperator = separator != '\0';
                    int totalLength = 0;
                    int* strLens = stackalloc int[length];
                    for (int i = 0; i < length - 1; i++)
                    {
                        string? str = values[i];
                        if (!filter(str))
                        {
                            strLens[i] = 0;
                            continue;
                        }
                        if (str is null)
                        {
                            totalLength += 4;
                            strLens[i] = 4;
                        }
                        else
                        {
                            int strLen = str.Length;
                            totalLength += strLen;
                            strLens[i] += strLen;
                        }
                        if (needAppendSeperator)
                            totalLength++;
                    }
                    {
                        int i = length - 1;
                        string? str = values[i];
                        if (filter(str))
                        {
                            if (str is null)
                            {
                                totalLength += 4;
                                strLens[i] = 4;
                            }
                            else
                            {
                                int strLen = str.Length;
                                totalLength += strLen;
                                strLens[i] += strLen;
                            }
                        }
                        else
                        {
                            strLens[i] = 0;
                        }
                    }
                    string result = new string(separator, totalLength);
                    fixed (char* c = result)
                    {
                        char* iterator = c;
                        for (int i = 0; i < length - 1; i++)
                        {
                            int strLen = strLens[i];
                            if (strLen <= 0)
                                continue;
                            string str = values[i] ?? "null";
                            fixed (char* c2 = str)
                            {
                                uint byteCount = unchecked((uint)(sizeof(char) * strLen));
                                UnsafeHelper.CopyBlock(iterator, c2, byteCount);
                                iterator += strLen;
                                if (needAppendSeperator)
                                    iterator++;
                            }
                        }
                        {
                            int i = length - 1;
                            int strLen = strLens[i];
                            if (strLen > 0)
                            {
                                string str = values[i] ?? "null";
                                fixed (char* c2 = str)
                                {
                                    uint byteCount = unchecked((uint)(sizeof(char) * strLen));
                                    UnsafeHelper.CopyBlock(iterator, c2, byteCount);
                                }
                            }
                        }
                    }
                    return result;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FilteredJoin(string separator, Func<string, bool> filter, params string[] values)
    {
        if (values is null)
            return string.Empty;
        if (filter is null)
            return string.Join(separator, values);
        int length = values.Length;
        switch (length)
        {
            case 0:
                return string.Empty;
            case 1:
                {
                    string str = values[0];
                    return filter(str) ? str : string.Empty;
                }
            default:
                if (length < 0)
                    return string.Empty;
                int seperatorLength = separator?.Length ?? 0;
                bool needAppendSeperator = seperatorLength > 0;
                int totalLength = 0;
                int* strLens = stackalloc int[length];
                for (int i = 0; i < length - 1; i++)
                {
                    string str = values[i];
                    if (!filter(str))
                    {
                        strLens[i] = 0;
                        continue;
                    }
                    if (str is null)
                    {
                        totalLength += 4;
                        strLens[i] = 4;
                    }
                    else
                    {
                        int strLen = str.Length;
                        totalLength += strLen;
                        strLens[i] += strLen;
                    }
                    if (needAppendSeperator)
                        totalLength += seperatorLength;
                }
                {
                    int i = length - 1;
                    string str = values[i];
                    if (filter(str))
                    {
                        if (str is null)
                        {
                            totalLength += 4;
                            strLens[i] = 4;
                        }
                        else
                        {
                            int strLen = str.Length;
                            totalLength += strLen;
                            strLens[i] += strLen;
                        }
                    }
                    else
                    {
                        strLens[i] = 0;
                    }
                }
                string result = AllocateRawString(totalLength);
                fixed (char* c = result)
                {
                    char* iterator = c;
                    for (int i = 0; i < length - 1; i++)
                    {
                        int strLen = strLens[i];
                        if (strLen <= 0)
                            continue;
                        string str = values[i];
                        fixed (char* c2 = str)
                        {
                            uint byteCount = unchecked((uint)(sizeof(char) * strLen));
                            UnsafeHelper.CopyBlock(iterator, c2, byteCount);
                            iterator += byteCount / sizeof(char);
                        }
                        if (needAppendSeperator)
                        {
                            fixed (char* c3 = separator)
                            {
                                uint byteCount = unchecked((uint)(sizeof(char) * seperatorLength));
                                UnsafeHelper.CopyBlock(iterator, c3, byteCount);
                                iterator += byteCount / sizeof(char);
                            }
                        }
                    }
                    {
                        int i = length - 1;
                        int strLen = strLens[i];
                        if (strLen > 0)
                        {
                            string str = values[i];
                            fixed (char* c2 = str)
                            {
                                uint byteCount = unchecked((uint)(sizeof(char) * strLen));
                                UnsafeHelper.CopyBlock(iterator, c2, byteCount);
                            }
                        }
                    }
                }
                return result;
        }
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf(string str, char value) => SequenceHelper.IndexOf(str, value);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf(string str, char value, int startIndex) => SequenceHelper.IndexOf(str, value, startIndex);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf(string str, char value, int startIndex, int count) => SequenceHelper.IndexOf(str, value, startIndex, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAny(string str, params char[] values)
    {
        for (int i = 0, valuesLength = values.Length; i < valuesLength; i++)
        {
            int result = IndexOf(str, values[i]);
            if (result < 0)
                continue;
            return result;
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAny(string str, int startIndex, params char[] values)
    {
        for (int i = 0, valuesLength = values.Length; i < valuesLength; i++)
        {
            int result = IndexOf(str, values[i], startIndex);
            if (result < 0)
                continue;
            return result;
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAny(string str, int startIndex, int count, params char[] values)
    {
        for (int i = 0, valuesLength = values.Length; i < valuesLength; i++)
        {
            int result = IndexOf(str, values[i], startIndex, count);
            if (result < 0)
                continue;
            return result;
        }
        return -1;
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains(string str, char value) => SequenceHelper.Contains(str, value);

#if !NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(string str, params char[] values)
    {
        int length = values.Length;
        if (length <= 0)
            return false;
        ref char valueRef = ref UnsafeHelper.GetArrayDataReference(values);
        for (int i = 0; i < length; i++)
        {
            if (SequenceHelper.Contains(str, UnsafeHelper.AddTypedOffset(ref valueRef, i)))
                return true;
        }
        return false;
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetStringForFirstNonEmptyLine(string original)
    {
        int length = original.Length;
        if (length <= 0)
            return original;
        fixed (char* ptr = original)
            return GetStringForFirstNonEmptyLineCore(ptr, length, original);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetStringForFirstNonEmptyLineCore(char* original, int length, string originalInManaged)
    {
        char* start = original, end = original + length;
        do
        {
            switch (*start)
            {
                case '\r':
                case '\n':
                    start++;
                    if (++start >= end)
                        return originalInManaged;
                    continue;
                default:
                    goto OutOfLoop;
            }
        } while (true);

    OutOfLoop:
        char* newLine = SequenceHelper.PointerIndexOf(start, end, '\n');
        char* newLine2 = SequenceHelper.PointerIndexOf(start, end, '\r');
        if (newLine < start)
        {
            if (newLine2 < start)
                return originalInManaged;
            newLine = newLine2;
        }
        else
        {
            if (newLine2 >= start)
                newLine = MathHelper.Min(newLine, newLine2);
        }
        return new string(start, 0, (int)(newLine - start));
    }

    private sealed class OridinalEqualityComparerImpl : IEqualityComparer<string>
    {
        public static readonly OridinalEqualityComparerImpl Instance = new OridinalEqualityComparerImpl();

        private OridinalEqualityComparerImpl() { }

        public bool Equals(string? x, string? y) => SequenceHelper.Equals(x, y);

        public int GetHashCode(string obj) => obj.GetHashCode();
    }

    private sealed class OridinalIgnoreCaseEqualityComparerImpl : IEqualityComparer<string>
    {
        public static readonly OridinalIgnoreCaseEqualityComparerImpl Instance = new OridinalIgnoreCaseEqualityComparerImpl();

#if NET8_0_OR_GREATER
        private OridinalIgnoreCaseEqualityComparerImpl() { }

        public int GetHashCode(string obj) => string.GetHashCode(obj, StringComparison.OrdinalIgnoreCase);
#else
        private static delegate* managed<string, int> _hashCodePointer;

        private OridinalIgnoreCaseEqualityComparerImpl()
        {
            nint pointer = ReflectionHelper.GetMethodPointer(typeof(TextInfo), "GetHashCodeOrdinalIgnoreCase",
                 [typeof(string)], typeof(int),
                 BindingFlags.Static | BindingFlags.NonPublic);
            if (pointer == default)
                _hashCodePointer = &GetHashCodeOrdinalIgnoreCase_Fallback;
            else
                _hashCodePointer = (delegate* managed<string, int>)pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(string obj) => _hashCodePointer(obj);

        private static int GetHashCodeOrdinalIgnoreCase_Fallback(string str)
            => StringComparer.OrdinalIgnoreCase.GetHashCode(str);
#endif

        public bool Equals(string? x, string? y) => SequenceHelper.Equals(x, y, StringComparison.OrdinalIgnoreCase);
    }
}
