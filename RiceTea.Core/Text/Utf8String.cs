using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace RiceTea.Core.Text;

internal sealed partial class Utf8String : StringWrapper, IPinnableReference<byte>, IReadOnlyViewProvider<byte>
{
    public const int CodePage = 65001;

    private static readonly nuint MaxUtf8StringBufferSize = unchecked((nuint)Limits.MaxArrayLength - 1);
    private static readonly nuint Utf16CompressionLengthLimit = unchecked((nuint)Limits.MaxArrayLength / 2 - 1);

    private readonly byte[] _value;
    private readonly int _length;

    public override StringType StringType => StringType.Utf8;
    public override int Length => _length;

    private Utf8String(byte[] value, int length)
    {
        _value = value;
        _length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8String Allocate(nuint lengthOfBuffer, int lengthOfChars, out byte[] buffer)
    {
        if (lengthOfBuffer > MaxUtf8StringBufferSize)
            OutOfMemoryException.Throw();

        return new Utf8String(buffer = new byte[lengthOfBuffer + 1], lengthOfChars);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe StringWrapper Create(byte* source, nuint length)
    {
        if (length > MaxUtf8StringBufferSize)
            OutOfMemoryException.Throw();

        if (!SequenceHelper.ContainsGreaterThan(source, length, AsciiEncodingHelper.AsciiEncodingLimit_InByte))
        {
            AsciiString result = AsciiString.Allocate(length, out byte[] buffer);
            fixed (byte* ptr = buffer)
                UnsafeHelper.CopyBlockUnaligned(ptr, source, length * sizeof(byte));
            return result;
        }
        else
        {
            byte[] buffer = new byte[length + 1];
            fixed (byte* ptr = buffer)
                UnsafeHelper.CopyBlockUnaligned(ptr, source, length * sizeof(byte));

            int resultLength = 0;
            using CharEnumerator enumerator = new CharEnumerator(buffer);
            while (enumerator.MoveNext())
                resultLength++;
            return new Utf8String(buffer, resultLength);
        }
    }

    public static unsafe bool TryCreate(char* source, nuint length, StringCreateOptions options, [NotNullWhen(true)] out StringWrapper? result)
    {
        if ((options & StringCreateOptions.UseAsciiCompression) != StringCreateOptions.UseAsciiCompression &&
            !SequenceHelper.ContainsGreaterThan(source, length, AsciiEncodingHelper.AsciiEncodingLimit))
        {
            result = AsciiString.Allocate(length, out byte[] buffer);
            fixed (byte* ptr = buffer)
                AsciiEncodingHelper.ReadFromUtf16BufferCore(source, ptr, length);
            return true;
        }

        bool tryEncodeAsPossible = (options & StringCreateOptions._Force_Flag) == StringCreateOptions._Force_Flag;
        return TryCreateCore(source, length, tryEncodeAsPossible, out result);
    }

    private static unsafe bool TryCreateCore(char* source, nuint length, bool tryEncodeAsPossible, [NotNullWhen(true)] out StringWrapper? result)
    {
        nuint bufferLength;

        if (tryEncodeAsPossible)
            bufferLength = Utf8EncodingHelper.GetWorstCaseForEncodeLength(length);
        else
            bufferLength = length * sizeof(char);
        bufferLength = MathHelper.Min(bufferLength, MaxUtf8StringBufferSize);

        NativeMemoryPool pool = NativeMemoryPool.Shared;
        TypedNativeMemoryBlock<byte> buffer = pool.Rent<byte>(bufferLength);
        try
        {
            byte* ptr = buffer.NativePointer;
            byte* bufferEnd = Utf8EncodingHelper.TryReadFromUtf16BufferCore(source, source + length, ptr, ptr + bufferLength);
            if (bufferEnd == null)
                goto Failed;
            nuint resultLength = unchecked((nuint)(bufferEnd - ptr));
            byte[] resultBuffer = new byte[resultLength + 1];
            fixed (byte* dest = resultBuffer)
                UnsafeHelper.CopyBlockUnaligned(dest, ptr, resultLength * sizeof(byte));
            result = new Utf8String(resultBuffer, unchecked((int)length));
            return true;
        }
        finally
        {
            pool.Return(buffer);
        }

    Failed:
        result = null;
        return false;
    }

    public override bool IsSpecificEncoding(Encoding encoding) => encoding.CodePage == CodePage;

    public override IEnumerator<char> GetEnumerator() => new CharEnumerator(_value);

    protected internal override unsafe void CopyToCore(char* destination, nuint startIndex, nuint count)
    {
        byte[] source = _value;
        fixed (byte* ptrSource = source)
        {
            byte* sourceIterator = ptrSource, sourceEnd = ptrSource + source.Length - 1;
            nuint offset = SkipCharacters(ref sourceIterator, sourceEnd, destination, startIndex);
            Utf8EncodingHelper.TryWriteToUtf16BufferCore(sourceIterator, sourceEnd, destination + offset, destination + count);
        }
    }

    public byte[] GetInternalRepresentation() => _value;

    public override StringWrapper ToStringWrapper(StringCreateOptions options)
    {
        if (!options.HasFlagFast(StringCreateOptions._Force_Flag))
        {
            if (!options.HasFlagFast(StringCreateOptions.UseUtf16Compression) || _value.Length <= _length)
                return this;
            goto Utf16;
        }
        if (options.HasFlagFast(StringCreateOptions.UseAsciiCompression))
            return ToStringWrapper_Ascii();
        if (options.HasFlagFast(StringCreateOptions.UseLatin1Compression))
            return ToStringWrapper_Latin1();
        if (options.HasFlagFast(StringCreateOptions.UseUtf16Compression))
            goto Utf16;
        return this;

    Utf16:
        return CreateUtf16String(ToString());
    }

    public override StringWrapper ToStringWrapper(StringType type)
        => type switch
        {
            StringType.Empty => Empty,
            StringType.Ascii => ToStringWrapper_Ascii(),
            StringType.Latin1 => ToStringWrapper_Latin1(),
            StringType.Utf8 => this,
            StringType.Utf16 => CreateUtf16String(ToString()),
            _ => ArgumentOutOfRangeException.Throw<StringWrapper>(nameof(type)),
        };

    ref readonly byte IPinnableReference<byte>.GetPinnableReference()
    {
        byte[] value = _value;
        int length = value.Length;
        if (length <= 0)
            return ref ((IPinnableReference<byte>)Empty).GetPinnableReference();
        return ref UnsafeHelper.GetArrayDataReference(value);
    }

    nuint IPinnableReference<byte>.GetPinnedLength() => MathHelper.MakeUnsigned(_value.Length - 1);

    ReadOnlyView<byte> IReadOnlyViewProvider<byte>.CreateView()
    {
        byte[] value = _value;
        return new ReadOnlyView<byte>(value).Slice(0, value.Length - 1);
    }

    private unsafe AsciiString ToStringWrapper_Ascii()
    {
        byte[] value = _value;

        nuint length = MathHelper.MakeUnsigned(_length);
        AsciiString result = AsciiString.Allocate(length, out byte[] buffer);
        fixed (byte* source = value, destination = buffer)
        {
            byte* iterator = source, sourceEnd = source + value.Length;
            for (nuint i = 0; i < length; i++)
            {
                iterator = Utf8EncodingHelper.TryReadUtf8Character(iterator, sourceEnd, out uint unicodeValue);
                nuint mask = UnsafeHelper.Negate(MathHelper.BooleanToNativeUnsigned(unicodeValue > AsciiEncodingHelper.AsciiEncodingLimit_InByte));
                destination[i] =  unchecked((byte)(('?' & mask) | (unicodeValue & ~mask)));
            }
        }
        return result;
    }

    private unsafe AsciiLikeString ToStringWrapper_Latin1()
    {
        byte[] value = _value;

        nuint length = MathHelper.MakeUnsigned(_length);
        byte[] buffer = new byte[length + 1];
        fixed (byte* source = value, destination = buffer)
        {
            byte* iterator = source, sourceEnd = source + value.Length;
            for (nuint i = 0; i < length; i++)
            {
                iterator = Utf8EncodingHelper.TryReadUtf8Character(iterator, sourceEnd, out uint unicodeValue);
                nuint mask = UnsafeHelper.Negate(MathHelper.BooleanToNativeUnsigned(unicodeValue > Latin1EncodingHelper.Latin1EncodingLimit_InByte));
                destination[i] =  unchecked((byte)(('?' & mask) | (unicodeValue & ~mask)));
            }
            if (SequenceHelper.ContainsGreaterThan(destination, length, AsciiEncodingHelper.AsciiEncodingLimit_InByte))
                return new Latin1String(buffer);
            else
                return new AsciiString(buffer);
        }
    }
}
