using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using InlineMethod;

using RiceTea.Core.Extensions;

namespace RiceTea.Core.Native;

internal sealed class DelayedCollector
{
    private const ulong DelayedCollectingPeriod = 5000 * TimeSpan.TicksPerMillisecond;
    private const ulong DelayedCollectingNoRefTime = DelayedCollectingPeriod / 2;

    private static readonly DelayedCollector _instance = new DelayedCollector();

    private static ulong _nowTime;

    private readonly Thread _thread;
    private readonly Queue<DelayedCollectingObject> _queue;
    private readonly HashSet<DelayedCollectingObject> _innerSet;
    private readonly Lock _lock;
    private readonly IntPtr _waitingEventHandle;

    public static DelayedCollector Instance => _instance;

    private DelayedCollector()
    {
        _lock = new Lock();
        _innerSet = new HashSet<DelayedCollectingObject>();
        _queue = new Queue<DelayedCollectingObject>();
        _waitingEventHandle = NativeMethods.CreateWaitingHandle(autoReset: true);
        _thread = new Thread(DoTick)
        {
            IsBackground = true,
            Name = nameof(DelayedCollector) + " Thread",
            Priority = ThreadPriority.Lowest
        };
        _thread.Start();
    }

    public void AddObject(DelayedCollectingObject obj)
    {
        lock (_lock)
            _queue.Enqueue(obj);
        NativeMethods.SetWaitingHandle(_waitingEventHandle);
    }

    private void DoTick()
    {
        HashSet<DelayedCollectingObject> innerSet = _innerSet;
        Queue<DelayedCollectingObject> queue = _queue;
        Lock @lock = _lock;

        IntPtr waitingEventHandle = _waitingEventHandle;
        NativeMethods.WaitForWaitingHandle(waitingEventHandle);
        while (true)
        {
            ulong predictedTicks = NativeMethods.GetTicksForSystem() + DelayedCollectingPeriod;

            lock (@lock)
            {
                while (queue.TryDequeue(out DelayedCollectingObject? obj))
                    innerSet.Add(obj!);
            }

            DoLifeCheck(innerSet);

            if (innerSet.Count <= 0)
            {
                Debug.WriteLine($"[{nameof(DelayedCollector)}] No object needs collecting, sleeping...");
                NativeMethods.WaitForWaitingHandle(waitingEventHandle);
                continue;
            }

            if (!NativeMethods.SleepInAbsoluteTicks(predictedTicks))
                Thread.Yield();
        }
    }

    [Inline(InlineBehavior.Remove)]
    private static void DoLifeCheck(HashSet<DelayedCollectingObject> list)
    {
        if (list.Count <= 0)
            return;
        _nowTime = NativeMethods.GetTicksForSystem();
#if DEBUG
        int count =
#endif
        list.RemoveWhere(static obj =>
        {
            if (obj.IsDisposed)
                return true;
            if (obj.IsInReference)
                return false;
            ulong nowTime = _nowTime;
            ulong deRefTime = obj.LastDereferenceTime;
            if (nowTime > deRefTime && (nowTime - deRefTime) > DelayedCollectingNoRefTime)
            {
                obj.RemoveObject();
                return true;
            }
            return false;
        });
#if DEBUG
        Debug.WriteLineIf(count > 0, $"[{nameof(DelayedCollector)}] Removed {count} object(s)");
#endif
    }
}
