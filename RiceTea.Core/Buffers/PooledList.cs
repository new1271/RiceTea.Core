using System;
using System.Runtime.CompilerServices;

using RiceTea.Core.Collections;
using RiceTea.Core.Helpers;

namespace RiceTea.Core.Buffers;

public sealed class PooledList<T> : CustomListBase<T>, IDisposable
{
    private readonly ArrayPool<T> _pool;

    private bool _disposed;

    public bool IsDisposed => _disposed;

    public PooledList() : this(ArrayPool<T>.Shared, capacity: 16) { }

    public PooledList(int capacity) : this(ArrayPool<T>.Shared, capacity) { }

    public PooledList(ArrayPool<T> pool) : this(pool, capacity: 16) { }

    public PooledList(ArrayPool<T> pool, int capacity) : this(pool, pool.Rent(capacity), initialCount: 0) { }

    internal PooledList(ArrayPool<T> pool, T[] array, int initialCount) : base(array, initialCount)
    {
        _pool = pool;
    }

    public override void EnsureCapacity(int capacityAtLeast)
    {
        T[] array = _array;
        int capacity = array.Length;
        if (capacity >= capacityAtLeast)
            return;

        int newCapacity;
        if (capacity >= Limits.MaxArrayLength / 2)
        {
            if (capacity >= Limits.MaxArrayLength)
                throw new OutOfMemoryException();
            newCapacity = Limits.MaxArrayLength;
        }
        else
            newCapacity = MathHelper.Max(capacity * 2, capacityAtLeast);

        ArrayPool<T> pool = _pool;
        T[] newArray = pool.Rent(newCapacity);
        Array.Copy(array, newArray, capacity);
        _array = newArray;
        pool.Return(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArrayPool<T>.RentScope ToRentScope()
    {
        Deconstruct(out T[] array, out int count);
        return new ArrayPool<T>.RentScope(_pool, array, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out ArrayPool<T> pool, out T[] array, out int count)
    {
        pool = _pool;
        Deconstruct(out array, out count);
    }

    public void Deconstruct(out T[] array, out int count)
    {
        try
        {
            if (_disposed)
            {
                array = _array;
                count = 0;
                return;
            }
            _disposed = true;

            (array, _array) = (_array, Array.Empty<T>());
            (_count, count) = (0, _count);
            return;
        }
        finally
        {
#pragma warning disable CA1816
            GC.SuppressFinalize(this);
#pragma warning restore CA1816
        }
    }

    internal T[] GetBuffer()
    {
        if (_disposed)
            return Array.Empty<T>();

        return _array;
    }

    ~PooledList() => DisposeCore(disposing: false);

    public void Dispose()
    {
        DisposeCore(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void DisposeCore(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;

        T[] array = _array;
        if (disposing)
        {
            _array = Array.Empty<T>();
            _count = 0;
        }
        _pool.Return(array);
    }
}
