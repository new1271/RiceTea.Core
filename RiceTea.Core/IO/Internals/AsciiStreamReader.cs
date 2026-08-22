using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Text;

namespace RiceTea.Core.IO.Internals;

internal sealed class AsciiStreamReader : AsciiBasedStreamReader
{
    private static readonly Encoding _encoding = Encoding.ASCII;

    public override Encoding CurrentEncoding => _encoding;

    public AsciiStreamReader(Stream stream, int bufferSize, bool leaveOpen) : base(stream, bufferSize, leaveOpen) { }

    protected override char? ReadCharacterCore(byte[] buffer, bool movePosition)
    {
        nuint currentPos, nextPos;
        while ((nextPos = (currentPos = _bufferPos) + 1) >= _bufferLength)
        {
            ReadStream();
            if (CheckEndOfStream(fullyCheck: false))
            {
                _bufferPos = _bufferLength;
                return null;
            }
        }
        if (movePosition)
            _bufferPos = nextPos;
        return unchecked((char)buffer[currentPos]);
    }

    [SkipLocalsInit]
    protected override unsafe string? ReadLineCore(byte[] buffer)
    {
        if (CheckEndOfStream(fullyCheck: true))
            return null;

        using StringBuilderTiny builder = new StringBuilderTiny();
        if (Limits.UseStackallocStringBuilder)
        {
            char* stackBuffer = stackalloc char[Limits.MaxStackallocChars];
            builder.SetStartPointer(stackBuffer, Limits.MaxStackallocChars);
        }
        nuint currentPos, currentLength;
        nuint? indexOf;

        NativeMemoryPool pool = NativeMemoryPool.Shared;
        TypedNativeMemoryBlock<char> charBuffer = pool.Rent<char>(buffer.Length);
        try
        {
            while ((indexOf = FindNewLineMark(buffer, currentPos = _bufferPos, currentLength = _bufferLength)) is null)
            {
                if (currentPos < currentLength)
                {
                    fixed (byte* source = buffer)
                    {
                        char* destination = charBuffer.NativePointer;
                        char* destinationEnd = Latin1EncodingHelper.WriteToUtf16Buffer(source + currentPos, source + currentLength, destination, destination + currentLength);
                        SequenceHelper.ReplaceGreaterThan(destination, unchecked((nuint)(destinationEnd - destination)), AsciiEncodingHelper.AsciiEncodingLimit, '?');
                        builder.Append(destination, destinationEnd);
                    }
                    _bufferPos = currentLength;
                }
                ReadStream();
                if (CheckEndOfStream(fullyCheck: false))
                {
                    _bufferPos = _bufferLength;
                    return builder.Length <= 0 ? null : builder.ToString();
                }
            }

            nuint indexOfReal = indexOf.Value;
            fixed (byte* source = buffer)
            {
                char* destination = charBuffer.NativePointer;
                char* destinationEnd = Latin1EncodingHelper.WriteToUtf16Buffer(source + currentPos, source + indexOfReal, destination, destination + currentLength);
                SequenceHelper.ReplaceGreaterThan(destination, unchecked((nuint)(destinationEnd - destination)), AsciiEncodingHelper.AsciiEncodingLimit, '?');
                builder.Append(destination, destinationEnd);
                byte* ptrIndexOf = source + currentPos + indexOfReal;
                if (*ptrIndexOf == (byte)'\r')
                {
                    ptrIndexOf++;
                    if (ptrIndexOf < (source + currentLength) && *ptrIndexOf == (byte)'\n')
                        indexOf++;
                }
            }
            _bufferPos = currentPos + unchecked((nuint)indexOf) + 1;
        }
        finally
        {
            pool.Return(charBuffer);
        }
        return builder.ToString();
    }

    [SkipLocalsInit]
    protected override unsafe string ReadToEndCore(byte[] buffer)
    {
        if (CheckEndOfStream(fullyCheck: true))
            return string.Empty;

        using StringBuilderTiny builder = new StringBuilderTiny();
        if (Limits.UseStackallocStringBuilder)
        {
            char* stackBuffer = stackalloc char[Limits.MaxStackallocChars];
            builder.SetStartPointer(stackBuffer, Limits.MaxStackallocChars);
        }
        nuint currentPos, currentLength;

        NativeMemoryPool pool = NativeMemoryPool.Shared;
        TypedNativeMemoryBlock<char> charBuffer = pool.Rent<char>(buffer.Length);
        try
        {
            do
            {
                currentPos = _bufferPos;
                currentLength = _bufferLength;
                if (currentPos < currentLength)
                {
                    fixed (byte* source = buffer)
                    {
                        char* destination = charBuffer.NativePointer;
                        char* destinationEnd = AsciiEncodingHelper.WriteToUtf16Buffer(source + currentPos, source + currentLength, destination, destination + currentLength);
                        SequenceHelper.ReplaceGreaterThan(destination, unchecked((nuint)(destinationEnd - destination)), AsciiEncodingHelper.AsciiEncodingLimit, '?');
                        builder.Append(destination, destinationEnd);
                    }
                    _bufferPos = currentLength;
                }
                ReadStream();
                if (CheckEndOfStream(fullyCheck: false))
                {
                    _bufferPos = _bufferLength;
                    break;
                }
            } while (true);
        }
        finally
        {
            pool.Return(charBuffer);
        }
        return builder.ToString();
    }

    protected override unsafe StringWrapper? ReadLineAsStringWrapperCore(byte[] buffer)
    {
        if (CheckEndOfStream(fullyCheck: true))
            return null;

        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        using PooledList<byte> list = new PooledList<byte>(pool, buffer.Length);
        bool isEndOfStream = TryReadLineIntoPooledList(buffer, list);
        try
        {
            (buffer, int count) = list;
            if (count <= 0)
                return isEndOfStream ? null : StringWrapper.Empty;
            fixed (byte* ptr = buffer)
            {
                SequenceHelper.ReplaceGreaterThan(ptr, unchecked((nuint)count), AsciiEncodingHelper.AsciiEncodingLimit_InByte, (byte)'?');
                return StringWrapper.CreateAsciiString(ptr, 0u, unchecked((nuint)count));
            }
        }
        finally
        {
            pool.Return(buffer);
        }
    }

    protected override unsafe StringWrapper ReadToEndAsStringWrapperCore(byte[] buffer)
    {
        if (CheckEndOfStream(fullyCheck: true))
            return StringWrapper.Empty;

        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        using PooledList<byte> list = new PooledList<byte>(pool, buffer.Length);
        ReadToEndIntoPooledList(buffer, list);
        try
        {
            (buffer, int count) = list;
            if (count <= 0)
                return StringWrapper.Empty;
            fixed (byte* ptr = buffer)
            {
                SequenceHelper.ReplaceGreaterThan(ptr, unchecked((nuint)count), AsciiEncodingHelper.AsciiEncodingLimit_InByte, (byte)'?');
                return StringWrapper.CreateAsciiString(ptr, 0u, unchecked((nuint)count));
            }
        }
        finally
        {
            pool.Return(buffer);
        }
    }
}
