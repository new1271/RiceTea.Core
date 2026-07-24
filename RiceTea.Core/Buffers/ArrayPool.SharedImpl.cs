#if NET472_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Threading;

namespace RiceTea.Core.Buffers;

partial class ArrayPool<T>
{
    private sealed partial class SharedImpl : ArrayPool<T>
    {
        private const int GenerationEden = 0;
        private const int MaxLocalGeneration = 3;
        private const int GlobalArrayStackPreserveCount = 1;

        private const int LocalBucketCount = 4;
        private const int LocalArraySizeLimit = 1 << (LocalBucketCount + 3);
        private const int GlobalBucketCount = 17;
        private const int GlobalArraySizeLimit = 1 << (GlobalBucketCount + 3);

        [ThreadStatic]
        private static LocalArray[]? _localArrayBuckets;
        [ThreadStatic]
        private static GlobalArrayStack[]? _globalArrayStackBuckets;

        private readonly HashSet<GCHandle> _localArrayCollectSet = new();
        private readonly ProcessorLocal<GlobalArrayStack[]> _globalArrayStacksGroup = new(CreateGlobalArrayStacks);
        private readonly Lock _syncLock = new();

        private GCHandle _trimCallerHandle = GCHandle.Alloc(null, GCHandleType.Weak);
        private ulong _lastTrimTimestamp = 0;

        private static GlobalArrayStack[] CreateGlobalArrayStacks()
        {
            GlobalArrayStack[] stacks = new GlobalArrayStack[GlobalBucketCount];
            ref GlobalArrayStack stackRef = ref UnsafeHelper.GetArrayDataReference(stacks);
            for (int i = 16, j = 0; i <= GlobalArraySizeLimit; i <<= 1, j++)
                UnsafeHelper.AddTypedOffset(ref stackRef, j) = new GlobalArrayStack(i);
            return stacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LocalArray[]? GetLocalArrayBucketsOrNull() => _localArrayBuckets;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LocalArray[] GetOrCreateLocalArrayBuckets()
        {
            LocalArray[]? buckets = _localArrayBuckets;
            if (buckets is null)
            {
                buckets = new LocalArray[LocalBucketCount];
                _localArrayBuckets = buckets;
                lock (_syncLock)
                    _localArrayCollectSet.Add(GCHandle.Alloc(buckets, GCHandleType.Weak));
            }
            return buckets;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GlobalArrayStack GetGlobalArrayStack(int index) => (_globalArrayStackBuckets ??= _globalArrayStacksGroup.Value).AsUnsafeRef()[index];

        protected override T[] RentCore(nuint capacity)
        {
            if (capacity > GlobalArraySizeLimit)
                return new T[capacity];
            int index;
            if (capacity <= 16)
            {
                index = 0;
            }
            else
            {
                capacity >>= 4;
                index = MathHelper.Log2(capacity);
                index += MathHelper.BooleanToInt32(capacity >= (1U << index));
            }

            T[]? array;
            if (index < LocalBucketCount)
                goto Local;
            else
                goto Global;

        Local:
            LocalArray[]? localBuckets = GetLocalArrayBucketsOrNull();
            if (localBuckets is null)
                goto Global;

            ref LocalArray localBucket = ref localBuckets.AsUnsafeRef()[index];
            if (Atomics.Read(ref localBucket.Array) is null)
                goto Global;
            localBucket.EnterBarrier();
            try
            {
                array = Cells.Exchange(ref localBucket.Array, null);
                if (array is null)
                    goto Global;
                Thread.MemoryBarrier();
                return array;
            }
            finally
            {
                localBucket.ExitBarrier();
            }

        Global:
            return GetGlobalArrayStack(index).Pop();
        }

        protected override void ReturnCore(T[] array)
        {
            int length = array.Length;
            if (length < 16 || length > GlobalArraySizeLimit || !MathHelper.IsPow2(length))
                return;
            int index = MathHelper.Log2((uint)length) - 4;
            if (index < LocalBucketCount)
                goto Local;
            else
                goto Global;

        Local:
            ref LocalArray localBucket = ref GetOrCreateLocalArrayBuckets().AsUnsafeRef()[index];
            if (Atomics.Read(ref localBucket.Array) is not null)
                goto Global;
            localBucket.EnterBarrier();
            try
            {
                if (localBucket.Array is not null)
                    goto Global;
                localBucket.Array = array;
                localBucket.Generation = GenerationEden;
                Thread.MemoryBarrier();
            }
            finally
            {
                localBucket.ExitBarrier();
            }
            ref GCHandle trimCallerHandle = ref _trimCallerHandle;
            if (trimCallerHandle.Target is null)
            {
                lock (_syncLock)
                {
                    if (trimCallerHandle.Target is null)
                        trimCallerHandle.Target = LocalArrayTrimCaller.Register(this);
                }
            }
            return;

        Global:
            GetGlobalArrayStack(index).Push(array);
        }

        private void TrimLocals()
        {
            const ulong MinimumTrimPeriod = 10 * TimeSpan.TicksPerSecond;

            Lock syncLock = _syncLock;
            if (!syncLock.TryEnter())
                return;
            try
            {
                ulong now = NativeMethods.GetTicksForSystem();
                if (now - _lastTrimTimestamp < MinimumTrimPeriod)
                    return;
                _lastTrimTimestamp = now;

                _localArrayCollectSet.RemoveWhere(static (GCHandle handle) =>
                {
                    if (handle.Target is not LocalArray[] buckets)
                        return true;

                    ref LocalArray bucketRef = ref UnsafeHelper.GetArrayDataReference(buckets);
                    for (int i = 0; i < LocalBucketCount; i++)
                    {
                        ref LocalArray bucket = ref UnsafeHelper.AddTypedOffset(ref bucketRef, i);
                        if (!bucket.TryEnterBarrier())
                            continue;
                        try
                        {
                            if (bucket.Array is null || ++bucket.Generation < MaxLocalGeneration)
                                continue;
                            bucket.Generation = GenerationEden;
                            bucket.Array = null;
                            Thread.MemoryBarrier();
                        }
                        finally
                        {
                            bucket.ExitBarrier();
                        }
                    }

                    return false;
                });
            }
            finally
            {
                syncLock.Exit();
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private struct LocalArray
        {
            public T[]? Array;
            public nuint Generation;
            private nuint _barrier;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnterBarrier() => Atomics.Exchange(ref _barrier, 1) == 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void EnterBarrier()
            {
                ref nuint barrierRef = ref _barrier;

                while (Atomics.Exchange(ref barrierRef, 1) != 0)
                {
                    SpinWait spin = new SpinWait();
                    while (Atomics.Read(ref barrierRef) != 0)
                        spin.SpinOnce();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ExitBarrier() => Atomics.Write(ref _barrier, 0);
        }

        private sealed class LocalArrayTrimCaller
        {
            private readonly SharedImpl _owner;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private LocalArrayTrimCaller(SharedImpl owner) => _owner = owner;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static LocalArrayTrimCaller Register(SharedImpl owner) => new LocalArrayTrimCaller(owner);

            ~LocalArrayTrimCaller() => _owner.TrimLocals();
        }
    }
}
#endif