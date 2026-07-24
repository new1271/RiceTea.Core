using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using RiceTea.Core.Native;

namespace RiceTea.Core.Windows.ObjectModels;

[StructLayout(LayoutKind.Auto)]
public unsafe ref struct ComObjectReference<T> : IUnknown where T : ComObject, new()
{
    private readonly ReferenceType _type;
    private void* _handle;

    internal ComObjectReference(void* handle, ReferenceType type)
    {
        _handle = handle;
        _type = type;
    }

    public readonly bool IsEmpty => _handle is null;

    public readonly uint AddRef()
    {
        void* handle = _handle;
        if (handle is null)
            return 0;
        return ComObject.AddRefCore(handle);
    }

    public readonly void* GetWin32Handle() => _handle;

    public readonly uint Release()
    {
        void* handle = _handle;
        if (handle is null)
            return 0;
        return ComObject.ReleaseCore(handle);
    }

    public readonly bool TryQueryInterface(in Guid guid, [NotNullWhen(true)] out IUnknown? queriedObject)
    {
        void* handle = _handle;
        if (handle is null)
        {
            queriedObject = null;
            return false;
        }
        int hr = ComObject.QueryInterfaceCore(ref handle, guid);
        if (hr < 0)
        {
            queriedObject = null;
            return false;
        }
        return (queriedObject = NativeObject.FromNativePointer<ComObject>(handle, ReferenceType.Owned)) is not null;
    }

    public readonly bool TryQueryInterface<TResult>(in Guid guid, [NotNullWhen(true)] out TResult? queriedObject) where TResult : ComObject, new()
    {
        void* handle = _handle;
        if (handle is null)
        {
            queriedObject = null;
            return false;
        }
        int hr = ComObject.QueryInterfaceCore(ref handle, guid);
        if (hr < 0)
        {
            queriedObject = null;
            return false;
        }
        return (queriedObject = NativeObject.FromNativePointer<TResult>(handle, ReferenceType.Owned)) is not null;
    }

    public T GetAndDispose()
    {
        void* handle = Cells.Exchange(ref _handle, null);
        T? result = NativeObject.FromNativePointer<T>(handle, _type);
        if (result is null)
            throw new ObjectDisposedException(nameof(NativeObjectReference<>));
        return result;
    }

    public void Dispose()
    {
        void* handle = Cells.Exchange(ref _handle, null);
        if (handle == null)
            return;

        if (_type == ReferenceType.Owned)
            ComObject.ReleaseCore(handle);
    }
}
