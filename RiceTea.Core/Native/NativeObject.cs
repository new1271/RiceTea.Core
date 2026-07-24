using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace RiceTea.Core.Native;

/// <summary>
/// Represents a native object
/// </summary>
public abstract unsafe partial class NativeObject : CriticalFinalizerObject, ICheckableDisposable
{
    private ReferenceType _referenceType;
    private void* _nativePointer;

    public NativeObject()
    {
        _referenceType = ReferenceType.NeedBinding;
        _nativePointer = null;
    }

    public NativeObject(IntPtr handle, ReferenceType referenceType) : this(handle.ToPointer(), referenceType) { }

    public NativeObject(void* nativePointer, ReferenceType referenceType)
    {
        _nativePointer = referenceType switch
        {
            ReferenceType.NeedBinding => null,
            ReferenceType.Owned or ReferenceType.Weak => nativePointer,
            _ => (void*)ArgumentException.Throw<nuint>("Invalid reference type!", nameof(referenceType)),
        };
        _referenceType = referenceType;
    }

    public void* NativePointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _nativePointer;
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _nativePointer == null;
    }

    public ReferenceType ReferenceType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _referenceType;
    }

    public bool IsDisposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _referenceType == ReferenceType.Disposed;
    }

    protected void LateBind(IntPtr handle, ReferenceType referenceType)
        => LateBind(handle.ToPointer(), referenceType);

    protected void LateBind(void* handle, ReferenceType referenceType)
    {
        if (referenceType == ReferenceType.NeedBinding || _referenceType != ReferenceType.NeedBinding)
            return;
        _referenceType = referenceType;
        _nativePointer = handle;
    }

    protected abstract void AfterPointerCopied();

    protected abstract void ReleasePointer(void* pointer);

    protected virtual void DisposeManaged() { }

    private void DisposeCore(bool disposing)
    {
        ReferenceType oldState = Cells.Exchange(ref _referenceType, ReferenceType.Disposed);
        if (oldState == ReferenceType.Disposed)
            return;

        if (disposing)
            DisposeManaged();

        void* nativePointer = _nativePointer;

        if (nativePointer == null)
            return;

        _nativePointer = null;

        if (oldState == ReferenceType.Owned)
        {
            lock (this)
                ReleasePointer(nativePointer);
        }
    }

    ~NativeObject() => DisposeCore(disposing: false);

    public void Dispose()
    {
        DisposeCore(disposing: true);
        GC.SuppressFinalize(this);
    }
}
