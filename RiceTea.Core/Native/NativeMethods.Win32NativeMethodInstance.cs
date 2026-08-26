using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

using InlineMethod;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;
using RiceTea.Core.Text;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    [SuppressUnmanagedCodeSecurity]
    private sealed unsafe class Win32NativeMethodInstance : INativeMethodInstance
    {
        private static readonly void* _waitOnAddressFunc, _wakeByAddressAllFunc;
        private readonly IntPtr _process;
#if !NET8_0_OR_GREATER
        private readonly IntPtr _heap;
#endif

        static Win32NativeMethodInstance()
        {
            _waitOnAddressFunc = GetImportedMethodPointerCore_Internal("kernelbase.dll", nameof(WaitOnAddress));
            _wakeByAddressAllFunc = GetImportedMethodPointerCore_Internal("kernelbase.dll", nameof(WakeByAddressAll));
        }

        [SuppressGCTransition]
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, EntryPoint = nameof(GetCurrentThreadId))]
        private static extern uint GetCurrentThreadIdCore();

        [SuppressGCTransition]
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetProcessHeap();

        [SuppressGCTransition]
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetCurrentProcess();

#if !NET8_0_OR_GREATER
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern void* HeapAlloc(IntPtr hHeap, int dwFlags, nuint size);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern void HeapFree(IntPtr hHeap, int dwFlags, void* ptr);
#endif

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern void* VirtualAlloc(void* address, nuint dwSize, MemoryAllocationTypes allocationTypes, PageAccessRights rights);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern SysBool32 VirtualProtect(void* address, nuint dwSize, PageAccessRights rights, PageAccessRights* oldRights);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern SysBool32 FlushInstructionCache(IntPtr hProcess, void* lpBaseAddress, nuint dwSize);

        [SuppressGCTransition]
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern uint GetCurrentProcessorNumber();

        [DllImport("ntdll", CallingConvention = CallingConvention.StdCall)]
        private static extern void RtlMoveMemory(void* dest, void* src, nuint sizeInBytes);

        [DllImport("ntdll", CallingConvention = CallingConvention.StdCall)]
        private static extern void RtlCopyMemory(void* dest, void* src, nuint sizeInBytes);

        [SuppressGCTransition]
        [DllImport("kernel32")]
        private static extern void QueryUnbiasedInterruptTime(ulong* pUnbiasedTime);

        [DllImport("ntdll")]
        private static extern uint NtDelayExecution(SysBool32 alertable, long* delayInterval);

        [SuppressGCTransition]
        [DllImport("kernel32")]
        private static extern void* GetProcAddress(IntPtr hModule, byte* lpProcName);

#if !NET8_0_OR_GREATER
        [DllImport("kernel32")]
        private static extern IntPtr LoadLibraryW(char* lpLibFileName);

        [DllImport("kernel32")]
        private static extern IntPtr GetModuleHandleW(char* lpModuleName);
