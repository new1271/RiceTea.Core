#if !NET6_0_OR_GREATER
#pragma warning disable IDE0130

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Text;

namespace System.Runtime.CompilerServices;

/// <summary>Provides a handler used by the language compiler to process interpolated strings into <see cref="string"/> instances.</summary>
[InterpolatedStringHandler]
public ref struct DefaultInterpolatedStringHandler : IDisposable
{
    private readonly TypedNativeMemoryBlock<char> _memoryBlock;
    private StringBuilderTiny _builder;

    public unsafe DefaultInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        const int PreservedLengthPerFormatArg = 32;

        _memoryBlock = NativeMemoryPool.Shared.Rent<char>(literalLength + PreservedLengthPerFormatArg * formattedCount);

        _builder = new StringBuilderTiny();
        _builder.SetStartPointer(_memoryBlock.NativePointer, _memoryBlock.Length);
    }

    public void AppendLiteral(string value) => _builder.Append(value);

    public void AppendFormatted<T>(T value) => _builder.Append(value);

    public void AppendFormatted<T>(T value, string? format)
    {
        if (StringHelper.IsNullOrEmpty(format))
        {
            _builder.Append(value);
            return;
        }

        string literalValue;
        if (value is IFormattable formattable)
            literalValue = formattable.ToString(format, null);
        else
            literalValue = value?.ToString() ?? "null";

        _builder.Append(literalValue);
    }

    public void AppendFormatted<T>(T value, int alignment)
    => AppendFormatted(value, alignment, format: null);

    public void AppendFormatted<T>(T value, int alignment, string? format)
    {
        string literalValue;
        if (StringHelper.IsNullOrEmpty(format))
        {
            literalValue = value?.ToString() ?? "null";
        }
        else
        {
            literalValue = value is IFormattable formattable
                ? formattable.ToString(format, null)
                : value?.ToString() ?? "null";
        }

        if (alignment != 0)
        {
            int absAlignment = MathHelper.Abs(alignment);
            if (literalValue.Length < absAlignment)
            {
                literalValue = alignment > 0
                    ? literalValue.PadLeft(absAlignment)
                    : literalValue.PadRight(absAlignment);
            }
        }

        _builder.Append(literalValue);
    }

    public override readonly string ToString() => _builder.ToString();

    public readonly string ToStringAndClear()
    {
        string result = ToString();
        Clear();
        return result;
    }

    public readonly void Clear() => Dispose();

    public readonly void Dispose()
    {
        _builder.Dispose();
        NativeMemoryPool.Shared.Return(_memoryBlock);
    }
}
#endif