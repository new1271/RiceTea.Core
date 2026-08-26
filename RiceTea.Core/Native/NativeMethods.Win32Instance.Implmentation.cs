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
    unsafe partial class Win32Instance
    {
        private static readonly void* _waitOnAddressFunc, _wakeByAddressAllFunc;
        private static readonly IntPtr _process = GetCurrentProcess();

        static Win32Instance()
        {
            _waitOnAddressFunc = GetImportedMethodPointer("kernelbase.dll", nameof(WaitOnAddress));
            _wakeByAddressAllFunc = GetImportedMethodPointer("kernelbase.dll", nameof(WakeByAddressAll));
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

        public static uint GetCurrentThreadId() => GetCurrentThreadIdCore();

        public static uint GetCurrentProcessorId() => GetCurrentProcessorNumber();

        public static ulong GetTicksForSystem()
        {
            ulong result;
            QueryUnbiasedInterruptTime(&result);
            return result;
        }

        public static bool SleepInRelativeTicks(ulong ticks)
        {
            if (ticks <= 0)
                return false;
            SleepInRelativeTicksCore(ticks);
            return true;
        }

        public static bool SleepInAbsoluteTicks(ulong ticks)
        {
            ulong currentTicks;
            QueryUnbiasedInterruptTime(&currentTicks);
            if (ticks <= currentTicks)
                return false;
            SleepInRelativeTicksCore(ticks - currentTicks);
            return true;
        }

        public static IntPtr CreateWaitingHandle(bool initialState, bool autoReset)
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

        public static void ResetWaitingHandle(IntPtr handle)
        {
            if (_waitOnAddressFunc is null)
                ResetEvent(handle);
            else
            {
                RawWaitingEvent.GetEventFromWaitingHandle(handle)->Reset();
            }
        }

        public static void SetWaitingHandle(IntPtr handle)
        {
            if (_waitOnAddressFunc is null)
                SetEvent(handle);
            else
            {
                if (RawWaitingEvent.GetEventFromWaitingHandle(handle)->Set())
                    WakeByAddressAll((void*)handle);
            }
        }

        public static void DestroyWaitingHandle(IntPtr handle)
        {
            if (_waitOnAddressFunc is null)
                CloseHandle(handle);
            else
            {
                RawWaitingEvent* ptr = RawWaitingEvent.GetEventFromWaitingHandle(handle);
                FreeMemory(ptr);
            }
        }

        public static bool WaitForWaitingHandle(IntPtr handle, uint timeout)
        {
            if (_waitOnAddressFunc is null)
                return LegacyWait(handle, timeout);
            else
                return ModernWait(handle, timeout);
        }

        public static void CopyMemory(void* destination, void* source, nuint sizeInBytes) => RtlCopyMemory(destination, source, sizeInBytes);

        public static void MoveMemory(void* destination, void* source, nuint sizeInBytes) => RtlMoveMemory(destination, source, sizeInBytes);

        public static void* AllocMemoryPage(nuint size, ProtectMemoryPageFlags flags)
            => VirtualAlloc(null, size, MemoryAllocationTypes.Commit | MemoryAllocationTypes.Reserve, ConvertPageAccessRightsFromFlags(flags));

        public static void ProtectMemoryPage(void* ptr, nuint size, ProtectMemoryPageFlags flags)
        {
            PageAccessRights rights = ConvertPageAccessRightsFromFlags(flags);
            PageAccessRights oldRights;
            VirtualProtect(ptr, size, rights, &oldRights);
        }

        public static void FlushInstructionCache(void* ptr, nuint size) => FlushInstructionCache(_process, ptr, size);

        public static partial void* GetImportedMethodPointer(string? dllName, int methodIndex);

        public static partial void* GetImportedMethodPointer(string? dllName, string methodName);

        public static partial void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices);

        public static partial void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<string> methodNames);

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
