using System.Runtime.CompilerServices;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe NativeMemoryBlock AllocMemoryBlock(nint size)
    {
        void* ptr = AllocMemory(size);
        if (ptr == default)
            return NativeMemoryBlock.Empty;
        return new NativeMemoryBlock(ptr, (nuint)size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe NativeMemoryBlock AllocMemoryBlock(nuint size)
    {
        void* ptr = AllocMemory(size);
        if (ptr == default)
            return NativeMemoryBlock.Empty;
        return new NativeMemoryBlock(ptr, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void FreeMemoryBlock(in NativeMemoryBlock block)
    {
        void* ptr = block.NativePointer;
        if (ptr == null)
            return;
        FreeMemory(ptr);
    }
}
