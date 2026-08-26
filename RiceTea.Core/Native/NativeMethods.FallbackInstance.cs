using System;

using RiceTea.Core.Structures;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    private sealed unsafe partial class FallbackInstance : INativeMethodInstance
    {
        uint INativeMethodInstance.GetCurrentThreadId()
            => GetCurrentThreadId();

        uint INativeMethodInstance.GetCurrentProcessorId()
            => GetCurrentProcessorId();

        ulong INativeMethodInstance.GetTicksForSystem()
            => GetTicksForSystem();

        void* INativeMethodInstance.GetImportedMethodPointer(string? dllName, int methodIndex)
            => GetImportedMethodPointer(dllName, methodIndex);

        void* INativeMethodInstance.GetImportedMethodPointer(string? dllName, string methodName)
            => GetImportedMethodPointer(dllName, methodName);

        void*[] INativeMethodInstance.GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices)
            => GetImportedMethodPointers(dllName, methodIndices);

        void*[] INativeMethodInstance.GetImportedMethodPointers(string? dllName, in ParamArrayTiny<string> methodNames)
            => GetImportedMethodPointers(dllName, methodNames);

        bool INativeMethodInstance.SleepInRelativeTicks(ulong ticks)
            => SleepInRelativeTicks(ticks);

        bool INativeMethodInstance.SleepInAbsoluteTicks(ulong ticks)
            => SleepInAbsoluteTicks(ticks);

        IntPtr INativeMethodInstance.CreateWaitingHandle(bool initialState, bool autoReset)
            => CreateWaitingHandle(initialState, autoReset);

        void INativeMethodInstance.ResetWaitingHandle(IntPtr handle)
            => ResetWaitingHandle(handle);

        void INativeMethodInstance.SetWaitingHandle(IntPtr handle)
            => SetWaitingHandle(handle);

        void INativeMethodInstance.DestroyWaitingHandle(IntPtr handle)
            => DestroyWaitingHandle(handle);

        bool INativeMethodInstance.WaitForWaitingHandle(IntPtr handle, uint timeout)
            => WaitForWaitingHandle(handle, timeout);

        void INativeMethodInstance.CopyMemory(void* destination, void* source, nuint sizeInBytes)
            => CopyMemory(destination, source, sizeInBytes);

        void INativeMethodInstance.MoveMemory(void* destination, void* source, nuint sizeInBytes)
            => MoveMemory(destination, source, sizeInBytes);

        void* INativeMethodInstance.AllocMemoryPage(nuint size, ProtectMemoryPageFlags flags)
            => AllocMemoryPage(size, flags);

        void INativeMethodInstance.ProtectMemoryPage(void* ptr, nuint size, ProtectMemoryPageFlags flags)
            => ProtectMemoryPage(ptr, size, flags);

        void INativeMethodInstance.FlushInstructionCache(void* ptr, nuint size)
            => FlushInstructionCache(ptr, size);
    }
}
