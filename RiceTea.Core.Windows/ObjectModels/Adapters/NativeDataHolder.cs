using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using RiceTea.Core.Native;

namespace RiceTea.Core.Windows.ObjectModels.Adapters;

public unsafe struct NativeDataHolder : IDisposable
{
    private readonly void* _methodTable;
    private readonly GCHandle _handle;

    private uint _refCount;

    public NativeDataHolder(void* methodTable, CustomUnknownAdapterBase adapter)
    {
        _methodTable = methodTable;
        _handle = GCHandle.Alloc(adapter, GCHandleType.Normal);
        _refCount = 1;
    }

    public uint AddRef() => Atomics.Increment(ref _refCount);

    public uint Release()
    {
        uint referenceCount = Atomics.Add(ref _refCount, unchecked((uint)-1));
        if (referenceCount == 0)
            Dispose();
        return referenceCount;
    }

    public readonly bool TryGetAdapter<T>([NotNullWhen(true)] out T? adapter) where T : CustomUnknownAdapterBase
    {
        adapter = _handle.Target as T;
        return adapter is not null;
    }

    public readonly void Dispose()
    {
        NativeMethods.FreeMemory(_methodTable);
        _handle.Free();
    }
}
