using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    [SuppressUnmanagedCodeSecurity]
    unsafe partial class UnixInstance : INativeMethodInstance
    {
        private static readonly void* _gettidFunc, _cacheflushFunc;
        private static readonly nint _syscallID_gettid, _syscallID_futex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int gettid() => ((delegate* unmanaged[Cdecl]<int>)_gettidFunc)();

        [SuppressGCTransition]
        [DllImport(CLibraryName, EntryPoint = nameof(syscall))]
        private static extern nint syscall_fast(nint number);

        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern nint syscall(nint number);

        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern nint syscall(nint number, uint* uaddr, FutexMode mode, uint val, TimeSpecification* timeout);

        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern nint syscall(nint number, uint* uaddr, FutexMode mode, uint val);

        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void* memmove(void* dest, void* src, nuint sizeInBytes);

        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void* memcpy(void* dest, void* src, nuint sizeInBytes);

        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void* mmap(void* ptr, nuint length, ProtectMemoryPageFlags prot, MemoryMapFlags flags, int fd, nint offset);

        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int mprotect(void* ptr, nuint length, ProtectMemoryPageFlags flags);

        public static int cacheflush(void* addr, int nbytes, int cache)
        {
            void* func = _cacheflushFunc;
            if (func is null)
                return 0;
            return ((delegate* unmanaged[Cdecl]<void*, int, int, int>)func)(addr, nbytes, cache);
        }

        [SuppressGCTransition]
        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sched_getcpu();

        [SuppressGCTransition]
        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int clock_gettime(int clk_id, TimeSpecification* t);

        [SuppressGCTransition]
        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int clock_nanosleep(int clk_id, int flags, TimeSpecification* t, TimeSpecification* remain);

        public static uint GetCurrentThreadId() => (uint)gettid();

        public static uint GetCurrentProcessorId() => (uint)sched_getcpu();

        public static partial ulong GetTicksForSystem();

        public static partial void* GetImportedMethodPointer(string? dllName, int methodIndex);

        public static partial void* GetImportedMethodPointer(string? dllName, string methodName);

        public static partial void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices);

        public static partial void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<string> methodNames);

        public static IntPtr CreateWaitingHandle(bool initialState, bool autoReset)
        {
            RawWaitingEvent* ptr = (RawWaitingEvent*)AllocMemory(UnsafeHelper.SizeOf<RawWaitingEvent>());
            *ptr = new RawWaitingEvent(initialState, autoReset);
            return RawWaitingEvent.GetWaitingHandleFromEvent(ptr);
        }

        public static void ResetWaitingHandle(IntPtr handle) => RawWaitingEvent.GetEventFromWaitingHandle(handle)->Reset();

        public static void SetWaitingHandle(IntPtr handle)
        {
            if (!RawWaitingEvent.GetEventFromWaitingHandle(handle)->Set())
                return;
            syscall(_syscallID_futex, (uint*)(SysBool32*)handle, FutexMode.WakeInPrivate, int.MaxValue);
        }

        public static void DestroyWaitingHandle(IntPtr handle)
        {
            RawWaitingEvent* ptr = RawWaitingEvent.GetEventFromWaitingHandle(handle);
            FreeMemory(ptr);
        }

        public static bool WaitForWaitingHandle(IntPtr handle, uint timeout)
        {
            const uint INFINITE = unchecked((uint)Timeout.Infinite);
            const int EINTR = 4;
            const int EAGAIN = 11;
            const int ETIMEDOUT = 110;

            int lastError;

            TimeSpecification ts;
            TimeSpecification* pTimeout;
            if (timeout == INFINITE)
                pTimeout = null;
            else
            {
                ts = new TimeSpecification()
                {
                    tv_sec = timeout / 1000,
                    tv_nsec = (nuint)(timeout % 1000) * 1000_000,
                };
                pTimeout = &ts;
            }

            RawWaitingEvent* ptr = RawWaitingEvent.GetEventFromWaitingHandle(handle);
            if (ptr->IsAutoReset)
            {
                do
                {
                    if (ptr->Reset())
                        return true;
                    if (WaitCore(handle, pTimeout))
                        goto Success;

                    switch (lastError = Marshal.GetLastWin32Error())
                    {
                        case EINTR:
                            goto Continue;
                        case EAGAIN:
                            goto Success;
                        default:
                            goto Fault;
                    }

                Success:
                    if (ptr->Reset())
                        return true;
                    else
                        goto Continue;

                Continue:
                    Thread.Yield();
                    continue;
                } while (true);
            }
            else
            {
                do
                {
                    if (ptr->State)
                        return true;
                    if (WaitCore(handle, pTimeout))
                        return true;

                    switch (lastError = Marshal.GetLastWin32Error())
                    {
                        case EINTR:
                            goto Continue;
                        case EAGAIN:
                            return true;
                        default:
                            goto Fault;
                    }

                Continue:
                    Thread.Yield();
                    continue;
                } while (true);
            }

        Fault:
            return (timeout < INFINITE && lastError == ETIMEDOUT) ? false : throw new Win32Exception(lastError);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool WaitCore(IntPtr handle, TimeSpecification* timeout)
            => syscall(_syscallID_futex, (uint*)handle, FutexMode.WaitInPrivate, Booleans.FalseInt, timeout) == 0;

        public static bool SleepInRelativeTicks(ulong ticks)
        {
            const int CLOCK_MONOTONIC = 1;
            const int EINTR = 4;

            if (ticks <= 0)
                return false;

            nuint secs = (nuint)(ticks / TimeSpan.TicksPerSecond);
            nuint nanos = (nuint)((ticks % TimeSpan.TicksPerSecond) * 100);
            TimeSpecification ts = new TimeSpecification() { tv_sec = secs, tv_nsec = nanos };
            while (clock_nanosleep(CLOCK_MONOTONIC, flags: 0, &ts, &ts) == EINTR) ;
            return true;
        }

        public static bool SleepInAbsoluteTicks(ulong ticks)
        {
            const int CLOCK_MONOTONIC = 1;
            const int TIMER_ABSTIME = 1;
            const int EINTR = 4;

            TimeSpecification ts;
            if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0)
                return false;
            nuint secs = (nuint)(ticks / TimeSpan.TicksPerSecond);
            nuint nanos = (nuint)((ticks % TimeSpan.TicksPerSecond) * 100);
            if (ts.tv_sec > secs || (ts.tv_sec == secs && ts.tv_nsec >= nanos))
                return false;
            ts.tv_sec = secs;
            ts.tv_nsec = nanos;
            while (clock_nanosleep(CLOCK_MONOTONIC, flags: TIMER_ABSTIME, &ts, null) == EINTR) ;
            return true;
        }

        public static void CopyMemory(void* destination, void* source, nuint sizeInBytes) => memcpy(destination, source, sizeInBytes);

        public static void MoveMemory(void* destination, void* source, nuint sizeInBytes) => memmove(destination, source, sizeInBytes);

        public static void* AllocMemoryPage(nuint size, ProtectMemoryPageFlags flags)
            => mmap(null, size, flags, MemoryMapFlags.Private | MemoryMapFlags.Anomymous, -1, 0);

        public static void ProtectMemoryPage(void* ptr, nuint size, ProtectMemoryPageFlags flags)
            => mprotect(ptr, size, flags);

        public static void FlushInstructionCache(void* ptr, nuint size)
        {
            const int ICACHE = 1 << 0;
            const int DCACHE = 1 << 1;
            const int BCACHE = ICACHE | DCACHE;
            for (; size > int.MaxValue; size -= int.MaxValue, ptr = (byte*)ptr + int.MaxValue)
            {
                if (cacheflush(ptr, int.MaxValue, BCACHE) != 0)
                    return;
            }
            if (size > 0)
                cacheflush(ptr, (int)size, BCACHE);
        }

        private static partial int gettid_fallback() => (int)syscall_fast(_syscallID_gettid);

        private enum MemoryMapFlags : uint
        {
            Failed = unchecked((uint)-1),

            Shared = 0x01,
            Private = 0x02,
            SharedValidate = 0x03,
            Fixed = 0x10,
            Anomymous = 0x20,
            NoReserve = 0x4000,
            GrowsDown = 0x0100,
            DenyWrite = 0x0800,
            Executable = 0x1000,
            Locked = 0x2000,
            Populate = 0x8000,
            NonBlock = 0x10000,
            Stack = 0x20000,
            HugeTlb = 0x40000,
            Sync = 0x80000,
            FixedNoReplace = 0x100000,
            File = 0
        }

        private enum FutexMode : uint
        {
            Wait = 0,
            Wake = 1,
            FileDescriptor = 2,
            Requeue = 3,
            CompareAndRequeue = 4,
            WakeWithOperation = 5,
            LockWithPI = 6,
            UnlockWithPI = 7,
            TryLockWithPI = 8,
            WaitForBitset = 9,
            WakeByBitset = 10,
            WaitAndRequeueWithPI = 11,
            CompareAndRequeueWithPI = 12,
            LockWithPI_2 = 13,

            WithPrivateFlag = 128,
            WithClockRealTime = 256,
            CommandMask = ~(WithPrivateFlag | WithClockRealTime),

            WaitInPrivate = (Wait | WithPrivateFlag),
            WakeInPrivate = (Wake | WithPrivateFlag),
            RequeueInPrivate = (Requeue | WithPrivateFlag),
            CompareAndRequeueInPrivate = (CompareAndRequeue | WithPrivateFlag),
            WakeWithOperationInPrivate = (WakeWithOperation | WithPrivateFlag),
            LockWithPI_Private = (LockWithPI | WithPrivateFlag),
            LockWithPI2_Private = (LockWithPI_2 | WithPrivateFlag),
            UnlockWithPI_Private = (UnlockWithPI | WithPrivateFlag),
            TryLockWithPI_Private = (TryLockWithPI | WithPrivateFlag),
            WaitForBitsetInPrivate = (WaitForBitset | WithPrivateFlag),
            WakeByBitsetInPrivate = (WakeByBitset | WithPrivateFlag),
            WaitAndRequeueWithPI_Private = (WaitAndRequeueWithPI | WithPrivateFlag),
            CompareAndRequeueWithPI_Private = (CompareAndRequeueWithPI | WithPrivateFlag)
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeSpecification
        {
            public nuint tv_sec;
            public nuint tv_nsec;
        }
    }
}
