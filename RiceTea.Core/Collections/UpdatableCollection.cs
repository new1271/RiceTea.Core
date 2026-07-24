using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;

namespace RiceTea.Core.Collections;

public static class UpdatableCollection
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UpdatableCollection<T, List<T>> Create<T>()
        => new UpdatableCollection<T, List<T>>(new List<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UpdatableCollection<T, PooledList<T>> CreatePooled<T>()
        => new UpdatableCollection<T, PooledList<T>>(new PooledList<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UpdatableCollection<T, UnwrappableList<T>> CreateUnwrapped<T>()
        => new UpdatableCollection<T, UnwrappableList<T>>(new UnwrappableList<T>());
}

public sealed class UpdatableCollection<T, TList> : ICollection<T>, IDisposable where TList : IList<T>
{
    private readonly TList _list;
    private readonly PooledList<T> _addList, _removeList;

    private ulong _version, _oldVersion;
    private bool _disposed;

    public UpdatableCollection(TList list)
    {
        if (list.IsReadOnly)
            ArgumentException.Throw("the list cannot be readonly!", nameof(list));
        _list = list;
        _addList = new PooledList<T>();
        _removeList = new PooledList<T>();
        _version = 0UL;
        _oldVersion = 0UL;
    }

    public int Count => _list.Count;

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        PooledList<T> list = _addList;
        lock (list)
        {
            list.Add(item);
            Atomics.Increment(ref _version);
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        PooledList<T> list = _addList;
        lock (list)
        {
            list.AddRange(items);
            Atomics.Increment(ref _version);
        }
    }

    public void Clear()
    {
        PooledList<T> list = _removeList;
        lock (list)
        {
            list.AddRange(_list);
            Atomics.Increment(ref _version);
        }
    }

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    public bool Remove(T item)
    {
        if (!_list.Contains(item))
            return false;

        PooledList<T> list = _removeList;
        lock (list)
        {
            list.Add(item);
            Atomics.Increment(ref _version);
        }
        return true;
    }

    public void RemoveAt(int index)
    {
        T item = _list[index];

        PooledList<T> list = _removeList;
        lock (list)
        {
            list.Add(item);
            Atomics.Increment(ref _version);
        }
    }

    public TList Update()
    {
        TList list = _list;

        ulong newVersion = Atomics.Read(ref _version);
        if (_oldVersion == newVersion)
            return list;
        _oldVersion = newVersion;
        PooledList<T> addList = _addList;
        PooledList<T> removeList = _removeList;
        lock (addList)
        {
            list.AddRange(addList);
            addList.Clear();
        }
        lock (removeList)
        {
            list.RemoveAll(removeList);
            removeList.Clear();
        }
        return list;
    }

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        PooledList<T> pooledList = _addList;
        lock (pooledList)
            pooledList.Dispose();
        pooledList = _removeList;
        lock (pooledList)
            pooledList.Dispose();

        TList list = _list;
        list.Clear();
        (list as IDisposable)?.Dispose();

        GC.SuppressFinalize(this);
    }
}
