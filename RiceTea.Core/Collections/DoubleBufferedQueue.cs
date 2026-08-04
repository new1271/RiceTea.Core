using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace RiceTea.Core.Collections;

public sealed class DoubleBufferedQueue<T> : CriticalFinalizerObject
{
    private readonly record struct LockableQueue(Queue<T> Queue, Lock Lock)
    {
        public LockableQueue() : this(new Queue<T>(), new Lock()) { }

        public LockableQueue(int capacity) : this(new Queue<T>(capacity), new Lock()) { }
    }

    private readonly Lock _globalLock = new Lock();
    private readonly IntPtr _waitingHandle = NativeMethods.CreateWaitingHandle(autoReset: true);

    private LockableQueue _frontQueue, _backQueue;

    public DoubleBufferedQueue()
    {
        _frontQueue = new LockableQueue();
        _backQueue = new LockableQueue();
    }

    public DoubleBufferedQueue(int capacity)
    {
        _frontQueue = new LockableQueue(capacity);
        _backQueue = new LockableQueue(capacity);
    }

    public void Append(T item)
    {
        LockableQueue queue = GetFrontQueue();
        if (Core(in queue, item))
            goto Tail;
        SpinWait spinWait = new SpinWait();
        do
        {
            spinWait.SpinOnce();
            queue = GetFrontQueue();
        } while (!Core(in queue, item));
        goto Tail;

    Tail:
        SendSignal();

        static bool Core(in LockableQueue queue, T item)
        {
            Lock @lock = queue.Lock;
            if (!@lock.TryEnter())
                return false;
            try
            {
                queue.Queue.Enqueue(item);
            }
            finally
            {
                @lock.Exit();
            }
            return true;
        }
    }

    public void Append(params T[]? items)
    {
        int length;
        if (items is null || (length = items.Length) <= 0)
            return;

        AppendCore(in UnsafeHelper.GetArrayDataReference(items), length);
    }

    public void Append(in ArrayPool<T>.RentScope rentScope)
    {
        int length;
        if ((length = rentScope.Count) <= 0)
            return;

        AppendCore(in rentScope.GetReferenceOfFirstElement(), length);
    }

    private void AppendCore(ref readonly T reference, int length)
    {
        LockableQueue queue = GetFrontQueue();
        if (Core(in queue, in reference, length))
            goto Tail;

        SpinWait spinWait = new SpinWait();
        do
        {
            spinWait.SpinOnce();
            queue = GetFrontQueue();
        } while (!Core(in queue, in reference, length));
        goto Tail;

    Tail:
        SendSignal();

        static bool Core(in LockableQueue queue, ref readonly T reference, int length)
        {
            Lock @lock = queue.Lock;
            if (!@lock.TryEnter())
                return false;
            try
            {
                Queue<T> internalQueue = queue.Queue;
                int i = 0;
                do
                {
                    internalQueue.Enqueue(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, i));
                } while (++i < length);
            }
            finally
            {
                @lock.Exit();
            }
            return true;
        }
    }

    public void Append(IEnumerable<T> items)
    {
        if (items is T[] array)
        {
            Append(array);
            return;
        }

        PooledList<T> list;
        using (IEnumerator<T> enumerator = items.GetEnumerator())
        {
            if (!enumerator.MoveNext())
                return;

            list = new PooledList<T>();
            do
            {
                list.Add(enumerator.Current);
            } while (enumerator.MoveNext());
        }
        try
        {
            Append(list.ToRentScope());
        }
        finally
        {
            list.Dispose();
        }
    }

    public bool TryTake(bool blocking, [MaybeNullWhen(false)] out T? result)
    {
        using (Lock.Scope scope = GetBackQueueOrSwap(blocking, out Queue<T> queue))
        {
            if (!queue.TryDequeue(out result))
                return false;
            if (queue.Count > 0)
                goto NotEmpty;
            goto Tail;
        }

    NotEmpty:
        SendSignal();
        goto Tail;

    Tail:
        return true;
    }

    public int TryTakeFullChunk(scoped ref ArrayPool<T>.RentScope destination, int startIndex, bool blocking)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);

        using Lock.Scope scope = GetBackQueueOrSwap(blocking, out Queue<T> queue);

        int result = queue.Count;
        if (result <= 0)
            return 0;

        destination.Resize(startIndex + result);
        ref T reference = ref UnsafeHelper.AddTypedOffset(ref destination.GetReferenceOfFirstElement(), startIndex);

        int i = 0;
        foreach (T item in queue)
            UnsafeHelper.AddTypedOffset(ref reference, i++) = item;

        DebugHelper.ThrowIf(result != i);

        queue.Clear();
        return result;
    }

    public int TryTakeFullChunk<TCollection>(TCollection destination, bool blocking) where TCollection : ICollection<T>
    {
        using Lock.Scope scope = GetBackQueueOrSwap(blocking, out Queue<T> queue);

        int result = queue.Count;
        if (result <= 0)
            return 0;

#pragma warning disable CS0162
        if (RTCore.IsDebug)
        {
            int i = 0;
            foreach (T item in queue)
            {
                destination.Add(item);
                i++;
            }
            DebugHelper.ThrowIf(result != i);
        }
        else
        {
            foreach (T item in queue)
                destination.Add(item);
        }
#pragma warning restore CS0162

        queue.Clear();
        return result;
    }

    public void SendSignal()
    {
        IntPtr waitingHandle = _waitingHandle;
        if (waitingHandle == IntPtr.Zero)
            return;
        NativeMethods.SetWaitingHandle(waitingHandle);

        GC.KeepAlive(this);
    }

    private LockableQueue Swap()
    {
        lock (_globalLock)
            return SwapCore();
    }

    private LockableQueue GetFrontQueue()
    {
        lock (_globalLock)
            return GetFrontQueueCore();
    }

    private LockableQueue GetBackQueue()
    {
        lock (_globalLock)
            return GetBackQueueCore();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LockableQueue SwapCore()
    {
        LockableQueue result = _frontQueue;
        _frontQueue = _backQueue;
        _backQueue = result;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LockableQueue GetFrontQueueCore() => _frontQueue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LockableQueue GetBackQueueCore() => _backQueue;

    private Lock.Scope GetBackQueueOrSwap(bool blocking, out Queue<T> queue)
    {
        do
        {
            LockableQueue result = GetBackQueue();
            Lock.Scope scope = result.Lock.EnterScope();
            try
            {
                queue = result.Queue;
                if (queue.Count > 0)
                {
                    Lock.Scope resultScope = scope;
                    scope = default;
                    return resultScope;
                }
            }
            finally
            {
                scope.Dispose();
            }

            result = Swap();
            scope = result.Lock.EnterScope();
            try
            {
                queue = result.Queue;
                if (!blocking || queue.Count > 0)
                {
                    Lock.Scope resultScope = scope;
                    scope = default;
                    return resultScope;
                }
            }
            finally
            {
                scope.Dispose();
            }

            IntPtr waitingHandle = _waitingHandle;
            if (waitingHandle == IntPtr.Zero)
            {
                blocking = false;
                continue;
            }
            NativeMethods.WaitForWaitingHandle(waitingHandle);

            GC.KeepAlive(this);
        } while (true);
    }

    ~DoubleBufferedQueue()
    {
        IntPtr waitingHandle = _waitingHandle;
        if (waitingHandle == IntPtr.Zero)
            return;
        NativeMethods.SetWaitingHandle(waitingHandle);
        NativeMethods.DestroyWaitingHandle(waitingHandle);
    }
}