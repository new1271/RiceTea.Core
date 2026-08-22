using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using RiceTea.Core.Buffers;
using RiceTea.Core.Text;

namespace RiceTea.Core.IO.Internals;

internal sealed unsafe class Utf8StreamReader : AsciiBasedStreamReader
{
    private static readonly Encoding _encoding = Encoding.UTF8;

    private char? _bufferingCharacter;

    public override Encoding CurrentEncoding => _encoding;

    public Utf8StreamReader(Stream stream, int bufferSize, bool leaveOpen) : base(stream, bufferSize, leaveOpen)
    {
        _bufferingCharacter = null;
    }

    protected override bool CheckEndOfStreamCore()
        => base.CheckEndOfStreamCore() && _bufferingCharacter == null;

    protected override char? ReadCharacterCore(byte[] buffer, bool movePosition)
    {
        char? bufferingCharacter = _bufferingCharacter;
        if (bufferingCharacter is not null)
        {
            if (movePosition)
                _bufferingCharacter = null;
            return bufferingCharacter;
        }

        nuint currentPos, currentLength;
        while ((currentPos = _bufferPos) >= (currentLength = _bufferLength))
        {
            if (CheckEndOfStream(fullyCheck: false))
                return null;
            ReadStream();
        }

        nuint nextPos;
        while ((nextPos = currentPos + Utf8EncodingHelper.GetNextUtf8CharacterOffset(buffer[currentPos])) >= currentLength)
        {
            ReadStream();
            if (CheckEndOfStream(fullyCheck: false))
            {
                _bufferPos = _bufferLength;
                return null;
            }
            currentPos = _bufferPos;
            currentLength = _bufferLength;
        }
        if (movePosition)
            _bufferPos = nextPos;
        fixed (byte* ptr = buffer)
        {
            byte* ptrOffset = Utf8EncodingHelper.TryReadUtf8Character(ptr + currentPos, ptr + nextPos, out uint unicodeValue);
            if (ptrOffset == null)
                return null;
            if (Utf8EncodingHelper.ToUtf16Characters(unicodeValue, out char leadSurrogate, out char trailSurrogate) && movePosition)
                _bufferingCharacter = trailSurrogate;
            return leadSurrogate;
        }
    }

    [SkipLocalsInit]
    protected override string? ReadLineCore(byte[] buffer)
    {
        bool isEndOfStream = CheckEndOfStream(fullyCheck: false) && base.CheckEndOfStreamCore();
        char? bufferingCharacter = _bufferingCharacter;
        if (bufferingCharacter is not null)
        {
            _bufferingCharacter = null;
            char c = bufferingCharacter.Value;
            if (c == '\n')
                return string.Empty;
            if (isEndOfStream)
                return new string(c, 1);
        }

        if (isEndOfStream)
            return null;

        using StringBuilderTiny builder = new StringBuilderTiny();
        if (Limits.UseStackallocStringBuilder)
        {
            char* stackBuffer = stackalloc char[Limits.MaxStackallocChars];
            builder.SetStartPointer(stackBuffer, Limits.MaxStackallocChars);
        }
        if (bufferingCharacter is not null)
            builder.Append(bufferingCharacter.Value);
        nuint currentPos, currentLength;
        nuint? indexOf;
        while ((indexOf = FindNewLineMark(buffer, currentPos = _bufferPos, currentLength = _bufferLength)) is null)
        {
            if (currentPos < currentLength)
            {
                fixed (byte* ptr = buffer)
                {
                    byte* iterator = ptr + currentPos;
                    byte* iteratorOld = iterator;
                    byte* ptrEnd = ptr + currentLength;
                    while ((iterator = Utf8EncodingHelper.TryReadUtf8Character(iterator, ptrEnd, out uint unicodeValue)) != null)
                    {
                        if (Utf8EncodingHelper.ToUtf16Characters(unicodeValue, out char leadSurrogate, out char trailSurrogate))
                        {
                            builder.Append(leadSurrogate);
                            builder.Append(trailSurrogate);
                        }
                        else
                        {
                            builder.Append(leadSurrogate);
                        }
                        iteratorOld = iterator;
                    }
                    _bufferPos = unchecked((nuint)(iteratorOld - ptr));
                }
            }
            ReadStream();
            if (CheckEndOfStream(fullyCheck: false))
            {
                _bufferPos = _bufferLength;
                return builder.Length <= 0 ? null : builder.ToString();
            }
        }

        nuint indexOfReal = indexOf.Value;
        fixed (byte* ptr = buffer)
        {
            byte* iterator = ptr + currentPos;
            byte* ptrEnd = iterator + indexOfReal;
            while ((iterator = Utf8EncodingHelper.TryReadUtf8Character(iterator, ptrEnd, out uint unicodeValue)) != null)
            {
                if (Utf8EncodingHelper.ToUtf16Characters(unicodeValue, out char leadSurrogate, out char trailSurrogate))
                {
                    builder.Append(leadSurrogate);
                    builder.Append(trailSurrogate);
                }
                else
                {
                    builder.Append(leadSurrogate);
                }
            }
            byte* ptrIndexOf = ptr + currentPos + indexOfReal;
            if (*ptrIndexOf == (byte)'\r')
            {
                ptrIndexOf++;
                if (ptrIndexOf < (ptr + currentLength) && *ptrIndexOf == (byte)'\n')
                    indexOf++;
            }
        }

        _bufferPos = currentPos + unchecked((nuint)indexOf) + 1;
        return builder.ToString();
    }

