using System;
using System.Runtime.CompilerServices;

namespace RiceTea.Core.Native;

unsafe partial class NativeObject
{
    public static T? FromNativePointer<T>(void* nativePointer, ReferenceType referenceType) where T : NativeObject, new()
    {
        if (nativePointer == null)
            return null;
        T result = new T();
        result.LateBind(nativePointer, referenceType);
        return result;
    }

    public static NativeObject? FromNativePointer(Type objectType, void* nativePointer, ReferenceType referenceType)
    {
        if (nativePointer == null || !typeof(NativeObject).IsAssignableFrom(objectType))
            return null;
        object? rawResult = Activator.CreateInstance(objectType);
        if (rawResult is null)
            return null;
        if (rawResult is not NativeObject result)
        {
            (rawResult as IDisposable)?.Dispose();
            return null;
        }
        result.LateBind(nativePointer, referenceType);
        return result;
    }

    public static NativeObject? Clone(NativeObject? obj)
    {
        if (obj is null)
            return null;
        return CloneCore(obj);
    }

    public static T? Clone<T>(T? obj) where T : NativeObject, new()
    {
        if (obj is null)
            return null;
        return CloneCore(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NativeObject? CloneCore(NativeObject obj)
    {
        void* nativePointer = obj.NativePointer;
        ReferenceType referenceType = obj.ReferenceType;
        obj.AfterPointerCopied();
        try
        {
            NativeObject? newObj = FromNativePointer(obj.GetType(), nativePointer, referenceType);
            if (newObj is null)
                goto Failed;
            return newObj;
        }
        catch (Exception)
        {
            goto Failed;
        }

    Failed:
        obj.ReleasePointer(nativePointer);
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T? CloneCore<T>(T obj) where T : NativeObject, new()
    {
        void* nativePointer = obj.NativePointer;
        ReferenceType referenceType = obj.ReferenceType;
        obj.AfterPointerCopied();
        try
        {
            T? newObj = FromNativePointer<T>(nativePointer, referenceType);
            if (newObj is null)
                goto Failed;
            return newObj;
        }
        catch (Exception)
        {
            goto Failed;
        }

    Failed:
        obj.ReleasePointer(nativePointer);
        return null;
    }

    public static NativeObjectReference<T> CloneLater<T>(T? obj) where T : NativeObject, new()
    {
        if (obj is null)
            return default;
        return CloneLaterCore(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NativeObjectReference<T> CloneLaterCore<T>(T obj) where T : NativeObject, new()
    {
        void* nativePointer = obj.NativePointer;
        ReferenceType referenceType = obj.ReferenceType;
        obj.AfterPointerCopied();
        try
        {
            return new NativeObjectReference<T>(nativePointer, referenceType);
        }
        catch (Exception)
        {
            goto Failed;
        }

    Failed:
        obj.ReleasePointer(nativePointer);
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator void*(NativeObject comObject) => comObject is not null ? comObject.NativePointer : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator nint(NativeObject comObject) => comObject is not null ? (nint)comObject._nativePointer : default;
}
