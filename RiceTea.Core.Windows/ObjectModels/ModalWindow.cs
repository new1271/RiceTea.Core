using System;
using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace RiceTea.Core.Windows.ObjectModels;

[SuppressUnmanagedCodeSecurity]
public unsafe class ModalWindow : ComObject
{
    protected new enum MethodTable
    {
        _Start = ComObject.MethodTable._End,
        Show = _Start,
        _End
    }

    public ModalWindow() : base() { }

    public ModalWindow(void* nativePointer, ReferenceType pointerType) : base(nativePointer, pointerType) { }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Show() => Show(IntPtr.Zero);

    public bool Show(nint hwndOwner)
    {
        const int E_CANCEL = unchecked((int)0x800704C7U);

        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Show);
        int hr = ((delegate* unmanaged[Stdcall]<void*, nint, int>)functionPointer)(nativePointer, hwndOwner);
        AfterUnmanagedCall();
        if (hr >= 0)
            return true;
        if (hr != E_CANCEL)
            ThrowHelper.ThrowExceptionForHR(hr);
        return false;
    }
}
