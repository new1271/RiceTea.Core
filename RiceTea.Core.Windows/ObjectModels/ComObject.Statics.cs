using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Windows.Internals;

namespace RiceTea.Core.Windows.ObjectModels;

unsafe partial class ComObject
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? CoCreateInstance<T>(in Guid clsid, in Guid iid, bool throwWhenFailed = true) where T : ComObject, new()
    {
        void* nativePointer = CoCreateInstanceCore(clsid, iid, throwWhenFailed);
        return FromNativePointer<T>(nativePointer, ReferenceType.Owned);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComObject? CoCreateInstance(in Guid clsid, bool throwWhenFailed = true)
    {
        void* nativePointer = CoCreateInstanceCore(clsid, IID_IUnknown, throwWhenFailed);
        return nativePointer == null ? null : new ComObject(nativePointer, ReferenceType.Owned);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* CoCreateInstanceCore(in Guid clsid, in Guid iid, bool throwWhenFailed)
    {
        fixed (Guid* rclsid = &clsid, riid = &iid)
            return CoCreateInstanceCore(rclsid, riid, throwWhenFailed);
    }

    [Inline(InlineBehavior.Remove)]
    private static void* CoCreateInstanceCore(Guid* rclsid, Guid* riid, bool throwWhenFailed)
    {
        void* nativePointer;
        int hr = Ole32.CoCreateInstance(rclsid, null, ClassContextFlags.InProcessServer, riid, &nativePointer);
        if (throwWhenFailed)
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
        else
            ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
        return nativePointer;
    }

    public new static ComObjectReference<T> CloneLater<T>(T? obj) where T : ComObject, new()
    {
        if (obj is null || obj.IsEmpty || obj.IsDisposed)
            return default;
        return CloneLaterCore(obj, obj.ReferenceType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ComObjectReference<T> CloneLaterCore<T>(T obj, ReferenceType pointerType) where T : ComObject, new()
    {
        void* nativePointer = obj.NativePointer;
        if (nativePointer is null)
            return default;
        if (pointerType == ReferenceType.Owned)
            AddRefCore(nativePointer);
        return new ComObjectReference<T>(nativePointer, pointerType);
    }
}
