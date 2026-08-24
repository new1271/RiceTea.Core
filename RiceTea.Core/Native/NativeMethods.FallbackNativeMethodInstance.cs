using System;
using System.Runtime.InteropServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    private sealed unsafe class FallbackNativeMethodInstance : INativeMethodInstance
    {
        public uint GetCurrentThreadId() => (uint)Environment.CurrentManagedThreadId;

        public uint GetCurrentProcessorId() =>
#if NET8_0_OR_GREATER
            (uint)Thread.GetCurrentProcessorId();
#else
            (uint)Thread.CurrentThread.ManagedThreadId;
#endif


        public ulong GetTicksForSystem()
#if NET8_0_OR_GREATER
            => (ulong)Environment.TickCount64;
#else
            => (ulong)(Environment.TickCount & uint.MaxValue);
#endif

        public bool SleepInRelativeTicks(ulong ticks)
        {
            if (ticks <= 0)
                return false;
            SleepCore(ticks);
            return true;
        }

        public bool SleepInAbsoluteTicks(ulong ticks)
        {
            ulong currentTicks = GetTicksForSystem();
            if (ticks <= currentTicks)
                return false;
            SleepCore(ticks - currentTicks);
            return true;
        }

        public void* GetImportedMethodPointer(string? dllName, int methodIndex) => null;

        public void* GetImportedMethodPointer(string? dllName, string methodName) => null;

        public void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices) => new void*[methodIndices.Length];

        public void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<string> methodNames) => new void*[methodNames.Length];

        public IntPtr CreateWaitingHandle(bool initialState, bool autoReset)
            => (IntPtr)GCHandle.Alloc(autoReset ? new AutoResetEvent(initialState) : new ManualResetEvent(initialState), GCHandleType.Normal);

        public void ResetWaitingHandle(IntPtr handle)
        {
            if (GCHandle.FromIntPtr(handle).Target is not EventWaitHandle waitHandle)
                return;
            waitHandle.Reset();
        }

        public void SetWaitingHandle(IntPtr handle)
        {
            if (GCHandle.FromIntPtr(handle).Target is not EventWaitHandle waitHandle)
                return;
            waitHandle.Set();
        }

        public void DestroyWaitingHandle(IntPtr handle)
        {
            GCHandle gcHandle = GCHandle.FromIntPtr(handle);
            if (gcHandle.Target is not EventWaitHandle waitHandle)
                return;
            gcHandle.Free();
            waitHandle.Dispose();
        }

        public bool WaitForWaitingHandle(IntPtr handle, uint timeout)
        {
            if (GCHandle.FromIntPtr(handle).Target is not EventWaitHandle waitHandle)
                return false;
            return waitHandle.WaitOne((int)timeout);
        }

#if !NET8_0_OR_GREATER
        public void* AllocMemory(nuint size) => Marshal.AllocHGlobal(unchecked((nint)size)).ToPointer();

        public void FreeMemory(void* ptr) => Marshal.FreeHGlobal(new IntPtr(ptr));
#endif

        public void CopyMemory(void* destination, void* source, nuint sizeInBytes)
            => UnsafeHelper.CopyBlockUnaligned(destination, source, sizeInBytes);

        public void MoveMemory(void* destination, void* source, nuint sizeInBytes)
            => Buffer.MemoryCopy(source, destination, sizeInBytes, sizeInBytes);

        public void* AllocMemoryPage(nuint size, ProtectMemoryPageFlags flags) => AllocMemory(size);

        public void ProtectMemoryPage(void* ptr, nuint size, ProtectMemoryPageFlags flags)
        {
            // Do nothing
        }

        public void FlushInstructionCache(void* ptr, nuint size)
        {
            // Do nothing
        }

        [Inline(InlineBehavior.Remove)]
        private static void SleepCore(ulong ticks) => Thread.Sleep((int)MathHelper.MakeSigned(ticks / TimeSpan.TicksPerMillisecond));
    }
}
