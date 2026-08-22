using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

namespace RiceTea.Core.Text;

internal sealed partial class AsciiString : AsciiLikeString, IPinnableReference<byte>
{
	public const int CodePage = 20127;

	public override StringType StringType => StringType.Ascii;

	internal AsciiString(byte[] value) : base(value) { }

	protected override byte GetCharacterLimit() => AsciiEncodingHelper.AsciiEncodingLimit_InByte;

	public override bool IsSpecificEncoding(Encoding encoding)
		=> encoding.CodePage switch
		{
			CodePage or Latin1String.CodePage or
			Latin1String.CodePage_Alternative or Utf8String.CodePage => true,
			_ => false
		};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static AsciiString Allocate(nuint length, out byte[] buffer)
	{
		if (length > MaxStringLength)
			OutOfMemoryException.Throw();

		buffer = ArrayHelper.CreateUninitializedArray<byte>((int)length);
		buffer.AsUnsafeRef()[length] = default;
		return new AsciiString(buffer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe StringWrapper Create(byte character, nuint length)
	{
		byte mask = UnsafeHelper.Negate(MathHelper.BooleanToUInt8(character > AsciiEncodingHelper.AsciiEncodingLimit_InByte));
		character = (byte)(((byte)'?' & mask) | (character & ~mask));

		AsciiString result = Allocate(length, out byte[] buffer);
		UnsafeHelper.InitBlockUnaligned(ref UnsafeHelper.GetArrayDataReference(buffer), character, length);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe AsciiString Create(byte* source, nuint length)
	{
		if (length >= MaxStringLength)
			OutOfMemoryException.Throw();

		byte[] buffer = ArrayHelper.CreateUninitializedArray<byte>((int)(length + 1));
		fixed (byte* ptr = buffer)
		{
			ptr[length] = 0;
			UnsafeHelper.CopyBlockUnaligned(ptr, source, length * sizeof(byte));
		}
		return new AsciiString(buffer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static AsciiString Create(ref readonly byte source, nuint length)
	{
		if (length >= MaxStringLength)
			OutOfMemoryException.Throw();

		byte[] buffer = ArrayHelper.CreateUninitializedArray<byte>((int)(length + 1));
		ref byte bufferRef = ref UnsafeHelper.GetArrayDataReference(buffer);
		UnsafeHelper.AddByteOffset(ref bufferRef, length) = 0;
		UnsafeHelper.CopyBlockUnaligned(ref bufferRef, in source, length * sizeof(byte));
		return new AsciiString(buffer);
	}

	public static unsafe bool TryCreate(char* source, nuint length, StringCreateOptions options, [NotNullWhen(true)] out AsciiString? result)
	{
		byte[] buffer;
		if ((options & StringCreateOptions._Force_Flag) == StringCreateOptions._Force_Flag)
		{
			buffer = CreateAsciiStringCore_OutOfAsciiRange(source, length);
		}
		else if (SequenceHelper.ContainsGreaterThan(source, length, AsciiEncodingHelper.AsciiEncodingLimit))
			goto Failed;
		else
		{
			buffer = CreateAsciiStringCore(source, length);
		}

		result = new AsciiString(buffer);
		return true;

	Failed:
		result = null;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static unsafe byte[] CreateAsciiStringCore_OutOfAsciiRange(char* source, nuint length)
	{
		byte[] buffer = new byte[length + 1];
		fixed (byte* destination = buffer)
			AsciiEncodingHelper.ReadFromUtf16BufferCore_OutOfAsciiRange(source, destination, length);
		return buffer;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static unsafe byte[] CreateAsciiStringCore(char* source, nuint length)
	{
		byte[] buffer = new byte[length + 1];
		fixed (byte* destination = buffer)
			AsciiEncodingHelper.ReadFromUtf16BufferCore(source, destination, length);
		return buffer;
	}

	public override StringWrapper ToStringWrapper(StringCreateOptions options)
	{
		if (options == StringCreateOptions.ForceUseUtf16)
			return CreateUtf16String(ToString());
		return this;
	}

	public override StringWrapper ToStringWrapper(StringType type)
		=> type switch
		{
			StringType.Empty => Empty,
			StringType.Ascii or StringType.Latin1 or StringType.Utf8 => this,
			StringType.Utf16 => CreateUtf16String(ToString()),
			_ => ArgumentOutOfRangeException.Throw<StringWrapper>(nameof(type))
		};
}