#endif

        [DllImport("kernel32")]
        private static extern IntPtr CreateEventW(void* lpEventAttributes, SysBool32 bManualReset, SysBool32 bInitialState, char* lpName);

        [DllImport("kernel32")]
        private static extern SysBool32 SetEvent(IntPtr hEvent);

        [DllImport("kernel32")]
        private static extern SysBool32 ResetEvent(IntPtr hEvent);

        [DllImport("kernel32")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32")]
        private static extern SysBool32 CloseHandle(IntPtr hObject);

        [SuppressGCTransition]
        [DllImport("kernel32")]
        private static extern uint GetLastError();

        private static SysBool32 WaitOnAddress(void* address, void* compareAddress, nuint addressSize, uint dwMilliseconds)
        {
            void* func = _waitOnAddressFunc;
            DebugHelper.ThrowIf(func is null);
            return ((delegate* unmanaged[Stdcall]<void*, void*, nuint, uint, SysBool32>)func)(address, compareAddress, addressSize, dwMilliseconds);
        }

        private static void WakeByAddressAll(void* address)
        {
            void* func = _wakeByAddressAllFunc;
            DebugHelper.ThrowIf(func is null);
            ((delegate* unmanaged[Stdcall]<void*, void>)func)(address);
        }

        public Win32NativeMethodInstance()
        {
            _process = GetCurrentProcess();
#if !NET8_0_OR_GREATER
            _heap = GetProcessHeap();
#endif
        }

        public uint GetCurrentThreadId() => GetCurrentThreadIdCore();

        public uint GetCurrentProcessorId() => GetCurrentProcessorNumber();

        public ulong GetTicksForSystem()
        {
            ulong result;
            QueryUnbiasedInterruptTime(&result);
            return result;
        }

        public bool SleepInRelativeTicks(ulong ticks)
        {
            if (ticks <= 0)
                return false;
            SleepInRelativeTicksCore(ticks);
            return true;
        }

        public bool SleepInAbsoluteTicks(ulong ticks)
        {
            ulong currentTicks;
            QueryUnbiasedInterruptTime(&currentTicks);
            if (ticks <= currentTicks)
                return false;
            SleepInRelativeTicksCore(ticks - currentTicks);
            return true;
        }

        public void* GetImportedMethodPointer(string? dllName, int methodIndex) => GetImportedMethodPointerCore(dllName, methodIndex);

        public void* GetImportedMethodPointer(string? dllName, string methodName) => GetImportedMethodPointerCore(dllName, methodName);

        public void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices) => GetImportedMethodPointersCore(dllName, methodIndices);

        public void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<string> methodNames) => GetImportedMethodPointersCore(dllName, methodNames);

        public IntPtr CreateWaitingHandle(bool initialState, bool autoReset)
        {
            if (_waitOnAddressFunc is null)
                return CreateEventW(null, !autoReset, initialState, null);
            else
            {
                RawWaitingEvent* ptr = (RawWaitingEvent*)AllocMemory(UnsafeHelper.SizeOf<RawWaitingEvent>());
                *ptr = new RawWaitingEvent(initialState, autoReset);
                return RawWaitingEvent.GetWaitingHandleFromEvent(ptr);
            }
        }

        public void ResetWaitingHandle(IntPtr handle)
        {
            if (_waitOnAddressFunc is null)
                ResetEvent(handle);
            else
            {
                RawWaitingEvent.GetEventFromWaitingHandle(handle)->Reset();
            }
        }

        public void SetWaitingHandle(IntPtr handle)
        {
            if (_waitOnAddressFunc is null)
                SetEvent(handle);
            else
            {
                if (RawWaitingEvent.GetEventFromWaitingHandle(handle)->Set())
                    WakeByAddressAll((void*)handle);
            }
        }

        public void DestroyWaitingHandle(IntPtr handle)
        {
            if (_waitOnAddressFunc is null)
                CloseHandle(handle);
            else
            {
                RawWaitingEvent* ptr = RawWaitingEvent.GetEventFromWaitingHandle(handle);
                FreeMemory(ptr);
            }
        }

        public bool WaitForWaitingHandle(IntPtr handle, uint timeout)
        {
            if (_waitOnAddressFunc is null)
                return LegacyWait(handle, timeout);
            else
                return ModernWait(handle, timeout);
        }

#if !NET8_0_OR_GREATER
        public void* AllocMemory(nuint size) => HeapAlloc(_heap, 0, size);

        public void FreeMemory(void* ptr) => HeapFree(_heap, 0, ptr);
#endif

        public void CopyMemory(void* destination, void* source, nuint sizeInBytes) => RtlCopyMemory(destination, source, sizeInBytes);

        public void MoveMemory(void* destination, void* source, nuint sizeInBytes) => RtlMoveMemory(destination, source, sizeInBytes);

        public void* AllocMemoryPage(nuint size, ProtectMemoryPageFlags flags)
            => VirtualAlloc(null, size, MemoryAllocationTypes.Commit | MemoryAllocationTypes.Reserve, ConvertPageAccessRightsFromFlags(flags));

        public void ProtectMemoryPage(void* ptr, nuint size, ProtectMemoryPageFlags flags)
        {
            PageAccessRights rights = ConvertPageAccessRightsFromFlags(flags);
            PageAccessRights oldRights;
            VirtualProtect(ptr, size, rights, &oldRights);
        }

        public void FlushInstructionCache(void* ptr, nuint size) => FlushInstructionCache(_process, ptr, size);

        private static void* GetImportedMethodPointerCore(string? dllName, int methodIndex)
        {
            IntPtr module = dllName is null ? GetMainProgramHandle() : LoadLibrary(dllName);
            return GetProcAddress(module, (byte*)methodIndex);
        }

        private static void* GetImportedMethodPointerCore(string? dllName, string methodName)
        {
            IntPtr module = dllName is null ? GetMainProgramHandle() : LoadLibrary(dllName);

#if NET8_0_OR_GREATER
            return GetImportedMethodPointerCore(module, methodName);
#else
            ArrayPool<byte> pool = ArrayPool<byte>.Shared;

            return GetImportedMethodPointerCore(pool, module, methodName);
#endif
        }

