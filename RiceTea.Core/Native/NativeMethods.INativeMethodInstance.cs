using System;

using RiceTea.Core.Structures;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    private unsafe interface INativeMethodInstance
    {
        uint GetCurrentThreadId();
        uint GetCurrentProcessorId();
        ulong GetTicksForSystem();
        void* GetImportedMethodPointer(string? dllName, int methodIndex);
        void* GetImportedMethodPointer(string? dllName, string methodName);
        void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices);
        void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<string> methodNames);
        bool SleepInRelativeTicks(ulong ticks);
        bool SleepInAbsoluteTicks(ulong ticks);
        IntPtr CreateWaitingHandle(bool initialState, bool autoReset);
        void ResetWaitingHandle(IntPtr handle);
        void SetWaitingHandle(IntPtr handle);
        void DestroyWaitingHandle(IntPtr handle);
        bool WaitForWaitingHandle(IntPtr handle, uint timeout);
#if !NET8_0_OR_GREATER
        void* AllocMemory(nuint size);

        void FreeMemory(void* ptr);
#endif
        void CopyMemory(void* destination, void* source, nuint sizeInBytes);
        void MoveMemory(void* destination, void* source, nuint sizeInBytes);
        void* AllocMemoryPage(nuint size, ProtectMemoryPageFlags flags);
        void ProtectMemoryPage(void* ptr, nuint size, ProtectMemoryPageFlags flags);
        void FlushInstructionCache(void* ptr, nuint size);
    }

    [Flags]
    public enum ProtectMemoryPageFlags : int
    {
        None = 0x0,
        CanRead = 0x1,
        CanWrite = 0x2,
        CanExecute = 0x4,
    }
}
