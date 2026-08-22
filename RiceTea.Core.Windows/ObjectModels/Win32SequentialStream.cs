using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace RiceTea.Core.Windows.ObjectModels;

public unsafe class Win32SequentialStream : ComObject, IWin32SequentialStream
{
    public static readonly Guid IID_ISequentialStream = new Guid(0x0c733a30, 0x2a1c, 0x11ce, 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d);

    public Win32SequentialStream() { }

    public Win32SequentialStream(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    protected new enum MethodTable
    {
        _Start = ComObject.MethodTable._End,
        Read = _Start,
        Write,
        _End
    }

    [SkipLocalsInit]
    public ulong Read(byte* ptr, ulong length)
    {
        ulong result;
        ThrowHelper.ThrowExceptionForHR(ReadCore(ptr, length, &result));
        return result;
    }

    [SkipLocalsInit]
    public ulong Write(byte* ptr, ulong length)
    {
        ulong result;
        ThrowHelper.ThrowExceptionForHR(WriteCore(ptr, length, &result));
        return result;
    }

    public bool TryRead(byte* ptr, ulong length, out ulong byteRead) 
        => ReadCore(ptr, length, UnsafeHelper.AsPointerOut(out byteRead)) >= 0;

    public bool TryWrite(byte* ptr, ulong length, out ulong byteWritten)
        => WriteCore(ptr, length, UnsafeHelper.AsPointerOut(out byteWritten)) >= 0;

    [Inline(InlineBehavior.Remove)]
    private int ReadCore(byte* ptr, ulong length, ulong* pByteRead)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Read);
        return ((delegate* unmanaged[Stdcall]<void*, byte*, ulong, ulong*, int>)functionPointer)(nativePointer,
            ptr, length, pByteRead);
    }

    [Inline(InlineBehavior.Remove)]
    private int WriteCore(byte* ptr, ulong length, ulong* pByteWritten)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Write);
        return ((delegate* unmanaged[Stdcall]<void*, byte*, ulong, ulong*, int>)functionPointer)(nativePointer,
            ptr, length, pByteWritten);
    }
}
