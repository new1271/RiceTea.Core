using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core.Helpers;

namespace RiceTea.Core;

public static partial class Atomics
{
    public static partial int Add(ref int location, int value);
    public static partial uint Add(ref uint location, uint value);
    public static partial long Add(ref long location, long value);
    public static partial ulong Add(ref ulong location, ulong value);
    public static partial nint Add(ref nint location, nint value);
    public static partial nuint Add(ref nuint location, nuint value);

    public static partial int GetAndAdd(ref int location, int value);
    public static partial uint GetAndAdd(ref uint location, uint value);
    public static partial long GetAndAdd(ref long location, long value);
    public static partial ulong GetAndAdd(ref ulong location, ulong value);
    public static partial nint GetAndAdd(ref nint location, nint value);
    public static partial nuint GetAndAdd(ref nuint location, nuint value);

    public static partial int Or(ref int location, int value);
    public static partial uint Or(ref uint location, uint value);
    public static partial long Or(ref long location, long value);
    public static partial ulong Or(ref ulong location, ulong value);
    public static partial nint Or(ref nint location, nint value);
    public static partial nuint Or(ref nuint location, nuint value);

    public static partial int And(ref int location, int value);
    public static partial uint And(ref uint location, uint value);
    public static partial long And(ref long location, long value);
    public static partial ulong And(ref ulong location, ulong value);
    public static partial nint And(ref nint location, nint value);
    public static partial nuint And(ref nuint location, nuint value);

    public static partial int Min(ref int location, int value);
    public static partial uint Min(ref uint location, uint value);
    public static partial long Min(ref long location, long value);
    public static partial ulong Min(ref ulong location, ulong value);
    public static partial nint Min(ref nint location, nint value);
    public static partial nuint Min(ref nuint location, nuint value);
    public static partial float Min(ref float location, float value);
    public static partial double Min(ref double location, double value);

    public static partial int Max(ref int location, int value);
    public static partial uint Max(ref uint location, uint value);
    public static partial long Max(ref long location, long value);
    public static partial ulong Max(ref ulong location, ulong value);
    public static partial nint Max(ref nint location, nint value);
    public static partial nuint Max(ref nuint location, nuint value);
    public static partial float Max(ref float location, float value);
    public static partial double Max(ref double location, double value);

    public static partial int Read(ref readonly int location);
    public static partial uint Read(ref readonly uint location);
    public static partial long Read(ref readonly long location);
    public static partial ulong Read(ref readonly ulong location);
    public static partial nint Read(ref readonly nint location);
    public static partial nuint Read(ref readonly nuint location);
    public static partial float Read(ref readonly float location);
    public static partial double Read(ref readonly double location);
    [return: NotNullIfNotNull(nameof(location))]
    public static partial object? Read([NotNullIfNotNull(nameof(location))] ref readonly object? location);
    [return: NotNullIfNotNull(nameof(location))]
    public static partial T? Read<T>([NotNullIfNotNull(nameof(location))] ref readonly T? location) where T : class;

