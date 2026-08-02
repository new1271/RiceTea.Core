using System;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Threading;

/// <inheritdoc cref="Lazy{T}"/>
public sealed class LazyTiny<T, TState> where T : class
{
    private readonly Func<TState, T> _factory;
    private readonly Lock? _syncLock;
    private readonly TState _state;
    private readonly bool _isThreadSafe;

    private T? _value;

    /// <inheritdoc cref="Lazy{T}.Lazy(Func{T})"/>
    public LazyTiny(Func<TState, T> factory, TState state) : this(factory, state, false, null) { }

    /// <inheritdoc cref="Lazy{T}.Lazy(Func{T}, bool)"/>
    public LazyTiny(Func<TState, T> factory, TState state, bool isThreadSafe) : this(factory, state, isThreadSafe, isThreadSafe ? new Lock() : null) { }

    /// <inheritdoc cref="Lazy{T}.Lazy(Func{T}, LazyThreadSafetyMode)"/>
    public LazyTiny(Func<TState, T> factory, TState state, LazyThreadSafetyMode mode) :
        this(factory, state, mode != LazyThreadSafetyMode.None, mode == LazyThreadSafetyMode.ExecutionAndPublication ? new Lock() : null)
    { }

    private LazyTiny(Func<TState, T> factory, TState state, bool isThreadSafe, Lock? syncLock)
    {
        _isThreadSafe = isThreadSafe;
        _factory = factory;
        _state = state;
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
            return result ?? LazyTinyHelper.InitializeAndReturn(ref _value, _factory, _state, _isThreadSafe, _syncLock);
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
            DisposeHelper.SwapDisposeInterlockedWeak(ref _value);
        else
            DisposeHelper.SwapDisposeWeak(ref _value);
    }
}
