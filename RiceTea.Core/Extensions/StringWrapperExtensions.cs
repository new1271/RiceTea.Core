using System;
using System.Runtime.CompilerServices;

using RiceTea.Core.Text;

namespace RiceTea.Core.Extensions;

public static class StringWrapperExtensions
{
    extension(StringWrapper)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringWrapper Create(in ReadOnlyMemory<char> memory)
            => StringWrapper.Create(memory.Span, RTCore.StringCreateOptions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringWrapper Create(in ReadOnlySpan<char> span)
            => StringWrapper.Create(span, RTCore.StringCreateOptions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper Create(in ReadOnlyMemory<char> memory, StringCreateOptions options)
        {
            int length = memory.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (char* ptr = memory.Span)
                return StringWrapper.CreateCore(ptr, unchecked((nuint)length), options);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper Create(in ReadOnlySpan<char> span, StringCreateOptions options)
        {
            int length = span.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (char* ptr = span)
                return StringWrapper.CreateCore(ptr, unchecked((nuint)length), options);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper CreateUtf16String(in ReadOnlyMemory<char> memory)
        {
            int length = memory.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (char* ptr = memory.Span)
                return Utf16String.Create(ptr, unchecked((nuint)length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper CreateUtf16String(in ReadOnlySpan<char> span)
        {
            int length = span.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (char* ptr = span)
                return Utf16String.Create(ptr, unchecked((nuint)length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper CreateAsciiString(in ReadOnlyMemory<byte> memory)
        {
            int length = memory.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (byte* ptr = memory.Span)
                return AsciiString.Create(ptr, unchecked((nuint)length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper CreateAsciiString(in ReadOnlySpan<byte> span)
        {
            int length = span.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (byte* ptr = span)
                return AsciiString.Create(ptr, unchecked((nuint)length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringWrapper CreateAsciiStringUnsafe(ref readonly byte reference, int length)
        {
            if (length <= 0)
                return StringWrapper.Empty;
            return AsciiString.Create(in reference, unchecked((nuint)length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper CreateLatin1String(in ReadOnlyMemory<byte> memory)
        {
            int length = memory.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (byte* ptr = memory.Span)
                return Latin1String.Create(ptr, unchecked((nuint)length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper CreateLatin1String(in ReadOnlySpan<byte> span)
        {
            int length = span.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (byte* ptr = span)
                return Latin1String.Create(ptr, unchecked((nuint)length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper CreateUtf8String(in ReadOnlyMemory<byte> memory)
        {
            int length = memory.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (byte* ptr = memory.Span)
                return Utf8String.Create(ptr, unchecked((nuint)length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe StringWrapper CreateUtf8String(in ReadOnlySpan<byte> span)
        {
            int length = span.Length;
            if (length <= 0)
                return StringWrapper.Empty;
            fixed (byte* ptr = span)
                return Utf8String.Create(ptr, unchecked((nuint)length));
        }
    }
}
