using System.Runtime.CompilerServices;

using InlineMethod;

namespace RiceTea.Core.Text;

unsafe partial class Latin1EncodingHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte* ReadFromUtf16BufferCore_OutOfLatin1Range(char* source, byte* destination, nuint length)
    {
        if (Limits.CheckTypeCanBeVectorized<ushort>() && Limits.CheckTypeCanBeVectorized<byte>() && length > Limits.GetLimitForVectorizing<byte>())
            return VectorizedReadFromUtf16BufferCore_OutOfLatin1Range(source, destination, length);
        return ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(ref source, ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte* ReadFromUtf16BufferCore(char* source, byte* destination, nuint length)
    {
        if (Limits.CheckTypeCanBeVectorized<ushort>() && Limits.CheckTypeCanBeVectorized<byte>() && length > Limits.GetLimitForVectorizing<byte>())
            return VectorizedReadFromUtf16BufferCore(source, destination, length);
        return ScalarizedReadFromUtf16BufferCore(ref source, ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static char* WriteToUtf16BufferCore(byte* source, char* destination, nuint length)
    {
        if (Limits.CheckTypeCanBeVectorized<ushort>() && Limits.CheckTypeCanBeVectorized<byte>() && length > Limits.GetLimitForVectorizing<byte>())
            return VectorizedWriteToUtf16BufferCore(source, destination, length);
        return ScalarizedWriteToUtf16BufferCore(ref source, ref destination, ref length);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static partial byte* VectorizedReadFromUtf16BufferCore_OutOfLatin1Range(char* source, byte* destination, nuint length);

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static partial byte* VectorizedReadFromUtf16BufferCore(char* source, byte* destination, nuint length);

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static partial char* VectorizedWriteToUtf16BufferCore(byte* source, char* destination, nuint length);

    [Inline(InlineBehavior.Remove)]
    private static byte* ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(ref char* source, ref byte* destination, ref nuint length)
    {
        for (; length >= 4; length -= 4, source += 4, destination += 4)
        {
            destination[0] = ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(source[0]);
            destination[1] = ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(source[1]);
            destination[2] = ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(source[2]);
            destination[3] = ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(source[3]);
        }
        char* sourceEnd = source + length;
        byte* destinationEnd = destination + length;
        if (source >= sourceEnd)
            goto Result;
        *destination++ = ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(*source++);
        if (source >= sourceEnd)
            goto Result;
        *destination++ = ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(*source++);
        if (source >= sourceEnd)
            goto Result;
        *destination = ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(*source);

    Result:
        return destinationEnd;
    }

    [Inline(InlineBehavior.Remove)]
    private static byte ScalarizedReadFromUtf16BufferCore_OutOfLatin1Range(char source)
    {
        const byte ReplaceCharacter = 0x003F;
#if NET8_0_OR_GREATER
        return source > Latin1EncodingLimit ? ReplaceCharacter : unchecked((byte)source);
#else
        nuint mask = unchecked((nuint)(-Helpers.MathHelper.BooleanToNative(source > Latin1EncodingLimit)));
        return unchecked((byte)(ReplaceCharacter & mask | source & ~mask));
#endif
    }

    [Inline(InlineBehavior.Remove)]
    private static byte* ScalarizedReadFromUtf16BufferCore(ref char* source, ref byte* destination, ref nuint length)
    {
        for (; length >= 4; length -= 4, source += 4, destination += 4)
        {
            destination[0] = unchecked((byte)source[0]);
            destination[1] = unchecked((byte)source[1]);
            destination[2] = unchecked((byte)source[2]);
            destination[3] = unchecked((byte)source[3]);
        }
        char* sourceEnd = source + length;
        byte* destinationEnd = destination + length;
        if (source >= sourceEnd)
            goto Result;
        *destination++ = unchecked((byte)*source++);
        if (source >= sourceEnd)
            goto Result;
        *destination++ = unchecked((byte)*source++);
        if (source >= sourceEnd)
            goto Result;
        *destination = unchecked((byte)*source);

    Result:
        return destinationEnd;
    }

    [Inline(InlineBehavior.Remove)]
    private static char* ScalarizedWriteToUtf16BufferCore(ref byte* source, ref char* destination, ref nuint length)
    {
        for (; length >= 4; length -= 4, source += 4, destination += 4)
        {
            destination[0] = unchecked((char)source[0]);
            destination[1] = unchecked((char)source[1]);
            destination[2] = unchecked((char)source[2]);
            destination[3] = unchecked((char)source[3]);
        }
        byte* sourceEnd = source + length;
        char* destinationEnd = destination + length;
        if (source >= sourceEnd)
            goto Result;
        *destination++ = unchecked((char)*source++);
        if (source >= sourceEnd)
            goto Result;
        *destination++ = unchecked((char)*source++);
        if (source >= sourceEnd)
            goto Result;
        *destination = unchecked((char)*source);

    Result:
        return destinationEnd;
    }
}
