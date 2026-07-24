#if NET8_0_OR_GREATER
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core.Helpers;

namespace RiceTea.Core;

partial class Atomics
{
    private static readonly unsafe delegate* managed<ref int, int, int> _getAndAdd32Func;
    private static readonly unsafe delegate* managed<ref long, long, long> _getAndAdd64Func;

    unsafe static Atomics()
    {
        Type type = typeof(Interlocked);
        _getAndAdd32Func = (delegate*<ref int, int, int>)ReflectionHelper.GetMethodPointer(type, "ExchangeAdd", [typeof(int).MakeByRefType(), typeof(int)],
            typeof(int), BindingFlags.Static | BindingFlags.NonPublic);
        _getAndAdd64Func = (delegate*<ref long, long, long>)ReflectionHelper.GetMethodPointer(type, "ExchangeAdd", [typeof(long).MakeByRefType(), typeof(long)],
            typeof(long), BindingFlags.Static | BindingFlags.NonPublic);
        if (_getAndAdd32Func is null)
            _getAndAdd32Func = &FallbackGetAndAdd;
        if (_getAndAdd64Func is null)
            _getAndAdd64Func = &FallbackGetAndAdd;
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int Add(ref int location, int value) => Interlocked.Add(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint Add(ref uint location, uint value)
        => unchecked((uint)Interlocked.Add(ref UnsafeHelper.As<uint, int>(ref location), (int)value));

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long Add(ref long location, long value) => Interlocked.Add(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong Add(ref ulong location, ulong value)
        => unchecked((ulong)Interlocked.Add(ref UnsafeHelper.As<ulong, long>(ref location), (long)value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint Add(ref nint location, nint value)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => unchecked(Interlocked.Add(ref UnsafeHelper.As<nint, int>(ref location), (int)value)),
            sizeof(long) => unchecked((nint)Interlocked.Add(ref UnsafeHelper.As<nint, long>(ref location), value)),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => unchecked(Interlocked.Add(ref UnsafeHelper.As<nint, int>(ref location), (int)value)),
                sizeof(long) => unchecked((nint)Interlocked.Add(ref UnsafeHelper.As<nint, long>(ref location), value)),
                _ => throw new PlatformNotSupportedException()
            }
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint Add(ref nuint location, nuint value)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => unchecked((nuint)Interlocked.Add(ref UnsafeHelper.As<nuint, int>(ref location), (int)value)),
            sizeof(long) => unchecked((nuint)Interlocked.Add(ref UnsafeHelper.As<nuint, long>(ref location), (long)value)),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => unchecked((nuint)Interlocked.Add(ref UnsafeHelper.As<nuint, int>(ref location), (int)value)),
                sizeof(long) => unchecked((nuint)Interlocked.Add(ref UnsafeHelper.As<nuint, long>(ref location), (long)value)),
                _ => throw new PlatformNotSupportedException()
            }
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int GetAndAdd(ref int location, int value)
    {
        unsafe
        {
            return _getAndAdd32Func(ref location, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint GetAndAdd(ref uint location, uint value)
    {
        unsafe
        {
            return unchecked((uint)_getAndAdd32Func(ref UnsafeHelper.As<uint, int>(ref location), (int)value));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long GetAndAdd(ref long location, long value)
    {
        unsafe
        {
            return _getAndAdd64Func(ref location, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong GetAndAdd(ref ulong location, ulong value)
    {
        unsafe
        {
            return unchecked((ulong)_getAndAdd64Func(ref UnsafeHelper.As<ulong, long>(ref location), (long)value));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint GetAndAdd(ref nint location, nint value)
    {
        unsafe
        {
            return UnsafeHelper.PointerSizeConstant switch
            {
                sizeof(int) => unchecked(_getAndAdd32Func(ref UnsafeHelper.As<nint, int>(ref location), (int)value)),
                sizeof(long) => unchecked((nint)_getAndAdd64Func(ref UnsafeHelper.As<nint, long>(ref location), value)),
                _ => UnsafeHelper.PointerSize switch
                {
                    sizeof(int) => unchecked(_getAndAdd32Func(ref UnsafeHelper.As<nint, int>(ref location), (int)value)),
                    sizeof(long) => unchecked((nint)_getAndAdd64Func(ref UnsafeHelper.As<nint, long>(ref location), value)),
                    _ => throw new PlatformNotSupportedException()
                }
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint GetAndAdd(ref nuint location, nuint value)
    {
        unsafe
        {
            return UnsafeHelper.PointerSizeConstant switch
            {
                sizeof(int) => unchecked((nuint)_getAndAdd32Func(ref UnsafeHelper.As<nuint, int>(ref location), (int)value)),
                sizeof(long) => unchecked((nuint)_getAndAdd64Func(ref UnsafeHelper.As<nuint, long>(ref location), (long)value)),
                _ => UnsafeHelper.PointerSize switch
                {
                    sizeof(int) => unchecked((nuint)_getAndAdd32Func(ref UnsafeHelper.As<nuint, int>(ref location), (int)value)),
                    sizeof(long) => unchecked((nuint)_getAndAdd64Func(ref UnsafeHelper.As<nuint, long>(ref location), (long)value)),
                    _ => throw new PlatformNotSupportedException()
                }
            };
        }
    }

    /// <inheritdoc cref="Interlocked.Or(ref int, int)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int Or(ref int location, int value) => Interlocked.Or(ref location, value);

    /// <inheritdoc cref="Interlocked.Or(ref uint, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint Or(ref uint location, uint value) => Interlocked.Or(ref location, value);

    /// <inheritdoc cref="Interlocked.Or(ref long, long)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long Or(ref long location, long value) => Interlocked.Or(ref location, value);

    /// <inheritdoc cref="Interlocked.Or(ref ulong, ulong)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong Or(ref ulong location, ulong value) => Interlocked.Or(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint Or(ref nint location, nint value)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => Or(ref UnsafeHelper.As<nint, int>(ref location), (int)value),
            sizeof(long) => (nint)Or(ref UnsafeHelper.As<nint, long>(ref location), value),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => Or(ref UnsafeHelper.As<nint, int>(ref location), (int)value),
                sizeof(long) => (nint)Or(ref UnsafeHelper.As<nint, long>(ref location), value),
                _ => throw new PlatformNotSupportedException()
            }
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint Or(ref nuint location, nuint value)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(uint) => Or(ref UnsafeHelper.As<nuint, uint>(ref location), (uint)value),
            sizeof(ulong) => (nuint)Or(ref UnsafeHelper.As<nuint, ulong>(ref location), value),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(uint) => Or(ref UnsafeHelper.As<nuint, uint>(ref location), (uint)value),
                sizeof(ulong) => (nuint)Or(ref UnsafeHelper.As<nuint, ulong>(ref location), value),
                _ => throw new PlatformNotSupportedException()
            }
        };

    /// <inheritdoc cref="Interlocked.And(ref int, int)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int And(ref int location, int value) => Interlocked.And(ref location, value);

    /// <inheritdoc cref="Interlocked.And(ref uint, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint And(ref uint location, uint value) => Interlocked.And(ref location, value);

    /// <inheritdoc cref="Interlocked.And(ref long, long)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long And(ref long location, long value) => Interlocked.And(ref location, value);

    /// <inheritdoc cref="Interlocked.And(ref ulong, ulong)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong And(ref ulong location, ulong value) => Interlocked.And(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint And(ref nint location, nint value)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => And(ref UnsafeHelper.As<nint, int>(ref location), (int)value),
            sizeof(long) => (nint)And(ref UnsafeHelper.As<nint, long>(ref location), value),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => And(ref UnsafeHelper.As<nint, int>(ref location), (int)value),
                sizeof(long) => (nint)And(ref UnsafeHelper.As<nint, long>(ref location), value),
                _ => throw new PlatformNotSupportedException()
            }
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint And(ref nuint location, nuint value)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(uint) => And(ref UnsafeHelper.As<nuint, uint>(ref location), (uint)value),
            sizeof(ulong) => (nuint)And(ref UnsafeHelper.As<nuint, ulong>(ref location), value),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(uint) => And(ref UnsafeHelper.As<nuint, uint>(ref location), (uint)value),
                sizeof(ulong) => (nuint)And(ref UnsafeHelper.As<nuint, ulong>(ref location), value),
                _ => throw new PlatformNotSupportedException()
            }
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int Min(ref int location, int value) => MinCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint Min(ref uint location, uint value) => MinCoreUnsigned(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long Min(ref long location, long value) => MinCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong Min(ref ulong location, ulong value) => MinCoreUnsigned(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint Min(ref nint location, nint value) => MinCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint Min(ref nuint location, nuint value) => MinCoreUnsigned(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial float Min(ref float location, float value) => MinCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial double Min(ref double location, double value) => MinCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int Max(ref int location, int value) => MaxCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint Max(ref uint location, uint value) => MaxCoreUnsigned(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long Max(ref long location, long value) => MaxCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong Max(ref ulong location, ulong value) => MaxCoreUnsigned(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint Max(ref nint location, nint value) => MaxCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint Max(ref nuint location, nuint value) => MaxCoreUnsigned(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial float Max(ref float location, float value) => MaxCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial double Max(ref double location, double value) => MaxCore(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int Read(ref readonly int location)
    {
        if (UnsafeHelper.PointerSize >= sizeof(int))
            return Volatile.Read(ref UnsafeHelper.AsRefIn(in location));
        return Interlocked.CompareExchange(ref UnsafeHelper.AsRefIn(in location), 0, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint Read(ref readonly uint location)
    {
        if (UnsafeHelper.PointerSize >= sizeof(uint))
            return Volatile.Read(ref UnsafeHelper.AsRefIn(in location));
        return UnsafeHelper.As<int, uint>(Interlocked.CompareExchange(ref UnsafeHelper.As<uint, int>(ref UnsafeHelper.AsRefIn(in location)), 0, 0));
    }

    /// <inheritdoc cref="Interlocked.Read(ref readonly long)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long Read(ref readonly long location)
    {
        if (UnsafeHelper.PointerSize >= sizeof(long))
            return Volatile.Read(ref UnsafeHelper.AsRefIn(in location));
        return Interlocked.CompareExchange(ref UnsafeHelper.AsRefIn(in location), 0L, 0L);
    }

    /// <inheritdoc cref="Interlocked.Read(ref readonly ulong)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong Read(ref readonly ulong location)
    {
        if (UnsafeHelper.PointerSize >= sizeof(ulong))
            return Volatile.Read(ref UnsafeHelper.AsRefIn(in location));
        return UnsafeHelper.As<long, ulong>(Interlocked.Read(ref UnsafeHelper.As<ulong, long>(ref UnsafeHelper.AsRefIn(in location))));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint Read(ref readonly nint location) => Volatile.Read(ref UnsafeHelper.AsRefIn(in location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint Read(ref readonly nuint location) => Volatile.Read(ref UnsafeHelper.AsRefIn(in location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial float Read(ref readonly float location)
    {
        if (UnsafeHelper.PointerSize >= sizeof(float))
            return Volatile.Read(ref UnsafeHelper.AsRefIn(in location));
        return Interlocked.CompareExchange(ref UnsafeHelper.AsRefIn(in location), 0.0f, 0.0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial double Read(ref readonly double location)
    {
        if (UnsafeHelper.PointerSize >= sizeof(double))
            return Volatile.Read(ref UnsafeHelper.AsRefIn(in location));
        return Interlocked.CompareExchange(ref UnsafeHelper.AsRefIn(in location), 0.0, 0.0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial object? Read(ref readonly object? location) => Volatile.Read(ref UnsafeHelper.AsRefIn(in location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T? Read<T>(ref readonly T? location) where T : class => Volatile.Read(ref UnsafeHelper.AsRefIn(in location));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write(ref int location, int value)
    {
        if (UnsafeHelper.PointerSize >= sizeof(int))
            Volatile.Write(ref location, value);
        else
            Exchange(ref location, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write(ref uint location, uint value)
    {
        if (UnsafeHelper.PointerSize >= sizeof(uint))
            Volatile.Write(ref location, value);
        else
            Exchange(ref location, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write(ref long location, long value)
    {
        if (UnsafeHelper.PointerSize >= sizeof(long))
            Volatile.Write(ref location, value);
        else
            Exchange(ref location, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write(ref ulong location, ulong value)
    {
        if (UnsafeHelper.PointerSize >= sizeof(ulong))
            Volatile.Write(ref location, value);
        else
            Exchange(ref location, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write(ref nint location, nint value) => Volatile.Write(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write(ref nuint location, nuint value) => Volatile.Write(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write(ref float location, float value)
    {
        if (UnsafeHelper.PointerSize >= sizeof(float))
            Volatile.Write(ref location, value);
        else
            Exchange(ref location, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write(ref double location, double value)
    {
        if (UnsafeHelper.PointerSize >= sizeof(double))
            Volatile.Write(ref location, value);
        else
            Exchange(ref location, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write(ref object? location, object? value) => Volatile.Write(ref location, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write<T>(ref T? location, T? value) where T : class => Volatile.Write(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange(ref int, int)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int Exchange(ref int location, int value)
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange(ref uint, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint Exchange(ref uint location, uint value)
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange(ref long, long)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long Exchange(ref long location, long value)
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange(ref ulong, ulong)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong Exchange(ref ulong location, ulong value)
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange(ref nint, nint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint Exchange(ref nint location, nint value)
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange(ref nuint, nuint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint Exchange(ref nuint location, nuint value)
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange(ref float, float)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial float Exchange(ref float location, float value)
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange(ref double, double)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial double Exchange(ref double location, double value)
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange(ref object?, object?)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial object? Exchange(ref object? location, object? value)
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.Exchange{T}(ref T, T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T Exchange<T>(ref T location, T value) where T : class?
        => Interlocked.Exchange(ref location, value);

    /// <inheritdoc cref="Interlocked.CompareExchange(ref int, int, int)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int CompareExchange(ref int location, int value, int comparand)
        => Interlocked.CompareExchange(ref location, value, comparand);

    /// <inheritdoc cref="Interlocked.CompareExchange(ref uint, uint, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint CompareExchange(ref uint location, uint value, uint comparand)
        => Interlocked.CompareExchange(ref location, value, comparand);

    /// <inheritdoc cref="Interlocked.CompareExchange(ref long, long, long)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long CompareExchange(ref long location, long value, long comparand)
        => Interlocked.CompareExchange(ref location, value, comparand);

    /// <inheritdoc cref="Interlocked.CompareExchange(ref ulong, ulong, ulong)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong CompareExchange(ref ulong location, ulong value, ulong comparand)
        => Interlocked.CompareExchange(ref location, value, comparand);

    /// <inheritdoc cref="Interlocked.CompareExchange(ref nint, nint, nint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint CompareExchange(ref nint location, nint value, nint comparand)
        => Interlocked.CompareExchange(ref location, value, comparand);

    /// <inheritdoc cref="Interlocked.CompareExchange(ref nuint, nuint, nuint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint CompareExchange(ref nuint location, nuint value, nuint comparand)
        => Interlocked.CompareExchange(ref location, value, comparand);

    /// <inheritdoc cref="Interlocked.CompareExchange(ref float, float, float)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial float CompareExchange(ref float location, float value, float comparand)
        => Interlocked.CompareExchange(ref location, value, comparand);

    /// <inheritdoc cref="Interlocked.CompareExchange(ref double, double, double)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial double CompareExchange(ref double location, double value, double comparand)
        => Interlocked.CompareExchange(ref location, value, comparand);

    /// <inheritdoc cref="Interlocked.CompareExchange(ref object?, object?, object?)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial object? CompareExchange(ref object? location, object? value, object? comparand)
        => Interlocked.CompareExchange(ref location, value, comparand);

    /// <inheritdoc cref="Interlocked.CompareExchange{T}(ref T, T, T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T CompareExchange<T>(ref T location, T value, T comparand) where T : class?
        => Interlocked.CompareExchange(ref location, value, comparand);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int Increment(ref int location) => Add(ref location, 1);

    /// <inheritdoc cref="Interlocked.Increment(ref uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint Increment(ref uint location) => Add(ref location, 1U);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long Increment(ref long location) => Add(ref location, 1L);

    /// <inheritdoc cref="Interlocked.Increment(ref ulong)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong Increment(ref ulong location) => Add(ref location, 1UL);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint Increment(ref nint location) => Add(ref location, 1);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint Increment(ref nuint location) => Add(ref location, 1);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int GetAndIncrement(ref int location) => GetAndAdd(ref location, 1);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint GetAndIncrement(ref uint location) => GetAndAdd(ref location, 1U);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long GetAndIncrement(ref long location) => GetAndAdd(ref location, 1L);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong GetAndIncrement(ref ulong location) => GetAndAdd(ref location, 1UL);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint GetAndIncrement(ref nint location) => GetAndAdd(ref location, 1);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint GetAndIncrement(ref nuint location) => GetAndAdd(ref location, 1);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int Decrement(ref int location) => Add(ref location, -1);

    /// <inheritdoc cref="Interlocked.Decrement(ref uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint Decrement(ref uint location) => Add(ref location, uint.MaxValue);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long Decrement(ref long location) => Add(ref location, -1L);

    /// <inheritdoc cref="Interlocked.Decrement(ref ulong)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong Decrement(ref ulong location) => Add(ref location, ulong.MaxValue);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint Decrement(ref nint location) => Add(ref location, -1);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint Decrement(ref nuint location) => Add(ref location, UnsafeHelper.GetMaxValue<nuint>());

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int GetAndDecrement(ref int location) => GetAndAdd(ref location, -1);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint GetAndDecrement(ref uint location) => GetAndAdd(ref location, uint.MaxValue);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial long GetAndDecrement(ref long location) => GetAndAdd(ref location, -1L);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ulong GetAndDecrement(ref ulong location) => GetAndAdd(ref location, ulong.MaxValue);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint GetAndDecrement(ref nint location) => GetAndAdd(ref location, -1);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint GetAndDecrement(ref nuint location) => GetAndAdd(ref location, UnsafeHelper.GetMaxValue<nuint>());
}
#endif