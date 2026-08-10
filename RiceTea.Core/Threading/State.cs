using System;
using System.Runtime.CompilerServices;

namespace RiceTea.Core.Threading;

public abstract class State<T> where T : struct
{
    private T _value;
    private nuint _version;

    public abstract T Value { get; set; }

    protected State(T value) => _value = value;

    public abstract void Update(Func<T, T> updateFactory);

    public abstract void Update<TState>(Func<T, TState, T> updateFactory, TState state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueUnsafe() => _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly T GetValueReferenceUnsafe() => ref _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected T GetValue() => StateHelper.GetValue(in _value, in _version);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected T GetValue(out nuint version) => StateHelper.GetValue(in _value, in _version, out version);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetValue(T value) => StateHelper.SetValue(ref _value, ref _version, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool TrySetValue(T value, nuint version) => StateHelper.TrySetValue(ref _value, ref _version, value, version);
}

public static partial class State
{
    public static State<T> OfSingleWriter<T>(T initialValue) where T : struct => new SingleWriterImpl<T>(initialValue);

    public static State<T> OfMultipleWriter<T>(T initialValue) where T : struct => new MultipleWriterImpl<T>(initialValue);
}
