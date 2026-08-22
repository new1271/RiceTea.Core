using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

using InlineIL;

using InlineMethod;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;
using RiceTea.Core.Text;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    [SuppressUnmanagedCodeSecurity]
    private sealed unsafe class UnixNativeMethodInstance : INativeMethodInstance
    {
        private static readonly void* _gettidFunc, _cacheflushFunc;
        private static readonly nint _syscallID_gettid, _syscallID_futex;

        static UnixNativeMethodInstance()
        {
            (_syscallID_gettid, _syscallID_futex) = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => (224, 240),
                Architecture.X64 => (186, 202),
                Architecture.Arm => (224, 240),
                Architecture.Arm64 => (178, 98),
#if NET8_0_OR_GREATER
                Architecture.S390x => (236, 238),
                Architecture.LoongArch64 => (178, 98),
                Architecture.Armv6 => (224, 240),
                Architecture.Ppc64le => (179, 221),
                //Architecture.RiscV64 => (178, 98),
#endif
                _ => (0, 0)
            };
            if (_syscallID_futex == 0)
                Debug.WriteLine($"This platform doesn't support futex, so {nameof(SetWaitingHandle)}, {nameof(WaitForWaitingHandle)} cannot work correctly!");
            void* func = GetImportedMethodPointerCore_Internal(null, nameof(gettid));
            if (func is null)
            {
#if NET8_0_OR_GREATER
                func = (delegate* unmanaged[Cdecl]<int>)&gettid_fallback;
#else
                func = (delegate*<int>)&gettid_fallback;
#endif
            }
            _gettidFunc = func;
            _cacheflushFunc = GetImportedMethodPointerCore_Internal(null, nameof(cacheflush));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int gettid() => ((delegate* unmanaged[Cdecl]<int>)_gettidFunc)();

        [SuppressGCTransition]
        [DllImport("libc", EntryPoint = nameof(syscall))]
        private static extern nint syscall_fast(nint number);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern nint syscall(nint number);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern nint syscall(nint number, uint* uaddr, FutexMode mode, uint val, TimeSpecification* timeout);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern nint syscall(nint number, uint* uaddr, FutexMode mode, uint val);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* malloc(nuint size);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern void free(void* memblock);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* memmove(void* dest, void* src, nuint sizeInBytes);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* memcpy(void* dest, void* src, nuint sizeInBytes);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* mmap(void* ptr, nuint length, ProtectMemoryPageFlags prot, MemoryMapFlags flags, int fd, nint offset);

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern int mprotect(void* ptr, nuint length, ProtectMemoryPageFlags flags);

        public static int cacheflush(void* addr, int nbytes, int cache)
        {
            void* func = _cacheflushFunc;
            if (func is null)
                return 0;
            return ((delegate* unmanaged[Cdecl]<void*, int, int, int>)func)(addr, nbytes, cache);
        }

        [SuppressGCTransition]
        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern int sched_getcpu();

        [SuppressGCTransition]
        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern int clock_gettime(int clk_id, TimeSpecification* t);

        [SuppressGCTransition]
        [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
        private static extern int clock_nanosleep(int clk_id, int flags, TimeSpecification* t, TimeSpecification* remain);

        public uint GetCurrentThreadId() => (uint)gettid();

        public uint GetCurrentProcessorId() => (uint)sched_getcpu();

        public ulong GetTicksForSystem()
        {
            const int CLOCK_MONOTONIC = 1;

            TimeSpecification ts;
            if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0)
            {
#if NET8_0_OR_GREATER
                return (ulong)Environment.TickCount64;
#else
                return (ulong)(Environment.TickCount & uint.MaxValue);
#endif
            }
            return ts.tv_sec * TimeSpan.TicksPerSecond + ts.tv_nsec / 100;
        }

        public void* GetImportedMethodPointer(string? dllName, int methodIndex) => null;

        public void* GetImportedMethodPointer(string? dllName, string methodName) => GetImportedMethodPointerCore(dllName, methodName);

        public void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices) => new void*[methodIndices.Length];

        public void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<string> methodNames) => GetImportedMethodPointersCore(dllName, methodNames);

        public IntPtr CreateWaitingHandle(bool initialState, bool autoReset)
        {
            RawWaitingEvent* ptr = (RawWaitingEvent*)AllocMemory(UnsafeHelper.SizeOf<RawWaitingEvent>());
            *ptr = new RawWaitingEvent(initialState, autoReset);
            return RawWaitingEvent.GetWaitingHandleFromEvent(ptr);
        }

        public void ResetWaitingHandle(IntPtr handle) => RawWaitingEvent.GetEventFromWaitingHandle(handle)->Reset();

        public void SetWaitingHandle(IntPtr handle)
        {
            if (!RawWaitingEvent.GetEventFromWaitingHandle(handle)->Set())
                return;
            syscall(_syscallID_futex, (uint*)(SysBool32*)handle, FutexMode.WakeInPrivate, int.MaxValue);
        }

        public void DestroyWaitingHandle(IntPtr handle)
        {
            RawWaitingEvent* ptr = RawWaitingEvent.GetEventFromWaitingHandle(handle);
            FreeMemory(ptr);
        }

        public bool WaitForWaitingHandle(IntPtr handle, uint timeout)
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

        public bool SleepInRelativeTicks(ulong ticks)
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

        public bool SleepInAbsoluteTicks(ulong ticks)
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

        public void* AllocMemory(nuint size) => malloc(size);

        public void FreeMemory(void* ptr) => free(ptr);

        public void CopyMemory(void* destination, void* source, nuint sizeInBytes) => memcpy(destination, source, sizeInBytes);

        public void MoveMemory(void* destination, void* source, nuint sizeInBytes) => memmove(destination, source, sizeInBytes);

        public void* AllocMemoryPage(nuint size, ProtectMemoryPageFlags flags)
            => mmap(null, size, flags, MemoryMapFlags.Private | MemoryMapFlags.Anomymous, -1, 0);

        public void ProtectMemoryPage(void* ptr, nuint size, ProtectMemoryPageFlags flags)
            => mprotect(ptr, size, flags);

        public void FlushInstructionCache(void* ptr, nuint size)
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

        public static void* GetImportedMethodPointerCore(string? dllName, string methodName)
        {
            const int RTLD_NOW = 2;
            const int RTLD_LOCAL = 0;

            IntPtr module = dlopen(dllName, RTLD_NOW | RTLD_LOCAL);

            ArrayPool<byte> pool = ArrayPool<byte>.Shared;

            return GetImportedMethodPointerCore(pool, module, methodName);
        }

