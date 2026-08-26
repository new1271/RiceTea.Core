using System;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

namespace RiceTea.Core.Native;

public static unsafe partial class NativeMethods
{
    private static readonly INativeMethodInstance _methodInstance = GetOSDependedInstance();

    [ThreadStatic]
    private static uint _currentThreadId;

    [Inline(InlineBehavior.Remove)]
    private static INativeMethodInstance GetOSDependedInstance()
    {
        if (PlatformHelper.IsWindows)
            return new Win32Instance();
        if (PlatformHelper.IsUnix)
            return new UnixInstance();
        return new FallbackInstance();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetCurrentThreadId()
    {
        uint threadId = _currentThreadId;
        if (threadId == 0)
            _currentThreadId = threadId = GetCurrentThreadIdCore();
        return threadId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetCurrentThreadIdCore() => _methodInstance switch
    {
        Win32Instance => Win32Instance.GetCurrentThreadId(),
        UnixInstance => UnixInstance.GetCurrentThreadId(),
        FallbackInstance => FallbackInstance.GetCurrentThreadId(),
        _ => _methodInstance.GetCurrentThreadId()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetCurrentProcessorId() => _methodInstance switch
    {
        Win32Instance => Win32Instance.GetCurrentProcessorId(),
        UnixInstance => UnixInstance.GetCurrentProcessorId(),
        FallbackInstance => FallbackInstance.GetCurrentProcessorId(),
        _ => _methodInstance.GetCurrentProcessorId()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetTicksForSystem() => _methodInstance switch
    {
        Win32Instance => Win32Instance.GetTicksForSystem(),
        UnixInstance => UnixInstance.GetTicksForSystem(),
        FallbackInstance => FallbackInstance.GetTicksForSystem(),
        _ => _methodInstance.GetTicksForSystem()
    };

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntPtr CreateWaitingHandle(bool autoReset) => CreateWaitingHandle(initialState: false, autoReset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntPtr CreateWaitingHandle(bool initialState, bool autoReset) => _methodInstance switch
    {
        Win32Instance => Win32Instance.CreateWaitingHandle(initialState, autoReset),
        UnixInstance => UnixInstance.CreateWaitingHandle(initialState, autoReset),
        FallbackInstance => FallbackInstance.CreateWaitingHandle(initialState, autoReset),
        _ => _methodInstance.CreateWaitingHandle(initialState, autoReset)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ResetWaitingHandle(IntPtr handle)
    {
        switch (_methodInstance)
        {
            case Win32Instance:
                Win32Instance.ResetWaitingHandle(handle);
                break;
            case UnixInstance:
                UnixInstance.ResetWaitingHandle(handle);
                break;
            case FallbackInstance:
                FallbackInstance.ResetWaitingHandle(handle);
                break;
            default:
                _methodInstance.ResetWaitingHandle(handle);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetWaitingHandle(IntPtr handle)
    {
        switch (_methodInstance)
        {
            case Win32Instance:
                Win32Instance.SetWaitingHandle(handle);
                break;
            case UnixInstance:
                UnixInstance.SetWaitingHandle(handle);
                break;
            case FallbackInstance:
                FallbackInstance.SetWaitingHandle(handle);
                break;
            default:
                _methodInstance.SetWaitingHandle(handle);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DestroyWaitingHandle(IntPtr handle)
    {
        switch (_methodInstance)
        {
            case Win32Instance:
                Win32Instance.DestroyWaitingHandle(handle);
                break;
            case UnixInstance:
                UnixInstance.DestroyWaitingHandle(handle);
                break;
            case FallbackInstance:
                FallbackInstance.DestroyWaitingHandle(handle);
                break;
            default:
                _methodInstance.DestroyWaitingHandle(handle);
                break;
        }
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WaitForWaitingHandle(IntPtr handle) => WaitForWaitingHandle(handle, unchecked((uint)Timeout.Infinite));

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WaitForWaitingHandle(IntPtr handle, TimeSpan timeout)
        => WaitForWaitingHandle(handle, (uint)(ulong)timeout.Ticks);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WaitForWaitingHandle(IntPtr handle, uint timeout) => _methodInstance switch
    {
        Win32Instance => Win32Instance.WaitForWaitingHandle(handle, timeout),
        UnixInstance => UnixInstance.WaitForWaitingHandle(handle, timeout),
        FallbackInstance => FallbackInstance.WaitForWaitingHandle(handle, timeout),
        _ => _methodInstance.WaitForWaitingHandle(handle, timeout)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SleepInRelativeTicks(ulong ticks) => _methodInstance switch
    {
        Win32Instance => Win32Instance.SleepInRelativeTicks(ticks),
        UnixInstance => UnixInstance.SleepInRelativeTicks(ticks),
        FallbackInstance => FallbackInstance.SleepInRelativeTicks(ticks),
        _ => _methodInstance.SleepInRelativeTicks(ticks)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SleepInAbsoluteTicks(ulong ticks) => _methodInstance switch
    {
        Win32Instance => Win32Instance.SleepInAbsoluteTicks(ticks),
        UnixInstance => UnixInstance.SleepInAbsoluteTicks(ticks),
        FallbackInstance => FallbackInstance.SleepInAbsoluteTicks(ticks),
        _ => _methodInstance.SleepInAbsoluteTicks(ticks)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* GetImportedMethodPointer(string dllName, int methodIndex) => _methodInstance switch
    {
        Win32Instance => Win32Instance.GetImportedMethodPointer(dllName, methodIndex),
        UnixInstance => UnixInstance.GetImportedMethodPointer(dllName, methodIndex),
        FallbackInstance => FallbackInstance.GetImportedMethodPointer(dllName, methodIndex),
        _ => _methodInstance.GetImportedMethodPointer(dllName, methodIndex)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* GetImportedMethodPointer(string dllName, string methodName) => _methodInstance switch
    {
        Win32Instance => Win32Instance.GetImportedMethodPointer(dllName, methodName),
        UnixInstance => UnixInstance.GetImportedMethodPointer(dllName, methodName),
        FallbackInstance => FallbackInstance.GetImportedMethodPointer(dllName, methodName),
        _ => _methodInstance.GetImportedMethodPointer(dllName, methodName)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void*[] GetImportedMethodPointers(string dllName, int methodIndex)
        => GetImportedMethodPointers(dllName, new ParamArrayTiny<int>(methodIndex));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void*[] GetImportedMethodPointers(string dllName, int methodIndex1, int methodIndex2)
        => GetImportedMethodPointers(dllName, new ParamArrayTiny<int>(methodIndex1, methodIndex2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void*[] GetImportedMethodPointers(string dllName, int methodIndex1, int methodIndex2, int methodIndex3)
        => GetImportedMethodPointers(dllName, new ParamArrayTiny<int>(methodIndex1, methodIndex2, methodIndex3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void*[] GetImportedMethodPointers(string dllName, params int[] methodIndices)
        => GetImportedMethodPointers(dllName, new ParamArrayTiny<int>(methodIndices));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void*[] GetImportedMethodPointers(string dllName, string methodName)
        => GetImportedMethodPointers(dllName, new ParamArrayTiny<string>(methodName));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void*[] GetImportedMethodPointers(string dllName, string methodName1, string methodName2)
        => GetImportedMethodPointers(dllName, new ParamArrayTiny<string>(methodName1, methodName2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void*[] GetImportedMethodPointers(string dllName, string methodName1, string methodName2, string methodName3)
        => GetImportedMethodPointers(dllName, new ParamArrayTiny<string>(methodName1, methodName2, methodName3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void*[] GetImportedMethodPointers(string dllName, params string[] methodNames)
        => GetImportedMethodPointers(dllName, new ParamArrayTiny<string>(methodNames));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void*[] GetImportedMethodPointers(string dllName, in ParamArrayTiny<int> methodIndices) => _methodInstance switch
    {
        Win32Instance => Win32Instance.GetImportedMethodPointers(dllName, methodIndices),
        UnixInstance => UnixInstance.GetImportedMethodPointers(dllName, methodIndices),
        FallbackInstance => FallbackInstance.GetImportedMethodPointers(dllName, methodIndices),
        _ => _methodInstance.GetImportedMethodPointers(dllName, methodIndices)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void*[] GetImportedMethodPointers(string dllName, in ParamArrayTiny<string> methodNames) => _methodInstance switch
    {
        Win32Instance => Win32Instance.GetImportedMethodPointers(dllName, methodNames),
        UnixInstance => UnixInstance.GetImportedMethodPointers(dllName, methodNames),
        FallbackInstance => FallbackInstance.GetImportedMethodPointers(dllName, methodNames),
        _ => _methodInstance.GetImportedMethodPointers(dllName, methodNames)
    };
}
