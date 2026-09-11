using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Windows.ObjectModels;

unsafe partial class ComObject
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* GetFunctionPointerCore(void* nativePointer, int offset)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(uint) => *(void**)(*(uint**)nativePointer + offset),
            sizeof(ulong) => *(void**)(*(ulong**)nativePointer + offset),
            UnsafeHelper.PointerSizeConstant_Indeterminate => UnsafeHelper.PointerSize switch
            {
                sizeof(uint) => *(void**)(*(uint**)nativePointer + offset),
                sizeof(ulong) => *(void**)(*(ulong**)nativePointer + offset),
                _ => throw new NotSupportedException($"Pointer size {UnsafeHelper.PointerSize} is not supported.")
            },
            _ => throw new NotSupportedException($"Pointer size {UnsafeHelper.PointerSizeConstant} is not supported.")
        };

    [Inline(InlineBehavior.Remove)]
    internal static int QueryInterfaceCore(ref void* nativePointer, in Guid iid)
    {
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.QueryInterface);
        fixed (Guid* riid = &iid)
        fixed (void** pResult = &nativePointer)
            return ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, pResult);
    }

    [Inline(InlineBehavior.Remove)]
    internal static uint AddRefCore(void* nativePointer)
    {
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddRef);
        return ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, uint>)functionPointer)(nativePointer);
    }

    [Inline(InlineBehavior.Remove)]
    internal static uint ReleaseCore(void* nativePointer)
    {
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Release);
        return ((delegate* unmanaged[Stdcall]<void*, uint>)functionPointer)(nativePointer);
    }
}
