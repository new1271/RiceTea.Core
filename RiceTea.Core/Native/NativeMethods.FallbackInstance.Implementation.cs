using System;
using System.Runtime.InteropServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    unsafe partial class FallbackInstance
    {
        public static uint GetCurrentThreadId() => (uint)Environment.CurrentManagedThreadId;

        public static partial uint GetCurrentProcessorId();

        public static partial ulong GetTicksForSystem();

        public static bool SleepInRelativeTicks(ulong ticks)
        {
            if (ticks <= 0)
                return false;
            SleepCore(ticks);
            return true;
        }

        public static bool SleepInAbsoluteTicks(ulong ticks)
        {
            ulong currentTicks = GetTicksForSystem();
            if (ticks <= currentTicks)
                return false;
            SleepCore(ticks - currentTicks);
            return true;
        }

        public static void* GetImportedMethodPointer(string? dllName, int methodIndex) => null;

        public static void* GetImportedMethodPointer(string? dllName, string methodName) => null;

        public static void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices) => new void*[methodIndices.Length];

        public static void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<string> methodNames) => new void*[methodNames.Length];

        public static IntPtr CreateWaitingHandle(bool initialState, bool autoReset)
            => (IntPtr)GCHandle.Alloc(autoReset ? new AutoResetEvent(initialState) : new ManualResetEvent(initialState), GCHandleType.Normal);

        public static void ResetWaitingHandle(IntPtr handle)
        {
            if (GCHandle.FromIntPtr(handle).Target is not EventWaitHandle waitHandle)
                return;
            waitHandle.Reset();
        }

        public static void SetWaitingHandle(IntPtr handle)
        {
            if (GCHandle.FromIntPtr(handle).Target is not EventWaitHandle waitHandle)
                return;
            waitHandle.Set();
        }

        public static void DestroyWaitingHandle(IntPtr handle)
        {
            GCHandle gcHandle = GCHandle.FromIntPtr(handle);
            if (gcHandle.Target is not EventWaitHandle waitHandle)
                return;
            gcHandle.Free();
            waitHandle.Dispose();
        }

        public static bool WaitForWaitingHandle(IntPtr handle, uint timeout)
        {
            if (GCHandle.FromIntPtr(handle).Target is not EventWaitHandle waitHandle)
                return false;
            return waitHandle.WaitOne((int)timeout);
        }

        public static void CopyMemory(void* destination, void* source, nuint sizeInBytes)
            => UnsafeHelper.CopyBlockUnaligned(destination, source, sizeInBytes);

        public static void MoveMemory(void* destination, void* source, nuint sizeInBytes)
            => Buffer.MemoryCopy(source, destination, sizeInBytes, sizeInBytes);

        public static void* AllocMemoryPage(nuint size, ProtectMemoryPageFlags flags) => AllocMemory(size);

        public static void ProtectMemoryPage(void* ptr, nuint size, ProtectMemoryPageFlags flags)
        {
            // Do nothing
        }

        public static void FlushInstructionCache(void* ptr, nuint size)
        {
            // Do nothing
        }

        [Inline(InlineBehavior.Remove)]
        private static void SleepCore(ulong ticks) => Thread.Sleep((int)MathHelper.MakeSigned(ticks / TimeSpan.TicksPerMillisecond));
    }
}
