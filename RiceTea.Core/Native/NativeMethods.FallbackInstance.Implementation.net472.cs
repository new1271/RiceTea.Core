#if NET472_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    unsafe partial class FallbackInstance
    {
        void* INativeMethodInstance.AllocMemory(nuint size) => AllocMemory(size);

        void INativeMethodInstance.FreeMemory(void* ptr) => FreeMemory(ptr);

        public static partial uint GetCurrentProcessorId() => (uint)Thread.CurrentThread.ManagedThreadId;

        public static partial ulong GetTicksForSystem() => (ulong)(Environment.TickCount & uint.MaxValue);

        public static void* AllocMemory(nuint size) => Marshal.AllocHGlobal(unchecked((nint)size)).ToPointer();

        public static void FreeMemory(void* ptr) => Marshal.FreeHGlobal(new IntPtr(ptr));
    }
}
#endif