using System;

using RiceTea.Core.Collections;
using RiceTea.Core.Helpers;

namespace RiceTea.Core.Buffers;

public sealed class PooledBitList : BitListBase, IDisposable
{
    private readonly ArrayPool<nuint> _pool;
    private bool _disposed;

    public PooledBitList() : this(ArrayPool<nuint>.Shared) { }

    public PooledBitList(int capacity) : this(ArrayPool<nuint>.Shared, capacity) { }

    public PooledBitList(ArrayPool<nuint> pool) : base(pool.Rent())
    {
        _pool = pool;
        _disposed = false;
    }

    public PooledBitList(ArrayPool<nuint> pool, int capacity) : base(pool.Rent(GetRequiredSectionCount(capacity)))
    {
        _pool = pool;
        _disposed = false;
    }

    protected override void EnsureCapacity()
    {
        nuint[] array = _array;
        int capacity = _array.Length;
        int count = _count;
        if (count <= capacity * SectionSize)
            return;

        int newCapacity;
        if (capacity >= Limits.MaxArrayLength / 2)
        {
            if (capacity >= Limits.MaxArrayLength)
                OutOfMemoryException.Throw();
            newCapacity = Limits.MaxArrayLength;
        }
        else
            newCapacity = MathHelper.Max(capacity * 2, MathHelper.CeilDiv(count, SectionSize));

        ArrayPool<nuint> pool = _pool;
        nuint[] newArray = pool.Rent(newCapacity);
        Array.Copy(array, newArray, capacity);
        pool.Return(array);
        _array = newArray;
    }

    ~PooledBitList() => DisposeCore();

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    private void DisposeCore()
    {
        if (_disposed)
            return;
        _disposed = true;

        nuint[] array = _array;
        _array = Array.Empty<nuint>();
        _pool.Return(array);
    }
}
