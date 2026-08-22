using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;

namespace RiceTea.Core.Text;

internal sealed partial class Utf16String : StringWrapper, IPinnableReference<char>, IReadOnlyViewProvider<char>
{
	private static readonly nuint MaxStringLength = unchecked((nuint)Limits.MaxStringLength);

	public const int CodePage = 1200;

    private readonly string _value;

    public override StringType StringType => StringType.Utf16;
    public override int Length => _value.Length;

    private Utf16String(string value)
    {
        _value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf16String Allocate(nuint length, out string buffer)
    {
        if (length > int.MaxValue)
            OutOfMemoryException.Throw();

        return new Utf16String(buffer = StringHelper.AllocateRawString(unchecked((int)length)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new static Utf16String Create(string source) => new Utf16String(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Utf16String Create(char character, nuint length)
    {
        if (length > MaxStringLength)
            OutOfMemoryException.Throw();

        return new Utf16String(new string(character, unchecked((int)length)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Utf16String Create(char* source, nuint length)
    {
        if (length > int.MaxValue)
            OutOfMemoryException.Throw();

        return new Utf16String(new string(source, 0, unchecked((int)length)));
    }

    public override bool IsSpecificEncoding(Encoding encoding) => encoding.CodePage == CodePage;

    protected internal override unsafe char GetCharAt(nuint index)
    {
        fixed (char* ptr = _value)
            return ptr[index];
    }

    protected override bool IsFullyWhitespaced()
    {
        foreach (char character in _value)
        {
            if (!Utf16StringHelper.IsWhiteSpaceCharacter(character))
                return false;
        }
        return true;
    }

    protected internal override unsafe void CopyToCore(char* destination, nuint startIndex, nuint count)
    {
        fixed (char* ptr = _value)
            UnsafeHelper.CopyBlockUnaligned(destination, ptr + startIndex, count * sizeof(char));
    }

    public override IEnumerator<char> GetEnumerator() => _value.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal string GetInternalRepresentation() => _value;

    public override int GetHashCode() => _value.GetHashCode();

    public override unsafe char[] ToCharArray()
    {
        int length = Length;
        if (length <= 0)
            return Array.Empty<char>();
        char[] result = new char[length];
        fixed (char* source = _value, destination = result)
            UnsafeHelper.CopyBlockUnaligned(destination, source, unchecked((uint)length * sizeof(char)));
        return result;
    }

    public override StringWrapper ToStringWrapper(StringCreateOptions options) => Create(_value, options);

    public override StringWrapper ToStringWrapper(StringType type)
        => type switch
        {
            StringType.Empty => Empty,
            StringType.Utf16 => this,
            _ => Create(_value, type switch
            {
                StringType.Ascii => StringCreateOptions.ForceUseAscii,
                StringType.Latin1 => StringCreateOptions.ForceUseLatin1,
                StringType.Utf8 => StringCreateOptions.ForceUseUtf8,
                _ => ArgumentOutOfRangeException.Throw<StringCreateOptions>(nameof(type)),
            })
        };

    public override string ToString() => _value;

    ref readonly char IPinnableReference<char>.GetPinnableReference() => ref UnsafeHelper.GetStringDataReference(_value);

    nuint IPinnableReference<char>.GetPinnedLength() => MathHelper.MakeUnsigned(_value.Length);

    ReadOnlyView<char> IReadOnlyViewProvider<char>.CreateView() => ReadOnlyView.FromString(_value);
}
