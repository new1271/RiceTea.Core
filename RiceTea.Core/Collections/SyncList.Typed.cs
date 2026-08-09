using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using RiceTea.Core.Helpers;
using RiceTea.Core.Threading;

namespace RiceTea.Core.Collections;

public class SyncList<T, TList> : IList<T>, ICollection, ILockableEnumerable<T> where TList : IList<T>
{
    private readonly TList _list;
    private readonly Lock _lock;

    public TList Items => _list;

    public SyncList(TList list)
    {
        _list = list;
        _lock = new Lock();
    }

    public T this[int index]
    {
        get
        {
            lock (_lock)
                return _list[index];
        }
        set
        {
            lock (_lock)
                _list[index] = value;
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
                return _list.Count;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            lock (_lock)
                return _list.IsReadOnly;
        }
    }

    bool ICollection.IsSynchronized => true;

    object ICollection.SyncRoot => this;

    public void Add(T item)
    {
        lock (_lock)
            _list.Add(item);
    }

    public void Clear()
    {
        lock (_lock)
            _list.Clear();
    }

    public bool Contains(T item)
    {
        lock (_lock)
            return _list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_lock)
            _list.CopyTo(array, arrayIndex);
    }

    public Lock.Scope EnterLockScope() => _lock.EnterScope();

    public Enumerator GetEnumerator() => new Enumerator(this);

    public int IndexOf(T item)
    {
        lock (_lock)
            return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        lock (_lock)
            _list.Insert(index, item);
    }

    public bool Remove(T item)
    {
        lock (_lock)
            return _list.Remove(item);
    }

    public void RemoveAt(int index)
    {
        lock (_lock)
            _list.RemoveAt(index);
    }

    void ICollection.CopyTo(Array array, int index)
    {
        if (array is not T[] typedArray)
        {
            InvalidOperationException.Throw();
            return;
        }
        CopyTo(typedArray, index);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    public sealed class Enumerator : IEnumerator<T>
    {
        private readonly TList _list;
        private readonly Lock _lock;

        private IEnumerator<T>? _enumerator;

        public Enumerator(SyncList<T, TList> list)
        {
            _list = list._list;
            _lock = list._lock;
        }

        public T Current
        {
            get
            {
                lock (_lock)
                {
                    IEnumerator<T>? enumerator = _enumerator;
                    if (enumerator is null)
                        return InvalidOperationException.Throw<T>();
                    return enumerator.Current;
                }
            }
        }

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            lock (_lock)
            {
                IEnumerator<T>? enumerator = _enumerator;
                if (enumerator is null)
                {
                    enumerator = _list.GetEnumerator();
                    _enumerator = enumerator;
                }
                return enumerator.MoveNext();
            }
        }

        public void Reset()
        {
            lock (_lock)
                DisposeHelper.SwapDispose(ref _enumerator);
        }

        public void Dispose() => Reset();
    }
}
