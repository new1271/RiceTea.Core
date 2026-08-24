using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    public static unsafe char* AllocCStyleUtf16String(string value)
    {
        int length = value.Length;
        uint byteCount = unchecked((uint)length) * sizeof(char);
        char* result = (char*)AllocMemory(byteCount + sizeof(char));
        result[length] = '\0';
        fixed (char* ptr = value)
            UnsafeHelper.CopyBlock(result, ptr, byteCount);
        return result;
    }

    public static unsafe T* AllocUnmanagedStructure<T>(T value) where T : unmanaged
    {
        T* result = (T*)AllocMemory(unchecked((nuint)sizeof(T)));
        *result = value;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void* AllocMemory(nint size) => AllocMemory(MathHelper.MakeUnsigned(size));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void* AllocMemory(nuint size)
#if NET8_0_OR_GREATER
        => System.Runtime.InteropServices.NativeMemory.Alloc(size);
#else
        => _methodInstance switch
        {
            Win32NativeMethodInstance inst => inst.AllocMemory(size),
            UnixNativeMethodInstance inst => inst.AllocMemory(size),
            FallbackNativeMethodInstance inst => inst.AllocMemory(size),
            _ => _methodInstance.AllocMemory(size)
        };
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void FreeMemory(void* ptr)
    {
#if NET8_0_OR_GREATER
        System.Runtime.InteropServices.NativeMemory.Free(ptr);
#else
        switch (_methodInstance)
        {
            case Win32NativeMethodInstance inst:
                inst.FreeMemory(ptr);
                break;
            case UnixNativeMethodInstance inst:
                inst.FreeMemory(ptr);
                break;
            case FallbackNativeMethodInstance inst:
                inst.FreeMemory(ptr);
                break;
            default:
                _methodInstance.FreeMemory(ptr);
                break;
        }
#endif
    }
}
