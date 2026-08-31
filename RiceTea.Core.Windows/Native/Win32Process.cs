using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Win32.SafeHandles;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Text;
using RiceTea.Core.Threading;
using RiceTea.Core.Windows.Internals;

namespace RiceTea.Core.Windows.Native;

public sealed partial class Win32Process : CriticalFinalizerObject, IDisposable
{
    private static readonly Win32Process _current = OpenCurrent();

    private readonly LazyTiny<ProcessExitWaiterThread, Win32Process> _exitWaiterLazy;
    private readonly LazyTiny<string> _nameLazy;
    private readonly Stream _stdIn, _stdOut, _stdErr;

    private readonly string? _workingDirectory;
    private readonly IntPtr _handle, _childStdInHandle, _childStdOutHandle, _childStdErrHandle;
    private readonly uint _id;

    private bool _disposed, _processHandleClosed, _childStdPipeHandleClosed, _stdPipeClosed;

    public static Win32Process Current => _current;

    public bool Disposed => _disposed;

    public uint Id => _id;

    public IntPtr Handle => _handle;

    public unsafe bool IsAlive
    {
        get
        {
            IntPtr hProcess = _handle;
            if (hProcess == IntPtr.Zero)
                return false;
            uint exitCode;
            if (!Kernel32.GetExitCodeProcess(hProcess, &exitCode))
                return false;
            return exitCode == 259;
        }
    }

    public unsafe DateTime StartTime
    {
        get
        {
            IntPtr hProcess = _handle;
            if (hProcess == IntPtr.Zero)
                return DateTime.MinValue;
            ulong creationTimeInTicks, reserved;
            if (!Kernel32.GetProcessTimes(hProcess, &creationTimeInTicks, &reserved, &reserved, &reserved))
                return DateTime.MinValue;
            return DateTime.FromFileTime((long)creationTimeInTicks);
        }
    }

    public string? WorkingDirectory => _workingDirectory;

    public bool IsStdPipeClosed => _disposed || _childStdPipeHandleClosed && _stdPipeClosed;

    public string ImageName => _nameLazy.Value;

    public Stream In => _stdIn;

    public Stream Out => _stdOut;

    public Stream Error => _stdErr;

    private Win32Process(string? workingDirectory, uint id, IntPtr handle, bool keepHandleWhenDisposing)
    {
        _disposed = false;
        _childStdPipeHandleClosed = true;
        _stdPipeClosed = true;
        _processHandleClosed = keepHandleWhenDisposing;
        _workingDirectory = workingDirectory;
        _id = id;
        _handle = handle;
        _stdIn = Stream.Null;
        _stdOut = Stream.Null;
        _stdErr = Stream.Null;
        _exitWaiterLazy = new LazyTiny<ProcessExitWaiterThread, Win32Process>(_this => new ProcessExitWaiterThread(_this), this, LazyThreadSafetyMode.ExecutionAndPublication);
        _nameLazy = new LazyTiny<string>(GetProcessImageNameCore);
    }