#if NET8_0_OR_GREATER
        [Inline(InlineBehavior.Remove)]
        private static void* GetImportedMethodPointerCore_Internal(string? dllName, string methodName)
            => GetImportedMethodPointerCore(dllName, methodName);
#else
        private static void* GetImportedMethodPointerCore_Internal(string? dllName, string methodName)
        {
            IntPtr module = dllName is null ? GetMainProgramHandle() : LoadLibrary(dllName);

            ArrayPool<byte> pool = ArrayPool<byte>.Shared;
            if (pool is ArrayPool<byte>.SystemBufferImpl)
                return GetImportedMethodPointerCore(pool, module, methodName);

            return GetImportedMethodPointerCore(module, methodName);
        }
#endif

        public static void*[] GetImportedMethodPointersCore(string? dllName, ParamArrayTiny<int> methodIndices)
        {
            IntPtr module = dllName is null ? GetMainProgramHandle() : LoadLibrary(dllName);

            int length = methodIndices.Length;
            void*[] pointers = new void*[length];

            for (int i = 0; i < length; i++)
            {
                int methodIndex = methodIndices[i];
                pointers[i] = GetProcAddress(module, (byte*)methodIndex);
            }

            return pointers;
        }

        private static void*[] GetImportedMethodPointersCore(string? dllName, ParamArrayTiny<string> methodNames)
        {
            int length = methodNames.Length;
            if (length <= 0)
                return [];

            IntPtr module = dllName is null ? GetMainProgramHandle() : LoadLibrary(dllName);
            void*[] pointers = new void*[length];

#if NET8_0_OR_GREATER
            int i = 0;
            do
            {
                string methodName = methodNames[i];
                pointers[i] = GetImportedMethodPointerCore(module, methodName);
            } while (++i < length);
#else
            ArrayPool<byte> pool = ArrayPool<byte>.Shared;

            int i = 0;
            do
            {
                string methodName = methodNames[i];
                pointers[i] = GetImportedMethodPointerCore(pool, module, methodName);
            } while (++i < length);
#endif

            return pointers;
        }

#if !NET8_0_OR_GREATER
        private static void* GetImportedMethodPointerCore(ArrayPool<byte> pool, IntPtr module, string methodName)
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
                    return GetProcAddress(module, destination);
                }
            }
            finally
            {
                pool.Return(buffer);
            }
        }
