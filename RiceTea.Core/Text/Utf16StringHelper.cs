using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Text;

internal static partial class Utf16StringHelper
{
    // UTF-16 代理對範圍
    private const char SurrogateStart = unchecked((char)0b_1101_1000_0000_0000);
    private const char SurrogateEnd = unchecked((char)0b_1101_1111_1111_1111);
    // 空白字元表，來源: https://www.unicode.org/Public/UCD/latest/ucd/PropList.txt
    private const int WhiteSpaceCharacterCount = 25;
    private const int WhiteSpaceCharacterCount_Latin1 = 8;
    private const int WhiteSpaceCharacterCount_NonLatin1 = WhiteSpaceCharacterCount - WhiteSpaceCharacterCount_Latin1;
    private static readonly char[] WhiteSpaceCharacters_Latin1 = new char[WhiteSpaceCharacterCount_Latin1] {
        '\u0009', '\u000A', '\u000B', '\u000C',
        '\u000D', '\u0020', '\u0085', '\u00A0'
    };
    private static readonly char[] WhiteSpaceCharacters_NonLatin1 = new char[WhiteSpaceCharacterCount_NonLatin1] {
        '\u1680', '\u2000', '\u2001', '\u2002',
        '\u2003', '\u2004', '\u2005', '\u2006',
        '\u2007', '\u2008', '\u2009', '\u200A',
        '\u2028', '\u2029', '\u202F', '\u205F',
        '\u3000'
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasSurrogateCharacters(char* ptr, nuint length)
        => VectorizedHasSurrogateCharacters(ptr, ptr + length);

    private static unsafe bool LegacyHasSurrogateCharacters(char* ptr, char* ptrEnd)
    {
        for (; ptr < ptrEnd; ptr++)
        {
            if (IsSurrogateCharacter(*ptr))
                return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSurrogateCharacter(char c)
        => c >= SurrogateStart && c <= SurrogateEnd;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool IsWhiteSpaceCharacter(char c)
    {
        char[] table = c <= Latin1EncodingHelper.Latin1EncodingLimit
             ? GetWhiteSpaceTableForLatin1(out nuint count)
             : GetWhiteSpaceTableForNonLatin1(out count);

        fixed (char* ptr = table)
            return SequenceHelper.Contains(ptr, count, c);
    }

    [Inline(InlineBehavior.Remove)]
    private static char[] GetWhiteSpaceTableForLatin1(out nuint count)
    {
        count = WhiteSpaceCharacterCount_Latin1;
        return WhiteSpaceCharacters_Latin1;
    }

    [Inline(InlineBehavior.Remove)]
    private static char[] GetWhiteSpaceTableForNonLatin1(out nuint count)
    {
        count = WhiteSpaceCharacterCount_NonLatin1;
        return WhiteSpaceCharacters_NonLatin1;
    }
}