    public static partial void Write(ref int location, int value);
    public static partial void Write(ref uint location, uint value);
    public static partial void Write(ref long location, long value);
    public static partial void Write(ref ulong location, ulong value);
    public static partial void Write(ref nint location, nint value);
    public static partial void Write(ref nuint location, nuint value);
    public static partial void Write(ref float location, float value);
    public static partial void Write(ref double location, double value);
    public static partial void Write([NotNullIfNotNull(nameof(value))] ref object? location, object? value);
    public static partial void Write<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value) where T : class;

    public static partial int Exchange(ref int location, int value);
    public static partial uint Exchange(ref uint location, uint value);
    public static partial long Exchange(ref long location, long value);
    public static partial ulong Exchange(ref ulong location, ulong value);
    public static partial nint Exchange(ref nint location, nint value);
    public static partial nuint Exchange(ref nuint location, nuint value);
    public static partial float Exchange(ref float location, float value);
    public static partial double Exchange(ref double location, double value);

    [return: NotNullIfNotNull(nameof(location))]
    public static partial object? Exchange([NotNullIfNotNull(nameof(value))] ref object? location, object? value);
    [return: NotNullIfNotNull(nameof(location))]
    public static partial T Exchange<T>([NotNullIfNotNull(nameof(value))] ref T location, T value) where T : class?;

    public static partial int CompareExchange(ref int location, int value, int comparand);
    public static partial uint CompareExchange(ref uint location, uint value, uint comparand);
    public static partial long CompareExchange(ref long location, long value, long comparand);
    public static partial ulong CompareExchange(ref ulong location, ulong value, ulong comparand);
    public static partial nint CompareExchange(ref nint location, nint value, nint comparand);
    public static partial nuint CompareExchange(ref nuint location, nuint value, nuint comparand);
    public static partial float CompareExchange(ref float location, float value, float comparand);
    public static partial double CompareExchange(ref double location, double value, double comparand);

    [return: NotNullIfNotNull(nameof(location))]
    public static partial object? CompareExchange([NotNullIfNotNull(nameof(value))] ref object? location, object? value, object? comparand);

    [return: NotNullIfNotNull(nameof(location))]
    public static partial T CompareExchange<T>([NotNullIfNotNull(nameof(value))] ref T location, T value, T comparand) where T : class?;

    /// <inheritdoc cref="Interlocked.Increment(ref int)"/>
    public static partial int Increment(ref int location);
    /// <inheritdoc cref="Interlocked.Increment(ref int)"/>
    public static partial uint Increment(ref uint location);
    /// <inheritdoc cref="Interlocked.Increment(ref long)"/>
    public static partial long Increment(ref long location);
    /// <inheritdoc cref="Interlocked.Increment(ref long)"/>
    public static partial ulong Increment(ref ulong location);
    /// <inheritdoc cref="Interlocked.Increment(ref long)"/>
    public static partial nint Increment(ref nint location);
    /// <inheritdoc cref="Interlocked.Increment(ref long)"/>
    public static partial nuint Increment(ref nuint location);

    public static partial int GetAndIncrement(ref int location);
    public static partial uint GetAndIncrement(ref uint location);
    public static partial long GetAndIncrement(ref long location);
    public static partial ulong GetAndIncrement(ref ulong location);
    public static partial nint GetAndIncrement(ref nint location);
    public static partial nuint GetAndIncrement(ref nuint location);

    /// <inheritdoc cref="Interlocked.Decrement(ref int)"/>
    public static partial int Decrement(ref int location);
    /// <inheritdoc cref="Interlocked.Decrement(ref int)"/>
    public static partial uint Decrement(ref uint location);
    /// <inheritdoc cref="Interlocked.Decrement(ref long)"/>
    public static partial long Decrement(ref long location);
    /// <inheritdoc cref="Interlocked.Decrement(ref long)"/>
    public static partial ulong Decrement(ref ulong location);
    /// <inheritdoc cref="Interlocked.Decrement(ref long)"/>
    public static partial nint Decrement(ref nint location);
    /// <inheritdoc cref="Interlocked.Decrement(ref long)"/>
    public static partial nuint Decrement(ref nuint location);

    public static partial int GetAndDecrement(ref int location);
    public static partial uint GetAndDecrement(ref uint location);
    public static partial long GetAndDecrement(ref long location);
    public static partial ulong GetAndDecrement(ref ulong location);
    public static partial nint GetAndDecrement(ref nint location);
    public static partial nuint GetAndDecrement(ref nuint location);

    // Slow routines
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T OrCore<T>(ref T location, T value) where T : unmanaged
    {
        T oldValue = CompareExchangeCore(ref location, value, default);
        if (UnsafeHelper.Equals(oldValue, default) || UnsafeHelper.Equals(UnsafeHelper.And(oldValue, value), value))
            return oldValue;
        do
        {
            T currentValue = CompareExchangeCore(ref location, UnsafeHelper.Or(oldValue, value), oldValue);
            if (UnsafeHelper.Equals(currentValue, oldValue))
                return oldValue;
            oldValue = currentValue;
        }
        while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T AndCore<T>(ref T location, T value) where T : unmanaged
    {
        T oldValue = ReadCore(ref location);
        if (UnsafeHelper.Equals(oldValue, value) || UnsafeHelper.Equals(UnsafeHelper.And(oldValue, UnsafeHelper.Not(value)), default))
            return oldValue;
        do
        {
            T currentValue = CompareExchangeCore(ref location, UnsafeHelper.And(oldValue, value), oldValue);
            if (UnsafeHelper.Equals(currentValue, oldValue))
                return oldValue;
            oldValue = currentValue;
        }
        while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T MinCore<T>(ref T location, T value) where T : unmanaged
    {
        T oldValue = ReadCore(ref location);
        if (UnsafeHelper.IsLessThanOrEquals(oldValue, value))
            return oldValue;
        do
        {
            T currentValue = CompareExchangeCore(ref location, value, oldValue);
            if (UnsafeHelper.Equals(currentValue, oldValue))
                break;
            oldValue = currentValue;
        }
        while (UnsafeHelper.IsGreaterThan(oldValue, value));
        return oldValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T MinCoreUnsigned<T>(ref T location, T value) where T : unmanaged
    {
        T oldValue = ReadCore(ref location);
        if (UnsafeHelper.IsLessThanOrEqualsUnsigned(oldValue, value))
            return oldValue;
        do
        {
            T currentValue = CompareExchangeCore(ref location, value, oldValue);
            if (UnsafeHelper.Equals(currentValue, oldValue))
                break;
            oldValue = currentValue;
        }
        while (UnsafeHelper.IsGreaterThanUnsigned(oldValue, value));
        return oldValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T MaxCore<T>(ref T location, T value) where T : unmanaged
    {
        T oldValue = ReadCore(ref location);
        if (UnsafeHelper.IsGreaterThanOrEquals(oldValue, value))
            return oldValue;
        do
        {
            T currentValue = CompareExchangeCore(ref location, value, oldValue);
            if (UnsafeHelper.Equals(currentValue, oldValue))
                break;
            oldValue = currentValue;
        }
        while (UnsafeHelper.IsLessThan(oldValue, value));
        return oldValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T MaxCoreUnsigned<T>(ref T location, T value) where T : unmanaged
    {
        T oldValue = ReadCore(ref location);
        if (UnsafeHelper.IsGreaterThanOrEqualsUnsigned(oldValue, value))
            return oldValue;
        do
        {
            T currentValue = CompareExchangeCore(ref location, value, oldValue);
            if (UnsafeHelper.Equals(currentValue, oldValue))
                break;
            oldValue = currentValue;
        }
        while (UnsafeHelper.IsLessThanUnsigned(oldValue, value));
        return oldValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe T ReadCore<T>(ref T location) where T : unmanaged
        => sizeof(T) switch
        {
            sizeof(uint) => UnsafeHelper.As<uint, T>(Read(ref UnsafeHelper.As<T, uint>(ref location))),
            sizeof(ulong) => UnsafeHelper.As<ulong, T>(Read(ref UnsafeHelper.As<T, ulong>(ref location))),
            _ => throw new PlatformNotSupportedException()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe T ExchangeCore<T>(ref T location, T value) where T : unmanaged
        => sizeof(T) switch
        {
            sizeof(uint) => UnsafeHelper.As<uint, T>(Exchange(ref UnsafeHelper.As<T, uint>(ref location),
                UnsafeHelper.As<T, uint>(value))),
            sizeof(ulong) => UnsafeHelper.As<ulong, T>(Exchange(ref UnsafeHelper.As<T, ulong>(ref location),
                UnsafeHelper.As<T, ulong>(value))),
            _ => throw new PlatformNotSupportedException()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe T CompareExchangeCore<T>(ref T location, T value, T comparand) where T : unmanaged
        => sizeof(T) switch
        {
            sizeof(uint) => UnsafeHelper.As<uint, T>(CompareExchange(ref UnsafeHelper.As<T, uint>(ref location),
                UnsafeHelper.As<T, uint>(value), UnsafeHelper.As<T, uint>(comparand))),
            sizeof(ulong) => UnsafeHelper.As<ulong, T>(CompareExchange(ref UnsafeHelper.As<T, ulong>(ref location),
                UnsafeHelper.As<T, ulong>(value), UnsafeHelper.As<T, ulong>(comparand))),
            _ => throw new PlatformNotSupportedException()
        };

    private static int FallbackGetAndAdd(ref int location, int value) => Interlocked.Add(ref location, value) - value;

    private static long FallbackGetAndAdd(ref long location, long value) => Interlocked.Add(ref location, value) - value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LimitedIncrement(ref int location, int limit)
        => LimitedIncrementCore(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LimitedIncrement(ref uint location, uint limit)
        => LimitedIncrementCoreUnsigned(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long LimitedIncrement(ref long location, long limit)
        => LimitedIncrementCore(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong LimitedIncrement(ref ulong location, ulong limit)
        => LimitedIncrementCoreUnsigned(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint LimitedIncrement(ref nint location, nint limit)
        => LimitedIncrementCore(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint LimitedIncrement(ref nuint location, nuint limit)
        => LimitedIncrementCoreUnsigned(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T LimitedIncrementCore<T>(ref T location, T limit) where T : unmanaged
    {
        T oldValue = ReadCore(ref location);
        do
        {
            T currentValue, newValue;
            if (UnsafeHelper.IsGreaterThanOrEquals(oldValue, limit))
                newValue = currentValue = ReadCore(ref location);
            else
            {
                newValue = UnsafeHelper.Add(oldValue, UnsafeHelper.As<int, T>(1));
                currentValue = CompareExchangeCore(ref location, newValue, oldValue);
            }
            if (UnsafeHelper.Equals(currentValue, oldValue))
                return newValue;
            oldValue = currentValue;
        }
        while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T LimitedIncrementCoreUnsigned<T>(ref T location, T limit) where T : unmanaged
    {
        T oldValue = ReadCore(ref location);
        do
        {
            T currentValue, newValue;
            if (UnsafeHelper.IsGreaterThanOrEqualsUnsigned(oldValue, limit))
                newValue = currentValue = ReadCore(ref location);
            else
            {
                newValue = UnsafeHelper.Add(oldValue, UnsafeHelper.As<int, T>(1));
                currentValue = CompareExchangeCore(ref location, newValue, oldValue);
            }
            if (UnsafeHelper.Equals(currentValue, oldValue))
                return newValue;
            oldValue = currentValue;
        }
        while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LimitedDecrement(ref int location, int limit)
        => LimitedDecrementCore(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LimitedDecrement(ref uint location, uint limit)
        => LimitedDecrementCoreUnsigned(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long LimitedDecrement(ref long location, long limit)
        => LimitedDecrementCore(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong LimitedDecrement(ref ulong location, ulong limit)
        => LimitedDecrementCoreUnsigned(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint LimitedDecrement(ref nint location, nint limit)
        => LimitedDecrementCore(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint LimitedDecrement(ref nuint location, nuint limit)
        => LimitedDecrementCoreUnsigned(ref location, limit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T LimitedDecrementCore<T>(ref T location, T limit) where T : unmanaged
    {
        T oldValue = ReadCore(ref location);
        do
        {
            T currentValue, newValue;
            if (UnsafeHelper.IsLessThanOrEquals(oldValue, limit))
                newValue = currentValue = ReadCore(ref location);
            else
            {
                newValue = UnsafeHelper.Subtract(oldValue, UnsafeHelper.As<int, T>(1));
                currentValue = CompareExchangeCore(ref location, newValue, oldValue);
            }
            if (UnsafeHelper.Equals(currentValue, oldValue))
                return newValue;
            oldValue = currentValue;
        }
        while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T LimitedDecrementCoreUnsigned<T>(ref T location, T limit) where T : unmanaged
    {
        T oldValue = ReadCore(ref location);
        do
        {
            T currentValue, newValue;
            if (UnsafeHelper.IsLessThanOrEqualsUnsigned(oldValue, limit))
                newValue = currentValue = ReadCore(ref location);
            else
            {
                newValue = UnsafeHelper.Subtract(oldValue, UnsafeHelper.As<int, T>(1));
                currentValue = CompareExchangeCore(ref location, newValue, oldValue);
            }
            if (UnsafeHelper.Equals(currentValue, oldValue))
                return newValue;
            oldValue = currentValue;
        }
        while (true);
    }
}
