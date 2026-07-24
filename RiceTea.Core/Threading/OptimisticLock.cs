using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Threading;

public static class OptimisticLock
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptimisticLock<T> Create<T>() where T : new()
        => new OptimisticLock<T>(new T());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OptimisticLock<T> Create<T>(T value)
        => new OptimisticLock<T>(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Enter(ref readonly nint versionReference) => Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Enter(ref readonly nuint versionReference) => Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(location))]
    public static T? EnterWithObject<T>([NotNullIfNotNull(nameof(location))] ref readonly T? location, 
        ref readonly nint versionReference, out nint version) where T : class
    {
        version = Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));
        return Volatile.Read(ref UnsafeHelper.AsRefIn(in location));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(location))]
    public static T? EnterWithObject<T>([NotNullIfNotNull(nameof(location))] ref readonly T? location, 
        ref readonly nuint versionReference, out nuint version) where T : class
    {
        version = Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));
        return Volatile.Read(ref UnsafeHelper.AsRefIn(in location));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T EnterWithPrimitive<T>(ref readonly T location, ref readonly nint versionReference, out nint version) where T : unmanaged
    {
        version = Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));
        return GenericVolatileRead(in location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T EnterWithPrimitive<T>(ref readonly T location, ref readonly nuint versionReference, out nuint version) where T : unmanaged
    {
        version = Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));
        return GenericVolatileRead(in location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Increase(ref nint versionReference) => Atomics.Increment(ref versionReference);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Increase(ref nuint versionReference) => Atomics.Increment(ref versionReference);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryLeave(ref readonly nint versionReference, ref nint currentVersion)
    {
        nint newVersion = Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));
        if (newVersion == currentVersion)
            return true;
        Volatile.Write(ref currentVersion, newVersion);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryLeave(ref readonly nuint versionReference, ref nuint currentVersion)
    {
        nuint newVersion = Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));
        if (newVersion == currentVersion)
            return true;
        Volatile.Write(ref currentVersion, newVersion);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryLeaveWithObject<T>(ref readonly T? objectReference, ref readonly nint versionReference,
       [NotNullIfNotNull(nameof(objectReference))] ref T? currentObject, ref nint currentVersion) where T : class
    {
        nint newVersion = Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));
        if (newVersion == currentVersion)
            return true;
        Volatile.Write(ref currentVersion, newVersion);
        Volatile.Write(ref currentObject, Volatile.Read(ref UnsafeHelper.AsRefIn(in objectReference)));
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryLeaveWithObject<T>(ref readonly T? objectReference, ref readonly nuint versionReference,
       [NotNullIfNotNull(nameof(objectReference))] ref T? currentObject, ref nuint currentVersion) where T : class
    {
        nuint newVersion = Volatile.Read(ref UnsafeHelper.AsRefIn(in versionReference));
        if (newVersion == currentVersion)
            return true;
        Volatile.Write(ref currentVersion, newVersion);
        Volatile.Write(ref currentObject, Volatile.Read(ref UnsafeHelper.AsRefIn(in objectReference)));
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryLeaveWithPrimitive<T>(ref readonly T primitiveReference, ref readonly nint versionReference,
        ref T currentPrimitive, ref nint currentVersion) where T : unmanaged
    {
        nint newVersion = GenericVolatileRead(in versionReference);
        if (newVersion == currentVersion)
            return true;
        Volatile.Write(ref currentVersion, newVersion);
        GenericVolatileWrite(ref currentPrimitive, GenericVolatileRead(in primitiveReference));
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryLeaveWithPrimitive<T>(ref readonly T primitiveReference, ref readonly nuint versionReference,
        ref T currentPrimitive, ref nuint currentVersion) where T : unmanaged
    {
        nuint newVersion = GenericVolatileRead(in versionReference);
        if (newVersion == currentVersion)
            return true;
        Volatile.Write(ref currentVersion, newVersion);
        GenericVolatileWrite(ref currentPrimitive, GenericVolatileRead(in primitiveReference));
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe T GenericVolatileRead<T>(ref readonly T location) where T : unmanaged
        => sizeof(T) switch
        {
            sizeof(byte) => UnsafeHelper.As<byte, T>(Volatile.Read(ref UnsafeHelper.As<T, byte>(ref UnsafeHelper.AsRefIn(in location)))),
            sizeof(ushort) => UnsafeHelper.As<ushort, T>(Volatile.Read(ref UnsafeHelper.As<T, ushort>(ref UnsafeHelper.AsRefIn(in location)))),
            sizeof(uint) => UnsafeHelper.As<uint, T>(Volatile.Read(ref UnsafeHelper.As<T, uint>(ref UnsafeHelper.AsRefIn(in location)))),
            sizeof(ulong) => UnsafeHelper.As<ulong, T>(Volatile.Read(ref UnsafeHelper.As<T, ulong>(ref UnsafeHelper.AsRefIn(in location)))),
            _ => throw new NotSupportedException($"Unsupported version type: {typeof(T).FullName}")
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void GenericVolatileWrite<T>(ref T location, T value) where T : unmanaged
    {
        switch (sizeof(T))
        {
            case sizeof(byte):
                Volatile.Write(ref UnsafeHelper.As<T, byte>(ref location), UnsafeHelper.As<T, byte>(value));
                break;
            case sizeof(ushort):
                Volatile.Write(ref UnsafeHelper.As<T, ushort>(ref location), UnsafeHelper.As<T, ushort>(value));
                break;
            case sizeof(uint):
                Volatile.Write(ref UnsafeHelper.As<T, uint>(ref location), UnsafeHelper.As<T, uint>(value));
                break;
            case sizeof(ulong):
                Volatile.Write(ref UnsafeHelper.As<T, ulong>(ref location), UnsafeHelper.As<T, ulong>(value));
                break;
            default:
                throw new NotSupportedException($"Unsupported version type: {typeof(T).FullName}");
        }
        ;
    }
}

public sealed class OptimisticLock<T>
{
    private T _value;
    private nuint _version;

    public T Value
    {
        get => GetValueCore();
        set
        {
            _value = value;
            OptimisticLock.Increase(ref _version);
        }
    }

    public OptimisticLock(T value)
    {
        _value = value;
        _version = 0;
    }

    private T GetValueCore()
    {
        T result;
        ref nuint versionRef = ref _version;

        nuint currentVersion = OptimisticLock.Enter(ref versionRef);
        do
        {
            result = _value;
        } while (!OptimisticLock.TryLeave(ref versionRef, ref currentVersion));
        return result;
    }

    public TResult Read<TResult>(Func<T, TResult> factory)
    {
        TResult result;
        ref nuint versionRef = ref _version;
        nuint currentVersion = OptimisticLock.Enter(ref versionRef);
        do
        {
            result = factory.Invoke(_value);
        } while (!OptimisticLock.TryLeave(ref versionRef, ref currentVersion));
        return result;
    }

    public TResult Read<TArg, TResult>(Func<T, TArg, TResult> factory, TArg argument)
    {
        TResult result;
        ref nuint versionRef = ref _version;
        nuint currentVersion = OptimisticLock.Enter(ref versionRef);
        do
        {
            result = factory.Invoke(_value, argument);
        } while (!OptimisticLock.TryLeave(ref versionRef, ref currentVersion));
        return result;
    }

    public void Write(Action<T> action)
    {
        T value = GetValueCore();
        action.Invoke(value);
        OptimisticLock.Increase(ref _version);
    }

    public void Write<TArg>(Action<T, TArg> action, TArg argument)
    {
        T value = GetValueCore();
        action.Invoke(value, argument);
        OptimisticLock.Increase(ref _version);
    }

    public TResult Write<TResult>(Func<T, TResult> factory)
    {
        T value = GetValueCore();
        TResult result = factory.Invoke(value);
        OptimisticLock.Increase(ref _version);
        return result;
    }

    public TResult Write<TArg, TResult>(Func<T, TArg, TResult> factory, TArg argument)
    {
        T value = GetValueCore();
        TResult result = factory.Invoke(value, argument);
        OptimisticLock.Increase(ref _version);
        return result;
    }

    public void MarkChanged() => OptimisticLock.Increase(ref _version);

    public override string? ToString() => GetValueCore()?.ToString();
}