#if NET8_0_OR_GREATER
        [Inline(InlineBehavior.Remove)]
        private static void* GetImportedMethodPointerCore_Internal(string? dllName, string methodName)
            => GetImportedMethodPointerCore(dllName, methodName);
#else
        private static void* GetImportedMethodPointerCore_Internal(string? dllName, string methodName)
        {
            const int RTLD_NOW = 2;
            const int RTLD_LOCAL = 0;

            IntPtr module = dlopen(dllName, RTLD_NOW | RTLD_LOCAL);

            ArrayPool<byte> pool = ArrayPool<byte>.Shared;
            if (pool is ArrayPool<byte>.SystemBufferImpl)
                return GetImportedMethodPointerCore(pool, module, methodName);

            return GetImportedMethodPointerCore(module, methodName);
        }
#endif

        public static void*[] GetImportedMethodPointersCore(string? dllName, ParamArrayTiny<string> methodNames)
        {
            const int RTLD_NOW = 2;
            const int RTLD_LOCAL = 0;

            IntPtr module = dlopen(dllName, RTLD_NOW | RTLD_LOCAL);

            ArrayPool<byte> pool = ArrayPool<byte>.Shared;

            int length = methodNames.Length;
            void*[] pointers = new void*[length];

            for (int i = 0; i < length; i++)
            {
                string methodName = methodNames[i];
                pointers[i] = GetImportedMethodPointerCore(pool, module, methodName);
            }

            return pointers;
        }

        public static void* GetImportedMethodPointerCore(ArrayPool<byte> pool, IntPtr module, string methodName)
        {
            int length = methodName.Length;
            byte[] buffer = pool.Rent(length + 1);
            try
            {
                fixed (char* source = methodName)
                fixed (byte* destination = buffer)
                {
                    AsciiEncodingHelper.ReadFromUtf16Buffer(source, source + length, destination, destination + length);
                    destination[length] = 0;
                    return LibDl.Instance.dlsym(module, destination);
                }
            }
            finally
            {
                pool.Return(buffer);
            }
        }

        public static void* GetImportedMethodPointerCore(IntPtr module, string methodName)
        {
            int length = methodName.Length;
            byte[] buffer = new byte[length + 1];
            fixed (char* source = methodName)
            fixed (byte* destination = buffer)
            {
                AsciiEncodingHelper.ReadFromUtf16Buffer(source, source + length, destination, destination + length);
                destination[length] = 0;
                return LibDl.Instance.dlsym(module, destination);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr dlopen(string? filename, int flags)
        {
            if (filename is null)
                return LibDl.Instance.dlopen((byte*)null, flags);

            int length = filename.Length;

            ArrayPool<byte> pool = ArrayPool<byte>.Shared;
            byte[] buffer = pool.Rent(Utf8EncodingHelper.GetWorstCaseForEncodeLength(length) + 1);
            try
            {
                fixed (char* source = filename)
                fixed (byte* destination = buffer)
                {
                    Utf8EncodingHelper.ReadFromUtf16Buffer(source, source + length, destination, destination + buffer.Length);
                    return LibDl.Instance.dlopen(destination, flags);
                }
            }
            finally
            {
                pool.Return(buffer);
            }
        }

#if NET8_0_OR_GREATER
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
        private static int gettid_fallback() => (int)syscall_fast(_syscallID_gettid);

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

        private static class LibDl
        {
            public static readonly ILibDl Instance = FindInstance();

            private static ILibDl FindInstance()
            {
                try
                {
                    return CreateModernInstance();
                }
                catch (Exception)
                {
                    return CreateLegacyInstance();
                }

                [MethodImpl(MethodImplOptions.NoInlining)]
                static ILibDl CreateModernInstance() => new Modern();

                [MethodImpl(MethodImplOptions.NoInlining)]
                static ILibDl CreateLegacyInstance() => new Legacy();
            }

            private sealed class Modern : ILibDl
            {
                public Modern()
                {
                    IL.Emit.Ldtoken(new MethodRef(typeof(Modern), nameof(dlopen)));
                    IL.Emit.Call(new MethodRef(typeof(RuntimeHelpers), nameof(RuntimeHelpers.PrepareMethod), typeof(RuntimeMethodHandle)));
                    IL.Emit.Ldtoken(new MethodRef(typeof(Modern), nameof(dlsym)));
                    IL.Emit.Call(new MethodRef(typeof(RuntimeHelpers), nameof(RuntimeHelpers.PrepareMethod), typeof(RuntimeMethodHandle)));
                }

                [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
                private static extern IntPtr dlopen(byte* filename, int flags);

                [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
                private static extern void* dlsym(IntPtr handle, byte* symbol);

                IntPtr ILibDl.dlopen(byte* filename, int flags) => dlopen(filename, flags);

                void* ILibDl.dlsym(nint handle, byte* symbol) => dlsym(handle, symbol);
            }

            private sealed class Legacy : ILibDl
            {
                public Legacy()
                {
                    IL.Emit.Ldtoken(new MethodRef(typeof(Legacy), nameof(dlopen)));
                    IL.Emit.Call(new MethodRef(typeof(RuntimeHelpers), nameof(RuntimeHelpers.PrepareMethod), typeof(RuntimeMethodHandle)));
                    IL.Emit.Ldtoken(new MethodRef(typeof(Legacy), nameof(dlsym)));
                    IL.Emit.Call(new MethodRef(typeof(RuntimeHelpers), nameof(RuntimeHelpers.PrepareMethod), typeof(RuntimeMethodHandle)));
                }

                [DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
                private static extern IntPtr dlopen(byte* filename, int flags);

                [DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
                private static extern void* dlsym(IntPtr handle, byte* symbol);

                IntPtr ILibDl.dlopen(byte* filename, int flags) => dlopen(filename, flags);

                void* ILibDl.dlsym(nint handle, byte* symbol) => dlsym(handle, symbol);
            }
        }

        private interface ILibDl
        {
            IntPtr dlopen(byte* filename, int flags);

            void* dlsym(IntPtr handle, byte* symbol);
        }
    }
}
