using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using RiceTea.Core.Native;
using RiceTea.Core.Structures;

namespace RiceTea.Core.Windows.Internals;

[SuppressUnmanagedCodeSecurity]
internal static unsafe class Kernel32
{
    private const string LibraryName = "kernel32.dll";
    public const ulong PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1UL;
    public const ulong PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 1UL;

    private static readonly ConcurrentDictionary<IntPtr, string> _cachedModuleNames = new ConcurrentDictionary<IntPtr, string>();

    [DllImport(LibraryName)]
    public static extern IntPtr GetCurrentProcess();

    [DllImport(LibraryName)]
    public static extern SysBool32 CloseHandle(IntPtr hObject);

    [DllImport(LibraryName)]
    public static extern int GetCurrentThreadId();

    [DllImport(LibraryName)]
    public static extern SysBool32 GetProcessTimes(IntPtr hProcess, ulong* lpCreationTime, ulong* lpExitTime, ulong* lpKernelTime, ulong* lpUserTime);

    [DllImport(LibraryName)]
    public static extern IntPtr OpenProcess(GenericAccessRights dwDesiredAccess, SysBool32 bInheritHandle, uint dwProcessId);

    [DllImport(LibraryName)]
    public static extern IntPtr OpenThread(uint dwDesiredAccess, SysBool32 bInheritHandle, uint dwThreadId);

    [DllImport(LibraryName)]
    public static extern IntPtr LoadLibraryW(char* lpLibFileName);


    [DllImport(LibraryName)]
    public static extern SysBool32 CreateProcessW(char* lpApplicationName, char* lpCommandLine, void* lpProcessAttributes, void* lpThreadAttributes,
        SysBool32 bInheritHandles, ProcessCreationFlags dwCreationFlags, void* lpEnvironment, char* lpCurrentDirectory, StartupInfo* lpStartupInfo,
        ProcessInformation* lpProcessInformation);

    [DllImport(LibraryName)]
    public static extern SysBool32 TerminateProcess(IntPtr hProcess, uint uExitCode);

    [DllImport(LibraryName)]
    public static extern uint GetProcessId(IntPtr hProcess);

    [DllImport(LibraryName)]
    public static extern SysBool32 GetExitCodeProcess(IntPtr hProcess, uint* lpExitCode);

    [DllImport(LibraryName)]
    public static extern SysBool32 QueryFullProcessImageNameW(IntPtr hProcess, uint dwFlags, char* lpExeName, uint* lpdwSize);

    [DllImport(LibraryName)]
    public static extern uint ResumeThread(IntPtr hThread);

    [DllImport(LibraryName)]
    public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport(LibraryName)]
    public static extern SysBool32 CreatePipe(IntPtr* hReadPipe, IntPtr* hWritePipe, SecurityAttributes* lpPipeAttributes, uint nSize);

    [DllImport(LibraryName)]
    public static extern uint GetModuleFileNameW(IntPtr hModule, char* lpFilename, uint nSize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetModuleFileName(IntPtr hModule)
        => _cachedModuleNames.GetOrAdd(hModule, GetModuleFileNameCore);

    private static string GetModuleFileNameCore(IntPtr hModule)
    {
        if (TryGetModuleFileNameCoreFast(hModule, out string? result))
            return result;
        return GetModuleFileNameCoreSlow(hModule);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryGetModuleFileNameCoreFast(IntPtr hModule, [NotNullWhen(true)] out string? result)
    {
        char* buffer = stackalloc char[InternalConstants.MAX_PATH + 1];
        uint actualSize = GetModuleFileNameW(hModule, buffer, InternalConstants.MAX_PATH + 1);
        if (actualSize <= InternalConstants.MAX_PATH)
        {
            result = new string(buffer, 0, (int)actualSize);
            return true;
        }
        result = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string GetModuleFileNameCoreSlow(IntPtr hModule)
    {
        NativeMemoryPool pool = NativeMemoryPool.Shared;
        uint rentedLength = 1024;
        do
        {
            TypedNativeMemoryBlock<char> buffer = pool.Rent<char>(rentedLength);
            rentedLength = (uint)buffer.Length;
            try
            {
                char* ptr = buffer.NativePointer;
                uint actualSize = GetModuleFileNameW(hModule, ptr, rentedLength);
                if (actualSize < rentedLength)
                    return new string(ptr, 0, (int)actualSize);
                rentedLength <<= 1;
            }
            finally
            {
                pool.Return(buffer);
            }
        } while (true);
    }
}
