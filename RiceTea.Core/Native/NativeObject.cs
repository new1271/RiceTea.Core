using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace RiceTea.Core.Native;

/// <summary>
/// Represents a native object
/// </summary>
public abstract unsafe partial class NativeObject : CriticalFinalizerObject, ICheckableDisposable
{
    private long _referenceType;
    private nuint _nativePointer;

    public NativeObject()
    {
        _referenceType = (long)ReferenceType.NeedBinding;
        _nativePointer = default;
    }

    public NativeObject(IntPtr handle, ReferenceType referenceType) : this(handle.ToPointer(), referenceType) { }

    public NativeObject(void* nativePointer, ReferenceType referenceType)
    {
        _nativePointer = referenceType switch
        {
            ReferenceType.NeedBinding => default,
            ReferenceType.Owned or ReferenceType.Weak => (nuint)nativePointer,
            _ => ArgumentException.Throw<nuint>("Invalid reference type!", nameof(referenceType)),
        };
        _referenceType = (long)referenceType;
    }

    public void* NativePointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (void*)Atomics.Read(ref _nativePointer);
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => NativePointer == default;
    }

    public ReferenceType ReferenceType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (ReferenceType)Atomics.Read(ref _referenceType);
    }

    public bool IsDisposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReferenceType == ReferenceType.Disposed;
    }

    protected void LateBind(IntPtr handle, ReferenceType referenceType)
        => LateBind(handle.ToPointer(), referenceType);

    protected void LateBind(void* handle, ReferenceType referenceType)
    {
        if (referenceType == ReferenceType.NeedBinding ||
            Atomics.CompareExchange(ref _referenceType, (long)referenceType, (long)ReferenceType.NeedBinding) != (long)ReferenceType.NeedBinding)
            return;
        Atomics.Write(ref _nativePointer, (nuint)handle);
    }

    protected abstract void AfterPointerCopied();

    protected abstract void ReleasePointer(void* pointer);

    internal void ReleaseInternal(void* pointer) => ReleasePointer(pointer);

    protected virtual void DisposeManaged() { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void DisposeCore(bool disposing)
    {
        ReferenceType oldState = (ReferenceType)Atomics.Exchange(ref _referenceType, (long)ReferenceType.Disposed);
        if (oldState == ReferenceType.Disposed)
            return;

        try
        {
            if (disposing)
                DisposeManaged();
        }
        finally
        {
            void* nativePointer = (void*)Atomics.Exchange(ref _nativePointer, default);

            if (nativePointer is not null && oldState == ReferenceType.Owned)
                ReleasePointer(nativePointer);
        }
    }

    ~NativeObject() => DisposeCore(disposing: false);

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        DisposeCore(disposing: true);
        AfterUnmanagedCall();
    }
}
