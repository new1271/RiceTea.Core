using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using InlineMethod;

using RiceTea.Core.IO.Internals;

namespace RiceTea.Core.IO;

public static partial class StreamReaders
{
    public static readonly IStreamReader Null = new NullStreamReader();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IStreamReader CreateStreamReader(Stream stream)
        => new StreamReaderWrapper(stream);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IStreamReader CreateStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
        => CreateStreamReader(stream, detectEncodingFromByteOrderMarks, Encoding.Default, bufferSize: 1024, leaveOpen: false);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IStreamReader CreateStreamReader(Stream stream, Encoding encoding)
        => CreateStreamReader(stream, detectEncodingFromByteOrderMarks: false, encoding, bufferSize: 1024, leaveOpen: false);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IStreamReader CreateStreamReader(Stream stream, Encoding encoding, int bufferSize)
        => CreateStreamReader(stream, detectEncodingFromByteOrderMarks: false, encoding, bufferSize, leaveOpen: false);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IStreamReader CreateStreamReader(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
        => CreateStreamReader(stream, detectEncodingFromByteOrderMarks: false, encoding, bufferSize, leaveOpen);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IStreamReader CreateStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks, Encoding encoding, int bufferSize, bool leaveOpen)
    {
        if (detectEncodingFromByteOrderMarks)
            goto Fallback;

        switch (encoding.CodePage)
        {
            case 65001: // UTF-8
                return new Utf8StreamReader(stream, bufferSize, leaveOpen);
            case 28591: // Latin-1
                return new Latin1StreamReader(stream, bufferSize, leaveOpen);
            case 20127:
                return new AsciiStreamReader(stream, bufferSize, leaveOpen);
            default:
                goto Fallback;
        }

    Fallback:
        return new StreamReaderWrapper(stream, detectEncodingFromByteOrderMarks, encoding, bufferSize, leaveOpen);
    }
}
