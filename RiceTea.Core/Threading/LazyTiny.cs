using System;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Threading;

/// <inheritdoc cref="Lazy{T}"/>
public sealed class LazyTiny<T> where T : class
{
    private readonly Func<T>? _factory;
    private readonly Lock? _syncLock;
    private readonly bool _isThreadSafe;

    private T? _value;

    /// <inheritdoc cref="Lazy{T}.Lazy(LazyThreadSafetyMode)"/>
    public LazyTiny(bool isThreadSafe) : this(null, isThreadSafe) { }

    /// <inheritdoc cref="Lazy{T}.Lazy(LazyThreadSafetyMode)"/>
    public LazyTiny(LazyThreadSafetyMode mode) : this(null, mode) { }

    /// <inheritdoc cref="Lazy{T}.Lazy(Func{T})"/>
    public LazyTiny(Func<T>? factory) : this(factory, false, null) { }

    /// <inheritdoc cref="Lazy{T}.Lazy(Func{T}, bool)"/>
    public LazyTiny(Func<T>? factory, bool isThreadSafe) : this(factory, isThreadSafe, isThreadSafe ? new Lock() : null) { }

    /// <inheritdoc cref="Lazy{T}.Lazy(Func{T}, LazyThreadSafetyMode)"/>
    public LazyTiny(Func<T>? factory, LazyThreadSafetyMode mode) :
        this(factory, mode != LazyThreadSafetyMode.None, mode == LazyThreadSafetyMode.ExecutionAndPublication ? new Lock() : null)
    { }

    public LazyTiny(T value)
    {
        _isThreadSafe = true;
        _syncLock = null;
        _factory = null;
        _value = value;
    }

    private LazyTiny(Func<T>? factory, bool isThreadSafe, Lock? syncLock)
    {
        _isThreadSafe = isThreadSafe;
        _factory = factory;
        _syncLock = syncLock;
        _value = null;
    }

    /// <inheritdoc cref="Lazy{T}.IsValueCreated"/>
    public bool IsValueCreated
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value is not null;
    }

    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            T? result = _value;
            return result ?? LazyTinyHelper.InitializeAndReturn(ref _value, _factory, _isThreadSafe, _syncLock);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetValueDirectly()
    {
        if (!_isThreadSafe)
            return _value;
        else
        {
            T? result = _value;
            if (result is not null)
                return result;
            Lock? syncLock = _syncLock;
            if (syncLock is null)
                return Volatile.Read(ref _value);
            else
                lock (syncLock)
                    return _value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        if (_isThreadSafe)
            DisposeHelper.SwapDisposeAtomicWeak(ref _value);
        else
            DisposeHelper.SwapDisposeWeak(ref _value);
    }
}
