using System;
using System.Runtime.CompilerServices;

namespace RiceTea.Core.Native;

#pragma warning disable CA1822

partial class NativeObject
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall() => GC.KeepAlive(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall(NativeObject? obj)
    {
        GC.KeepAlive(obj);
        GC.KeepAlive(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall(NativeObject? obj1, NativeObject? obj2)
    {
        GC.KeepAlive(obj1);
        GC.KeepAlive(obj2);
        GC.KeepAlive(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall(NativeObject? obj1, NativeObject? obj2, NativeObject? obj3)
    {
        GC.KeepAlive(obj1);
        GC.KeepAlive(obj2);
        GC.KeepAlive(obj3);
        GC.KeepAlive(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall(NativeObject? obj1, NativeObject? obj2, NativeObject? obj3,
        NativeObject? obj4)
    {
        GC.KeepAlive(obj1);
        GC.KeepAlive(obj2);
        GC.KeepAlive(obj3);
        GC.KeepAlive(obj4);
        GC.KeepAlive(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall(NativeObject? obj1, NativeObject? obj2, NativeObject? obj3,
        NativeObject? obj4, NativeObject? obj5)
    {
        GC.KeepAlive(obj1);
        GC.KeepAlive(obj2);
        GC.KeepAlive(obj3);
        GC.KeepAlive(obj4);
        AfterUnmanagedCall(obj5);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall(NativeObject? obj1, NativeObject? obj2, NativeObject? obj3,
        NativeObject? obj4, NativeObject? obj5, NativeObject? obj6)
    {
        GC.KeepAlive(obj1);
        GC.KeepAlive(obj2);
        GC.KeepAlive(obj3);
        GC.KeepAlive(obj4);
        AfterUnmanagedCall(obj5, obj6);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall(NativeObject? obj1, NativeObject? obj2, NativeObject? obj3,
        NativeObject? obj4, NativeObject? obj5, NativeObject? obj6,
        NativeObject? obj7)
    {
        GC.KeepAlive(obj1);
        GC.KeepAlive(obj2);
        GC.KeepAlive(obj3);
        GC.KeepAlive(obj4);
        AfterUnmanagedCall(obj5, obj6, obj7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall(NativeObject? obj1, NativeObject? obj2, NativeObject? obj3,
        NativeObject? obj4, NativeObject? obj5, NativeObject? obj6,
        NativeObject? obj7, NativeObject? obj8)
    {
        GC.KeepAlive(obj1);
        GC.KeepAlive(obj2);
        GC.KeepAlive(obj3);
        GC.KeepAlive(obj4);
        AfterUnmanagedCall(obj5, obj6, obj7, obj8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AfterUnmanagedCall(NativeObject? obj1, NativeObject? obj2, NativeObject? obj3,
        NativeObject? obj4, NativeObject? obj5, NativeObject? obj6,
        NativeObject? obj7, NativeObject? obj8, NativeObject? obj9)
    {
        GC.KeepAlive(obj1);
        GC.KeepAlive(obj2);
        GC.KeepAlive(obj3);
        GC.KeepAlive(obj4);
        AfterUnmanagedCall(obj5, obj6, obj7, obj8, obj9);
    }
}
