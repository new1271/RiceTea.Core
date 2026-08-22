using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using RiceTea.Core.Buffers;
using RiceTea.Core.Native;
using RiceTea.Core.Text;
using RiceTea.Core.Threading;

namespace RiceTea.Core.IO.Internals;

internal sealed class Latin1StreamReader : AsciiBasedStreamReader
{
    private static readonly LazyTiny<Encoding> _encodingLazy = new LazyTiny<Encoding>(GetEncoding, LazyThreadSafetyMode.PublicationOnly);

    public override Encoding CurrentEncoding => _encodingLazy.Value;

    public Latin1StreamReader(Stream stream, int bufferSize, bool leaveOpen) : base(stream, bufferSize, leaveOpen) { }

    private static Encoding GetEncoding()
    {
#if NET8_0_OR_GREATER
        return Encoding.Latin1;
#else
        try
        {
            return Encoding.GetEncoding(codepage: 28591);
        }
        catch (Exception)
        {
            return Encoding.GetEncoding(codepage: 1252); // Windows-1252 編碼頁，雖然與 Latin-1 (ISO-8859-1) 不完全一樣，但可做為佔位符使用
        }
#endif
    }

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
                        char* destinationEnd = Latin1EncodingHelper.WriteToUtf16Buffer(source + currentPos, source + currentLength, destination, destination + currentLength);
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
                return StringWrapper.CreateLatin1String(ptr, 0u, unchecked((nuint)count));
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
                return StringWrapper.CreateLatin1String(ptr, 0u, unchecked((nuint)count));
        }
        finally
        {
            pool.Return(buffer);
        }
    }
}
