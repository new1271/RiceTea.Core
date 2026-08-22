using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace RiceTea.Core.Text;

internal static class AsciiLikeStringHelper
{
    // 空白字元表，來源: https://www.unicode.org/Public/UCD/latest/ucd/PropList.txt
    private const int WhiteSpaceCharacterCount = 8;
    private static readonly byte[] WhiteSpaceCharacters = new byte[WhiteSpaceCharacterCount] { 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x20, 0x85, 0xA0 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int CompareTo_Utf16(byte* a, char* b, nuint length)
    {
        for (nuint i = 0; i < length; i++)
        {
            int comparison = unchecked((char)a[i]).CompareTo(b[i]);
            if (comparison != 0)
                return MathHelper.Sign(comparison);
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool Equals_Utf16(byte* a, char* b, nuint length)
    {
        if (SequenceHelper.ContainsGreaterThan(b, length, Latin1EncodingHelper.Latin1EncodingLimit))
            return false;
        NativeMemoryPool pool = NativeMemoryPool.Shared;
        TypedNativeMemoryBlock<byte> buffer = pool.Rent<byte>(length);
        try
        {
            byte* ptrBuffer = buffer.NativePointer;
            Latin1EncodingHelper.ReadFromUtf16BufferCore(b, ptrBuffer, length);
            return SequenceHelper.Equals(a, ptrBuffer, length);
        }
        finally
        {
            pool.Return(buffer);
        }
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool IsWhiteSpaceCharacter(byte c)
    {
        fixed (byte* ptr = WhiteSpaceCharacters)
            return SequenceHelper.Contains(ptr, WhiteSpaceCharacterCount, c);
    }
}