    private Win32Process(string? workingDirectory, uint id, IntPtr handle, IntPtr stdIn, IntPtr stdOut, IntPtr stdErr, in StartupInfo processStartupInfo)
    {
        _disposed = false;
        _childStdPipeHandleClosed = false;
        _stdPipeClosed = false;
        _processHandleClosed = false;
        _workingDirectory = workingDirectory;
        _id = id;
        _handle = handle;
        _childStdInHandle = processStartupInfo.hStdInput;
        _childStdOutHandle = processStartupInfo.hStdOutput;
        _childStdErrHandle = processStartupInfo.hStdError;
        _stdIn = new FileStream(new SafeFileHandle(stdIn, ownsHandle: true), FileAccess.Write);
        _stdOut = new FileStream(new SafeFileHandle(stdOut, ownsHandle: true), FileAccess.Read);
        _stdErr = new FileStream(new SafeFileHandle(stdErr, ownsHandle: true), FileAccess.Read);
        _exitWaiterLazy = new(_this => new ProcessExitWaiterThread(_this), this);
        _ = _exitWaiterLazy.Value;
        _nameLazy = new LazyTiny<string>(GetProcessImageNameCore);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Start(string fileName, string arguments, string workingDirectory)
        => TryCreateProcess(fileName, arguments, false, ProcessCreationFlags.CreateNoWindow,
            workingDirectory, new StartupInfo() { cb = UnsafeHelper.SizeOf<StartupInfo>() }, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCreate(string fileName, string? arguments, string? workingDirectory,
        [NotNullWhen(true)] out Win32Process? result)
        => TryCreateCoreFast(fileName, arguments, workingDirectory, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCreate(string fileName, string? arguments, string? workingDirectory, bool redirectStdPipes,
        [NotNullWhen(true)] out Win32Process? result)
        => redirectStdPipes ?
        TryCreateCoreSlow(fileName, arguments, workingDirectory, out result) :
        TryCreateCoreFast(fileName, arguments, workingDirectory, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryCreateCoreFast(string fileName, string? arguments, string? workingDirectory,
        [NotNullWhen(true)] out Win32Process? result)
    {
        if (!TryCreateProcess(fileName, arguments, false, ProcessCreationFlags.CreateNoWindow,
            workingDirectory, new StartupInfo() { cb = UnsafeHelper.SizeOf<StartupInfo>() }, out ProcessInformation processInformation))
        {
            result = null;
            return false;
        }
        Kernel32.CloseHandle(processInformation.hThread);
        result = new Win32Process(workingDirectory, processInformation.dwProcessId, processInformation.hProcess, keepHandleWhenDisposing: false);
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe bool TryCreateCoreSlow(string fileName, string? arguments, string? workingDirectory,
        [NotNullWhen(true)] out Win32Process? result)
    {
        IntPtr stdInRead, stdInWrite, stdOutRead, stdOutWrite, stdErrRead, stdErrWrite;
        SecurityAttributes securityAttributes = new SecurityAttributes()
        {
            bInheritHandle = true,
            lpSecurityDescriptor = null,
            nLength = 0,
        };
        if (!Kernel32.CreatePipe(&stdInRead, &stdInWrite, &securityAttributes, 0))
            goto Failed;
        if (!Kernel32.CreatePipe(&stdOutRead, &stdOutWrite, &securityAttributes, 0))
        {
            Kernel32.CloseHandle(stdInRead);
            Kernel32.CloseHandle(stdInWrite);
            goto Failed;
        }
        if (!Kernel32.CreatePipe(&stdErrRead, &stdErrWrite, &securityAttributes, 0))
        {
            Kernel32.CloseHandle(stdInRead);
            Kernel32.CloseHandle(stdInWrite);
            Kernel32.CloseHandle(stdOutRead);
            Kernel32.CloseHandle(stdOutWrite);
            goto Failed;
        }

        StartupInfo startupInfo = new StartupInfo()
        {
            cb = UnsafeHelper.SizeOf<StartupInfo>(),
            dwFlags = StartupInfoFlags.UseStdHandles,
            hStdInput = stdInRead,
            hStdOutput = stdOutWrite,
            hStdError = stdErrWrite,
        };
        if (!TryCreateProcess(fileName, arguments, true,
            ProcessCreationFlags.CreateNoWindow | ProcessCreationFlags.CreateSuspended,
            workingDirectory, startupInfo, out ProcessInformation processInformation))
        {
            Kernel32.CloseHandle(stdInRead);
            Kernel32.CloseHandle(stdInWrite);
            Kernel32.CloseHandle(stdOutRead);
            Kernel32.CloseHandle(stdOutWrite);
            Kernel32.CloseHandle(stdErrRead);
            Kernel32.CloseHandle(stdErrWrite);
            goto Failed;
        }
        IntPtr threadHandle = processInformation.hThread;
        result = new Win32Process(workingDirectory, processInformation.dwProcessId, processInformation.hProcess,
            stdInWrite, stdOutRead, stdErrRead, startupInfo);
        Kernel32.ResumeThread(threadHandle);
        Kernel32.CloseHandle(threadHandle);
        return true;

    Failed:
        result = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe bool TryCreateProcess(string fileName, string? argument, bool inheritHandles, ProcessCreationFlags dwCreationFlags,
        string? workingDirectory, in StartupInfo startupInfo, out ProcessInformation processInformation)
    {
        int predictedSpace = CalculateSpaceForBuffer(fileName, argument);
        DebugHelper.ThrowIf(predictedSpace < 0);

        NativeMemoryPool pool = NativeMemoryPool.Shared;
        TypedNativeMemoryBlock<char> buffer = pool.Rent<char>(predictedSpace);
        try
        {
            using StringBuilderTiny builder = new StringBuilderTiny();
            char* ptr = buffer.NativePointer;
            builder.SetStartPointer(ptr, predictedSpace);
            if (!fileName.StartsWith('\"'))
                builder.Append('\"');
            builder.Append(fileName);
            if (!fileName.EndsWith('\"'))
                builder.Append('\"');
            if (!StringHelper.IsNullOrEmpty(argument))
            {
                builder.Append(' ');
                builder.Append(argument);
            }
            ptr[builder.Length] = '\0';
            fixed (char* pCurrentDirectory = workingDirectory)
            {
                return Kernel32.CreateProcessW(null, ptr, null, null, inheritHandles, dwCreationFlags, null, pCurrentDirectory,
                    UnsafeHelper.AsPointerIn(in startupInfo), UnsafeHelper.AsPointerOut(out processInformation));
            }
        }
        finally
        {
            pool.Return(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int CalculateSpaceForBuffer(string fileName, string? argument)
        {
            int length = fileName.Length;
            if (!fileName.StartsWith('\"'))
                length++;
            if (!fileName.EndsWith('\"'))
                length++;
            if (!StringHelper.IsNullOrEmpty(argument))
                length += argument.Length + 1;
            return length + 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryOpenExists(uint id, [NotNullWhen(true)] out Win32Process? result)
    {
        IntPtr handle = Kernel32.OpenProcess(GenericAccessRights.Read, false, id);
        if (handle == IntPtr.Zero)
        {
            result = null;
            return false;
        }
        result = new Win32Process(null, id, handle, keepHandleWhenDisposing: false);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Win32Process? FromHandle(IntPtr handle, bool leaveOpen)
    {
        if (handle == IntPtr.Zero)
            return null;
        return new Win32Process(null, Kernel32.GetProcessId(handle), handle, keepHandleWhenDisposing: leaveOpen);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Win32Process OpenCurrent()
    {
        IntPtr handle = Kernel32.GetCurrentProcess();
        return new Win32Process(Environment.CurrentDirectory, Kernel32.GetProcessId(handle), handle, keepHandleWhenDisposing: true); ;
    }

    private string GetProcessImageNameCore()
    {
        IntPtr handle = _handle;
        if (handle == IntPtr.Zero)
            return string.Empty;
        return GetProcessNameCoreSmall(handle) ?? GetProcessNameCoreLarge(handle) ?? GetProcessNameCoreHuge(handle) ?? string.Empty;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [SkipLocalsInit]
    private unsafe string? GetProcessNameCoreSmall(IntPtr handle)
    {
        char* buffer = stackalloc char[InternalConstants.MAX_PATH + 1];
        uint length = InternalConstants.MAX_PATH + 1;
        if (!Kernel32.QueryFullProcessImageNameW(handle, 0, buffer, &length) || length >= InternalConstants.MAX_PATH + 1)
            return null;
        return new string(buffer, 0, unchecked((int)length));
    }

    private static unsafe string? GetProcessNameCoreLarge(IntPtr handle)
    {
        NativeMemoryPool pool = NativeMemoryPool.Shared;
        for (uint baseSize = InternalConstants.MAX_PATH << 1; baseSize < 32767u; baseSize <<= 1)
        {
            uint length = baseSize + 1;
            TypedNativeMemoryBlock<char> buffer = pool.Rent<char>(length);
            try
            {
                char* ptr = buffer.NativePointer;
                if (!Kernel32.QueryFullProcessImageNameW(handle, 0, ptr, &length))
                    return null;
                if (length > baseSize)
                    continue;
                return new string(ptr, 0, unchecked((int)length));
            }
            finally
            {
                pool.Return(buffer);
            }
        }
        return null;
    }

    private static unsafe string? GetProcessNameCoreHuge(IntPtr handle)
    {
        const int MaxAllowedPathLength = 32767;

        NativeMemoryPool pool = NativeMemoryPool.Shared;

        TypedNativeMemoryBlock<char> buffer = pool.Rent<char>(MaxAllowedPathLength + 1);
        try
        {
            uint length = MaxAllowedPathLength + 1;

            char* ptr = buffer.NativePointer;
            if (!Kernel32.QueryFullProcessImageNameW(handle, 0, ptr, &length))
                return null;

            return new string(ptr, 0, unchecked((int)length));
        }
        finally
        {
            pool.Return(buffer);
        }
    }

    public void WaitForExit()
    {
        IntPtr handle = _handle;
        if (handle == IntPtr.Zero)
            return;
        Kernel32.WaitForSingleObject(handle, unchecked((uint)Timeout.Infinite));
        CloseProcessHandle();
        CloseChildStdPipeHandles();
    }

    public bool WaitForExit(int timeout)
    {
        IntPtr handle = _handle;
        if (handle == IntPtr.Zero)
            return true;
        if (Kernel32.WaitForSingleObject(handle, unchecked((uint)timeout)) == 0x00000102L)
            return false;
        CloseProcessHandle();
        CloseChildStdPipeHandles();
        return true;
    }

    public Task WaitForExitAsync()
    {
        Task? task = _exitWaiterLazy.GetValueDirectly()?.WaitAsync();
        if (task is not null)
            return task;
        if (!IsAlive)
            return Task.CompletedTask;
        return _exitWaiterLazy.Value.WaitAsync();
    }

    public async Task<bool> WaitForExitAsync(int timeout)
    {
        if (!IsAlive)
            return true;
        Task workingTask = WaitForExitAsync();
        Task timeoutTask = Task.Delay(timeout);
        return ReferenceEquals(await Task.WhenAny(workingTask, timeoutTask), workingTask);
    }

    public void Terminate()
    {
        IntPtr handle = _handle;
        if (handle == IntPtr.Zero || !Kernel32.TerminateProcess(handle, 0))
            return;
        CloseProcessHandle();
        CloseChildStdPipeHandles();
    }

    private void CloseChildStdPipeHandles()
    {
        if (_childStdPipeHandleClosed)
            return;
        _childStdPipeHandleClosed = true;
        Kernel32.CloseHandle(_childStdInHandle);
        Kernel32.CloseHandle(_childStdOutHandle);
        Kernel32.CloseHandle(_childStdErrHandle);
    }

    private void CloseProcessHandle()
    {
        if (_processHandleClosed)
            return;
        _processHandleClosed = true;
        Kernel32.CloseHandle(_handle);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;

        if (disposing)
        {
            _exitWaiterLazy.GetValueDirectly()?.Dispose();
            if (!_stdPipeClosed)
            {
                _stdPipeClosed = true;
                _stdIn?.Dispose();
                _stdOut?.Dispose();
                _stdErr?.Dispose();
            }
        }
        CloseProcessHandle();
        CloseChildStdPipeHandles();
    }

    ~Win32Process()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
