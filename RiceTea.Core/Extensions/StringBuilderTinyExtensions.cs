using System;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;
using RiceTea.Core.Text;

namespace RiceTea.Core.Extensions;

public static class StringBuilderTinyExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetStartPointer(this StringBuilderTiny _this, in Span<char> span)
    {
        fixed (char* ptr = span)
            _this.SetStartPointer(ptr, span.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetStartPointer(this StringBuilderTiny _this, in ReadOnlySpan<char> span)
    {
        fixed (char* ptr = span)
            _this.SetStartPointer(ptr, span.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Append(this StringBuilderTiny _this, in ReadOnlySpan<char> span)
    {
        fixed (char* ptr = span)
            _this.Append(ptr, span.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AppendLine(this StringBuilderTiny _this, in ReadOnlySpan<char> span)
    {
        fixed (char* ptr = span)
            _this.AppendLine(ptr, span.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AppendFormat<T>(this StringBuilderTiny _this, string format, params ReadOnlySpan<T> args) where T : unmanaged
    {
        fixed (T* ptr = args)
            _this.AppendFormatCore(format, ptr, args.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AppendFormat<T>(this StringBuilderTiny _this, StringWrapper format, params ReadOnlySpan<T> args) where T : unmanaged
    {
        fixed (T* ptr = args)
            _this.AppendFormatCore(format, ptr, args.Length);
    }
}
