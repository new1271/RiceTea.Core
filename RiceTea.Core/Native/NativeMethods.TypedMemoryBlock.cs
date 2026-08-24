using System.Runtime.CompilerServices;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe TypedNativeMemoryBlock<T> AllocMemoryBlock<T>(nint size) where T : unmanaged
    {
        void* ptr = AllocMemory(size);
        if (ptr == default)
            return default;
        return new TypedNativeMemoryBlock<T>((T*)ptr, (nuint)size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe TypedNativeMemoryBlock<T> AllocMemoryBlock<T>(nuint size) where T : unmanaged
    {
        void* ptr = AllocMemory(size);
        if (ptr == default)
            return default;
        return new TypedNativeMemoryBlock<T>((T*)ptr, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void FreeMemoryBlock<T>(in TypedNativeMemoryBlock<T> block) where T : unmanaged
    {
        void* ptr = block.NativePointer;
        if (ptr == null)
            return;
        FreeMemory(ptr);
    }
}
