using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RiceTea.Core.Threading;

/// <inheritdoc cref="Lazy{T}"/>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public ref struct LazyTinyRef<T, TState> where T : class
{
    private readonly Func<TState, T> _factory;
    private readonly TState _state;

    private T? _value;

    /// <inheritdoc cref="Lazy{T}.Lazy(Func{T})"/>
    public LazyTinyRef(Func<TState, T> factory, TState state)
    {
        _factory = factory;
        _state = state;
        _value = null;
    }

    /// <inheritdoc cref="Lazy{T}.IsValueCreated"/>
    public readonly bool IsValueCreated
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value is not null;
    }

    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value ?? LazyTinyHelper.InitializeAndReturn(ref _value, _factory, _state, false, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T? GetValueDirectly() => _value;
}
