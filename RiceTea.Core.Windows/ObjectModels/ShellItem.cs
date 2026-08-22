using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Windows.Internals;

namespace RiceTea.Core.Windows.ObjectModels;

[SuppressUnmanagedCodeSecurity]
public unsafe class ShellItem : ComObject
{
    private static readonly Guid IID_ShellItem = new Guid(0x43826d1e, 0xe718, 0x42ee, 0xbc, 0x55, 0xa1, 0xe2, 0x61, 0xc3, 0x7b, 0xfe);

    private new enum MethodTable
    {
        _Start = ComObject.MethodTable._End,
        BindToHandler = _Start,
        GetParent,
        GetDisplayName,
        GetAttributes,
        Compare,
        _End
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ShellItem Create(Environment.SpecialFolder folder) => Create(Environment.GetFolderPath(folder));

    public static ShellItem Create(string path)
    {
        ThrowHelper.ThrowExceptionForHR(TryCreateCore(path, out void* nativePointer), nativePointer);
        return new ShellItem(nativePointer, ReferenceType.Owned);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCreate(Environment.SpecialFolder folder, [NotNullWhen(true)] out ShellItem? result) 
        => TryCreate(Environment.GetFolderPath(folder), out result);

    public static bool TryCreate(string path, [NotNullWhen(true)] out ShellItem? result)
    {
        int hr = TryCreateCore(path, out void* nativePointer);
        if (hr < 0 || nativePointer == null)
        {
            result = null;
            return false;
        }
        result = new ShellItem(nativePointer, ReferenceType.Owned);
        return true;
    }

    [SkipLocalsInit]
    private static int TryCreateCore(string path, out void* result)
        => Shell32.SHCreateItemFromParsingName(path, null, IID_ShellItem, UnsafeHelper.AsPointerOut(out result));

    public ShellItem() : base() { }

    public ShellItem(void* nativePointer, ReferenceType pointerType) : base(nativePointer, pointerType) { }

    public string GetDisplayName(ShellItemGetDisplayName sigdnName)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDisplayName);
        int hr = ((delegate* unmanaged[Stdcall]<void*, ShellItemGetDisplayName, char**, int>)functionPointer)(nativePointer, sigdnName, (char**)&nativePointer);
        ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
        try
        {
            return new string((char*)nativePointer);
        }
        finally
        {
            Ole32.CoTaskMemFree(nativePointer);
        }
    }
}
