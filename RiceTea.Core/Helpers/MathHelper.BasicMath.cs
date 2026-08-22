using System;
using System.Runtime.CompilerServices;

using InlineMethod;

namespace RiceTea.Core.Helpers;

partial class MathHelper
{
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte CheckedAdd(byte a, byte b)
        => unchecked((byte)(a + Min(b, (byte)(byte.MaxValue - a))));

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort CheckedAdd(ushort a, ushort b)
        => unchecked((ushort)(a + Min(b, (ushort)(ushort.MaxValue - a))));

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CheckedAdd(uint a, uint b)
        => a + Min(b, uint.MaxValue - a);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CheckedAdd(ulong a, ulong b)
        => a + Min(b, ulong.MaxValue - a);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public static nuint CheckedAdd(nuint a, nuint b)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(uint) => CheckedAdd((uint)a, (uint)b),
            sizeof(ulong) => (nuint)CheckedAdd((ulong)a, b),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(uint) => CheckedAdd((uint)a, (uint)b),
                sizeof(ulong) => (nuint)CheckedAdd((ulong)a, b),
                _ => throw new NotSupportedException("Unsupported pointer size: " + UnsafeHelper.PointerSize)
            }
        };

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte CheckedSubtract(byte a, byte b)
    {
#if NET8_0_OR_GREATER
        return a > b ? unchecked((byte)(a - b)) : byte.MinValue;
#else
        return unchecked((byte)(a - Min(a, b)));
#endif
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort CheckedSubtract(ushort a, ushort b)
    {
#if NET8_0_OR_GREATER
        return a > b ? unchecked((ushort)(a - b)) : ushort.MinValue;
#else
        return unchecked((ushort)(a - Min(a, b)));
#endif
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CheckedSubtract(uint a, uint b)
    {
#if NET8_0_OR_GREATER
        return a > b ? a - b : uint.MinValue;
#else
        return a - Min(a, b);
#endif
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CheckedSubtract(ulong a, ulong b)
    {
#if NET8_0_OR_GREATER
        return a > b ? a - b : ulong.MinValue;
#else
        return a - Min(a, b);
#endif
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint CheckedSubtract(nuint a, nuint b)
    {
#if NET8_0_OR_GREATER
        return a > b ? a - b : nuint.MinValue;
#else
        return a - Min(a, b);
#endif
    }
}
