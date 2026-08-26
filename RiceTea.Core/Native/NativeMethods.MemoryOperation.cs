using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Native;

unsafe partial class NativeMethods
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyMemory(void* destination, void* source, nint sizeInBytes)
        => CopyMemory(destination, source, MathHelper.MakeUnsigned(sizeInBytes));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyMemory(void* destination, void* source, nuint sizeInBytes)
    {
        switch (_methodInstance)
        {
            case Win32Instance:
                Win32Instance.CopyMemory(destination, source, sizeInBytes);
                break;
            case UnixInstance:
                UnixInstance.CopyMemory(destination, source, sizeInBytes);
                break;
            case FallbackInstance:
                FallbackInstance.CopyMemory(destination, source, sizeInBytes);
                break;
            default:
                _methodInstance.CopyMemory(destination, source, sizeInBytes);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MoveMemory(void* destination, void* source, nint sizeInBytes)
        => MoveMemory(destination, source, MathHelper.MakeUnsigned(sizeInBytes));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MoveMemory(void* destination, void* source, nuint sizeInBytes)
    {
        switch (_methodInstance)
        {
            case Win32Instance:
                Win32Instance.MoveMemory(destination, source, sizeInBytes);
                break;
            case UnixInstance:
                UnixInstance.MoveMemory(destination, source, sizeInBytes);
                break;
            case FallbackInstance:
                FallbackInstance.MoveMemory(destination, source, sizeInBytes);
                break;
            default:
                _methodInstance.MoveMemory(destination, source, sizeInBytes);
                break;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocMemoryPage(nint size, ProtectMemoryPageFlags flags)
        => AllocMemoryPage(MathHelper.MakeUnsigned(size), flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocMemoryPage(nuint size, ProtectMemoryPageFlags flags)
        => _methodInstance switch
        {
            Win32Instance => Win32Instance.AllocMemoryPage(size, flags),
            UnixInstance => UnixInstance.AllocMemoryPage(size, flags),
            FallbackInstance => FallbackInstance.AllocMemoryPage(size, flags),
            _ => _methodInstance.AllocMemoryPage(size, flags)
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ProtectMemoryPage(void* ptr, nint size, ProtectMemoryPageFlags flags)
        => ProtectMemoryPage(ptr, MathHelper.MakeUnsigned(size), flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ProtectMemoryPage(void* ptr, nuint size, ProtectMemoryPageFlags flags)
    {
        switch (_methodInstance)
        {
            case Win32Instance:
                Win32Instance.ProtectMemoryPage(ptr, size, flags);
                break;
            case UnixInstance:
                UnixInstance.ProtectMemoryPage(ptr, size, flags);
                break;
            case FallbackInstance:
                FallbackInstance.ProtectMemoryPage(ptr, size, flags);
                break;
            default:
                _methodInstance.ProtectMemoryPage(ptr, size, flags);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FlushInstructionCache(void* ptr, nint size)
        => FlushInstructionCache(ptr, MathHelper.MakeUnsigned(size));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FlushInstructionCache(void* ptr, nuint size)
    {
        switch (_methodInstance)
        {
            case Win32Instance:
                Win32Instance.FlushInstructionCache(ptr, size);
                break;
            case UnixInstance:
                UnixInstance.FlushInstructionCache(ptr, size);
                break;
            case FallbackInstance:
                FallbackInstance.FlushInstructionCache(ptr, size);
                break;
            default:
                _methodInstance.FlushInstructionCache(ptr, size);
                break;
        }
    }
}
