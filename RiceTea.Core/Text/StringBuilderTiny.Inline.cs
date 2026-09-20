using System;
using System.Runtime.CompilerServices;

using InlineMethod;

namespace RiceTea.Core.Text;

unsafe partial struct StringBuilderTiny
{
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char* ptr, int startIndex, int count)
        => Append(ptr + startIndex, count);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char* ptr, char* ptrEnd)
        => Append(ptr, unchecked((int)(ptrEnd - ptr)));

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(object value)
        => Append(value?.ToString() ?? "null");

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append<T>(T value)
        => Append(value?.ToString() ?? "null");

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine()
        => Append(Environment.NewLine);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(char value)
    {
        Append(value);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(char value, int repeatCount)
    {
        Append(value, repeatCount);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(string value)
    {
        Append(value);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(string value, int startIndex, int count)
    {
        Append(value, startIndex, count);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(StringWrapper value)
    {
        Append(value);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(StringWrapper value, int startIndex, int count)
    {
        Append(value, startIndex, count);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(char* ptr, int count)
    {
        Append(ptr, count);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(char* ptr, int startIndex, int count)
    {
        Append(ptr, startIndex, count);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(char* ptr, char* ptrEnd)
    {
        Append(ptr, ptrEnd);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(object value)
    {
        Append(value);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine<T>(T value)
    {
        Append(value);
        AppendLine();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, object value)
        => Insert(index, value?.ToString() ?? "null");

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert<T>(int index, T value)
        => Insert(index, value?.ToString() ?? "null");
}
