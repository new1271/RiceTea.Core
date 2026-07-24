using System;
using System.Threading;

using InlineMethod;

namespace RiceTea.Core.Native;

public abstract class DelayedCollectingObject : ICheckableDisposable
{
    private ulong _disposed, _created, _refCount, _lastDerefTime;

    public bool IsDisposed => CheckDisposed();

    public bool IsCreated => Volatile.Read(ref _created) > 0;

    public bool IsInReference => Atomics.Read(ref _refCount) > 0;

    public ulong LastDereferenceTime => Atomics.Read(ref _lastDerefTime);

    protected DelayedCollectingObject()
    {
        _disposed = 0;
        _created = 0;
        _refCount = 0;
        _lastDerefTime = 0;
    }

    public void AddRef()
    {
        if (CheckDisposed() || Atomics.LimitedIncrement(ref _refCount, ulong.MaxValue) != 1 || !TryGenerateObject())
            return;
        DelayedCollector.Instance.AddObject(this);
    }

    public void RemoveRef()
    {
        if (CheckDisposed() || Atomics.LimitedDecrement(ref _refCount, 0) != 0)
            return;
        Atomics.Write(ref _lastDerefTime, NativeMethods.GetTicksForSystem());
    }

    internal void RemoveObject()
    {
        if (CheckDisposed())
            return;
        TryDestroyObject();
    }

    [Inline(InlineBehavior.Remove)]
    private bool TryGenerateObject()
    {
        if (Atomics.Exchange(ref _created, ulong.MaxValue) != 0)
            return false;
        GenerateObject();
        return true;
    }

    [Inline(InlineBehavior.Remove)]
    private void TryDestroyObject()
    {
        if (Atomics.Exchange(ref _created, 0) == 0)
            return;
        DestroyObject();
    }

    protected abstract void GenerateObject();

    protected abstract void DestroyObject();

    [Inline(InlineBehavior.Remove)]
    private bool CheckDisposed() => Volatile.Read(ref _disposed) != 0;

    protected virtual void Dispose(bool disposing)
    {
        if (IsInReference && disposing)
            return;
        if (Atomics.CompareExchange(ref _disposed, 1, 0) == 0)
            TryDestroyObject();
    }

    ~DelayedCollectingObject() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