#endif

        private static void* GetImportedMethodPointerCore(IntPtr module, string methodName)
        {
#if NET8_0_OR_GREATER
            return NativeLibrary.TryGetExport(module, methodName, out module) ? module.ToPointer() : null;
#else
            int length = methodName.Length;
            byte[] buffer = new byte[length + 1];
            fixed (char* source = methodName)
            fixed (byte* destination = buffer)
            {
                AsciiEncodingHelper.ReadFromUtf16Buffer(source, source + length, destination, destination + length);
                destination[length] = 0;
                return GetProcAddress(module, destination);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr GetMainProgramHandle()
        {
#if NET8_0_OR_GREATER
            return NativeLibrary.GetMainProgramHandle();
#else
            return GetModuleHandleW(null);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr LoadLibrary(string lpLibFileName)
        {
#if NET8_0_OR_GREATER
            return NativeLibrary.TryLoad(lpLibFileName, out IntPtr result) ? result : IntPtr.Zero;
#else
            fixed (char* ptr = lpLibFileName)
                return LoadLibraryW(ptr);
#endif
        }

        private static bool LegacyWait(IntPtr waitingHandle, uint timeout)
        {
            const uint INFINITE = unchecked((uint)Timeout.Infinite);
            const uint WAIT_TIMEOUT = 0x00000102U;

            if (timeout == INFINITE)
            {
                WaitForSingleObject(waitingHandle, dwMilliseconds: timeout);
                return true;
            }

            return WaitForSingleObject(waitingHandle, dwMilliseconds: timeout) != WAIT_TIMEOUT;
        }

        private static bool ModernWait(IntPtr waitingHandle, uint timeout)
        {
            const uint INFINITE = unchecked((uint)Timeout.Infinite);
            const int ERROR_TIMEOUT = 0x5B4;
            uint lastError;

            SysBool32 result;
            RawWaitingEvent* ptr = RawWaitingEvent.GetEventFromWaitingHandle(waitingHandle);
            if (ptr->IsAutoReset)
            {
                do
                {
                    result = SysBool32.False;
                    result = WaitOnAddress((void*)waitingHandle, &result, RawWaitingEvent.HandleSize, timeout);
                } while (result && !ptr->Reset());
            }
            else
            {
                result = SysBool32.False;
                result = WaitOnAddress((void*)waitingHandle, &result, RawWaitingEvent.HandleSize, timeout);
            }
            if (result || timeout == INFINITE || (lastError = GetLastError()) == ERROR_TIMEOUT)
                return result;

            throw new Win32Exception((int)lastError);
        }

        private static PageAccessRights ConvertPageAccessRightsFromFlags(ProtectMemoryPageFlags flags)
        {
            if ((flags & ProtectMemoryPageFlags.CanExecute) == ProtectMemoryPageFlags.CanExecute)
            {
                if ((flags & ProtectMemoryPageFlags.CanRead) == ProtectMemoryPageFlags.CanRead)
                {
                    if ((flags & ProtectMemoryPageFlags.CanWrite) == ProtectMemoryPageFlags.CanWrite)
                        return PageAccessRights.ExecuteReadWrite;
                    return PageAccessRights.ExecuteRead;
                }
                return PageAccessRights.Execute;
            }
            else
            {
                if ((flags & ProtectMemoryPageFlags.CanRead) == ProtectMemoryPageFlags.CanRead)
                {
                    if ((flags & ProtectMemoryPageFlags.CanWrite) == ProtectMemoryPageFlags.CanWrite)
                        return PageAccessRights.ReadWrite;
                    return PageAccessRights.ReadOnly;
                }
                return PageAccessRights.NoAccess;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SleepInRelativeTicksCore(ulong ticks)
        {
            if (ticks > long.MaxValue)
            {
                long time = -long.MaxValue;
                NtDelayExecution(alertable: SysBool32.False, &time);
                time = -(long)(ticks - long.MaxValue);
                NtDelayExecution(alertable: SysBool32.False, &time);
            }
            else
            {
                ticks = UnsafeHelper.Negate(ticks);
                NtDelayExecution(alertable: SysBool32.False, (long*)&ticks);
            }
        }

        [Flags]
        private enum MemoryAllocationTypes : uint
        {
            None = 0,
            Commit = 0x00001000,
            Reserve = 0x00002000,
            ReplacePlaceholder = 0x00004000,
            ReservePlaceholder = 0x00040000,
            Reset = 0x00080000,
            TopDown = 0x00100000,
            WriteWatch = 0x00200000,
            Physical = 0x00400000,
            Rotate = 0x00800000,
            DifferenceImageBaseOk = 0x00800000,
            ResetUndo = 0x01000000,
            LargePages = 0x20000000,
            Alloc4MbPages = 0x80000000,
            Alloc64KPages = (LargePages | Physical),
            UnmapWithTransientBoost = 0x00000001,
            Coalesce_Placeholders = 0x00000001,
            PreservePlaceholder = 0x00000002,
            Decommit = 0x00004000,
            Release = 0x00008000,
            Free = 0x00010000
        }

        [Flags]
        private enum PageAccessRights : uint
        {
            None = 0x00,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            Guard = 0x100,
            NoCache = 0x200,
            WriteCombine = 0x400,
            GraphicsNoAccess = 0x0800,
            GraphicsReadOnly = 0x1000,
            GraphicsReadWrite = 0x2000,
            GraphicsExecute = 0x4000,
            GraphicsExecuteRead = 0x8000,
            GraphicsExecuteReadWrite = 0x10000,
            GraphicsConherent = 0x20000,
            GraphicsNoCache = 0x40000,
            EnclaveThreadControl = 0x80000000,
            RevertToFileMap = 0x80000000,
            TargetsNoUpdate = 0x40000000,
            TargetsInvalid = 0x40000000,
            EnclaveUnvalidated = 0x20000000,
            EnclaveMask = 0x10000000,
            EnclaveDecommit = (EnclaveMask | 0),
            EnclaveSSFirst = (EnclaveMask | 1),
            EnclaveSSRest = (EnclaveMask | 2),
        }
    }
}
