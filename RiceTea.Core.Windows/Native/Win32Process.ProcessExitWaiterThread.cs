using System;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;

using RiceTea.Core.Windows.Helpers;

namespace RiceTea.Core.Windows.Native;

partial class Win32Process
{
    private sealed class ProcessExitWaiterThread : CriticalFinalizerObject, IDisposable
    {
        private static ulong _idCounter = 0;

        private readonly Win32Process _process;
        private readonly Thread _thread;
        private readonly TaskCompletionSource<bool> _completionSource;

        private long _state;

        public ProcessExitWaiterThread(Win32Process process)
        {
            _process = process;
            _thread = new Thread(ThreadMain) { IsBackground = true, Priority = ThreadPriority.Lowest };
            _thread.Start();
            _completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _state = 0L;
        }

        public Task WaitAsync() => _completionSource.Task;

        private void ThreadMain()
        {
            Win32Process process = _process;
            if (process.IsAlive)
            {
                if (Interlocked.Read(ref _state) != 0L)
                    return;
                ThreadHelper.SetCurrentThreadName($"{nameof(Win32Process)} Process Exit Waiter Thread #{Atomics.GetAndIncrement(ref _idCounter)}");
                process.WaitForExit();
                process.CloseChildStdPipeHandles();
            }
            if (Interlocked.CompareExchange(ref _state, 0L, 1L) != 0L) // Already disposed
                return;
            _completionSource.TrySetResult(true);
        }

        ~ProcessExitWaiterThread()
        {
            DisposeCore();
        }

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }

        private void DisposeCore()
        {
            if (Interlocked.CompareExchange(ref _state, 0L, 1L) != 0L) // Already disposed
                return;

            _completionSource.TrySetResult(false);
        }
    }
}