    [SkipLocalsInit]
    protected override string ReadToEndCore(byte[] buffer)
    {
        bool isEndOfStream = CheckEndOfStream(fullyCheck: false) && base.CheckEndOfStreamCore();
        char? bufferingCharacter = _bufferingCharacter;
        if (bufferingCharacter is not null)
        {
            _bufferingCharacter = null;
            if (isEndOfStream)
                return new string(bufferingCharacter.Value, 1);
        }

        if (isEndOfStream)
            return string.Empty;

        using StringBuilderTiny builder = new StringBuilderTiny();
        if (Limits.UseStackallocStringBuilder)
        {
            char* stackBuffer = stackalloc char[Limits.MaxStackallocChars];
            builder.SetStartPointer(stackBuffer, Limits.MaxStackallocChars);
        }
        if (bufferingCharacter is not null)
            builder.Append(bufferingCharacter.Value);
        nuint currentPos, currentLength;
        do
        {
            currentPos = _bufferPos;
            currentLength = _bufferLength;
            if (currentPos < currentLength)
            {
                fixed (byte* ptr = buffer)
                {
                    byte* iterator = ptr + currentPos;
                    byte* iteratorOld = iterator;
                    byte* ptrEnd = ptr + currentLength;
                    while ((iterator = Utf8EncodingHelper.TryReadUtf8Character(iterator, ptrEnd, out uint unicodeValue)) != null)
                    {
                        if (Utf8EncodingHelper.ToUtf16Characters(unicodeValue, out char leadSurrogate, out char trailSurrogate))
                        {
                            builder.Append(leadSurrogate);
                            builder.Append(trailSurrogate);
                        }
                        else
                        {
                            builder.Append(leadSurrogate);
                        }
                        iteratorOld = iterator;
                    }
                    _bufferPos = unchecked((nuint)(iteratorOld - ptr));
                }
            }
            ReadStream();
            if (CheckEndOfStream(fullyCheck: false))
            {
                _bufferPos = _bufferLength;
                break;
            }
        } while (true);
        return builder.ToString();
    }

    [SkipLocalsInit]
    protected override StringWrapper? ReadLineAsStringWrapperCore(byte[] buffer)
    {
        bool isEndOfStream = CheckEndOfStream(fullyCheck: false) && base.CheckEndOfStreamCore();

        char? bufferingCharacter = _bufferingCharacter;
        if (bufferingCharacter is not null)
        {
            _bufferingCharacter = null;
            char c = bufferingCharacter.Value;
            if (c == '\n')
                return StringWrapper.Empty;
            if (isEndOfStream)
            {
                byte* tempBuffer = stackalloc byte[3];
                byte* tempBufferEnd = tempBuffer + 3;
                byte* tempBufferLimit = Utf8EncodingHelper.TryWriteUtf8Character(tempBuffer, tempBufferEnd, bufferingCharacter.Value);
                if (tempBufferLimit > tempBuffer)
                    return StringWrapper.CreateUtf8String(tempBuffer, 0, unchecked((nuint)(tempBufferLimit - tempBuffer)));
                return StringWrapper.Empty;
            }
        }

        if (isEndOfStream)
            return null;

        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        using PooledList<byte> list = new PooledList<byte>(pool, buffer.Length);
        if (bufferingCharacter is not null)
        {
            byte* tempBuffer = stackalloc byte[4];
            byte* tempBufferEnd = tempBuffer + 4;
            byte* tempBufferLimit = Utf8EncodingHelper.TryWriteUtf8Character(tempBuffer, tempBufferEnd, bufferingCharacter.Value);
            if (tempBufferLimit > tempBuffer)
                list.AddRange(tempBuffer, 0, unchecked((nuint)(tempBufferLimit - tempBuffer)));
        }

        isEndOfStream = TryReadLineIntoPooledList(buffer, list);
        try
        {
            (buffer, int count) = list;
            if (count <= 0)
                return isEndOfStream ? null : StringWrapper.Empty;
            fixed (byte* ptr = buffer)
                return StringWrapper.CreateUtf8String(ptr, 0u, unchecked((nuint)count));
        }
        finally
        {
            pool.Return(buffer);
        }
    }

    [SkipLocalsInit]
    protected override StringWrapper ReadToEndAsStringWrapperCore(byte[] buffer)
    {
        bool isEndOfStream = CheckEndOfStream(fullyCheck: false) && base.CheckEndOfStreamCore();
        char? bufferingCharacter = _bufferingCharacter;
        if (bufferingCharacter is not null)
        {
            _bufferingCharacter = null;
            if (isEndOfStream)
            {
                byte* tempBuffer = stackalloc byte[3];
                byte* tempBufferEnd = tempBuffer + 3;
                byte* tempBufferLimit = Utf8EncodingHelper.TryWriteUtf8Character(tempBuffer, tempBufferEnd, bufferingCharacter.Value);
                if (tempBufferLimit > tempBuffer)
                    return StringWrapper.CreateUtf8String(tempBuffer, 0, unchecked((nuint)(tempBufferLimit - tempBuffer)));
                return StringWrapper.Empty;
            }
        }

        if (isEndOfStream)
            return StringWrapper.Empty;

        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        using PooledList<byte> list = new PooledList<byte>(pool, buffer.Length);
        if (bufferingCharacter is not null)
        {
            byte* tempBuffer = stackalloc byte[3];
            byte* tempBufferEnd = tempBuffer + 3;
            byte* tempBufferLimit = Utf8EncodingHelper.TryWriteUtf8Character(tempBuffer, tempBufferEnd, bufferingCharacter.Value);
            if (tempBufferLimit > tempBuffer)
                list.AddRange(tempBuffer, 0, unchecked((nuint)(tempBufferLimit - tempBuffer)));
        }

        ReadToEndIntoPooledList(buffer, list);
        try
        {
            (buffer, int count) = list;
            if (count <= 0)
                return StringWrapper.Empty;
            fixed (byte* ptr = buffer)
                return StringWrapper.CreateUtf8String(ptr, 0u, unchecked((nuint)count));
        }
        finally
        {
            pool.Return(buffer);
        }
    }
}
