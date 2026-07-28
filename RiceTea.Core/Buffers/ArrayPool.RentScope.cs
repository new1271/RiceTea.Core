using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Collections;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

namespace RiceTea.Core.Buffers;

partial class ArrayPool<T>
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct RentScope : IList<T>, IDisposable
    {
        private ArrayPool<T>? _pool;
        private T[]? _array;
        private int _count;

        public RentScope(ArrayPool<T> pool, T[] array, int count)
        {
            _pool = pool;
            _array = array;
            _count = count;
        }

        public readonly T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= _count)
                    return IndexOutOfRangeException.Throw<T>();

                T[]? array = _array;
                DebugHelper.ThrowIf(array is null);
                return array.AsUnsafeRef()[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= _count)
                    IndexOutOfRangeException.Throw();

                T[]? array = _array;
                DebugHelper.ThrowIf(array is null);
                array.AsUnsafeRef()[index] = value;
            }
        }

        public readonly int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int count = _count;
                if (count <= 0)
                    return 0;
                T[]? array = _array;
                DebugHelper.ThrowIf(array is null);
                return array.Length;
            }
        }

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        readonly bool ICollection<T>.IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly T GetReferenceOfFirstElement()
            => ref UnsafeHelper.GetArrayDataReference(NullSafetyHelper.ThrowIfNull(_array));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear()
        {
            int count = _count;
            if (count <= 0)
                return;
            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);
            Array.Clear(array, 0, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(T item)
        {
            int count = _count;
            if (count <= 0)
                return false;
            int index = IndexOfCore(item, count);
            return index >= 0 && index < _count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] source, int startIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
            CopyFromCore(source, startIndex, source.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] source, int startIndex, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
            CopyFromCore(source, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CopyFromCore(T[] source, int startIndex, int count)
        {
            Resize(startIndex + count, moveArray: startIndex != 0);
            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);
            Array.Copy(source, 0, array, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom<TCollection>(TCollection source, int startIndex, int count) where TCollection : ICollection<T>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
            Resize(startIndex + count, moveArray: startIndex != 0);
            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);
            source.CopyTo(array, startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(T[] destination, int arrayIndex)
        {
            int count = _count;
            if (count <= 0)
                return;
            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);
            Array.Copy(array, 0, destination, arrayIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator()
        {
            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);
            return new Enumerator(array, _count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T item)
        {
            int count = _count;
            if (count <= 0)
                return -1;
            return IndexOfCore(item, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newCount, bool moveArray = true)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(newCount);

            int count = _count;
            if (newCount == count)
                return;

            ArrayPool<T>? pool = _pool;
            if (pool is null)
            {
                ObjectDisposedException.Throw(null);
                return;
            }

            T[]? array = _array;
            if (count > newCount)
            {
                DebugHelper.ThrowIf(array is null);
                if (!_isUnmanagedType)
                    Array.Clear(array, newCount, array.Length - newCount);
                return;
            }
            DebugHelper.ThrowIf(count == newCount, "Unexpectable!");

            int capacity;
            if (array is null || (capacity = array.Length) <= 0)
            {
                array = pool.Rent(newCount);
                DebugHelper.ThrowIf(array.Length < newCount);
                _array = array;
                _count = newCount;
            }
            else if (capacity >= newCount)
                _count = newCount;
            else
            {
                T[] newArray = pool.Rent(newCount);
                DebugHelper.ThrowIf(newArray.Length < newCount);
                try
                {
                    if (moveArray)
                        Array.Copy(array, 0, newArray, 0, count);
                }
                finally
                {
                    pool.Return(array);
                    _array = newArray;
                    _count = newCount;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T[] ToArray()
        {
            int count = _count;
            if (count <= 0)
                return Array.Empty<T>();

            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);

            T[] result = ArrayHelper.CreateUninitializedArray<T>(count);
            Array.Copy(array, 0, result, 0, count);
            return result;
        }

        readonly void IList<T>.Insert(int index, T item)
            => NotSupportedException.Throw();

        readonly void IList<T>.RemoveAt(int index)
            => NotSupportedException.Throw();

        readonly void ICollection<T>.Add(T item)
            => NotSupportedException.Throw();

        readonly void ICollection<T>.Clear()
            => NotSupportedException.Throw();

        readonly bool ICollection<T>.Remove(T item)
            => NotSupportedException.Throw<bool>();

        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            int count = _count;
            if (count <= 0)
                return EnumeratorHelper.CreateEmptyEnumerator<T>();
            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);
            return new LimitedArrayEnumerator<T>(array, count);
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            int count = _count;
            if (count <= 0)
                return EnumeratorHelper.CreateEmptyEnumerator<T>();
            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);
            return new LimitedArrayEnumerator<T>(array, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int IndexOfCore(T item, int count)
        {
            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);
            if (_isUnmanagedType)
                return SequenceHelper.IndexOf(array, item, 0, count);
            else
                return Array.IndexOf(array, item, 0, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PooledList<T> ToPooledList()
        {
            ArrayPool<T> pool = _pool ?? ObjectDisposedException.Throw<ArrayPool<T>>(nameof(RentScope));
            Deconstruct(out T[] array, out int count);
            return new PooledList<T>(pool, array, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out ArrayPool<T> pool, out T[] array, out int count)
        {
            pool = _pool ?? ObjectDisposedException.Throw<ArrayPool<T>>(nameof(RentScope));
            Deconstruct(out array, out count);
        }

        public void Deconstruct(out T[] array, out int count)
        {
            ArrayPool<T>? pool = _pool;
            if (pool is null)
            {
                array = Array.Empty<T>();
                count = 0;
                return;
            }

            T[]? selfArray = _array;
            DebugHelper.ThrowIf(selfArray is null);

            _pool = null;
            (_array, array) = (null, selfArray);
            (_count, count) = (0, _count);
        }

        public void Dispose()
        {
            ArrayPool<T>? pool = _pool;
            if (pool is null)
                return;

            T[]? array = _array;
            DebugHelper.ThrowIf(array is null);
            pool.Return(array);

            _pool = null;
            _array = null;
            _count = 0;
        }

        public ref struct Enumerator : IEnumerator<T>
        {
            private readonly T[] _array;
            private int _count, _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(T[] array, int count)
            {
                _array = array;
                _count = count;
                _index = -1;
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    int index = _index;
                    if (index < 0 || index >= _count)
                        return InvalidOperationException.Throw<T>();
                    return _array.AsUnsafeRef()[index];
                }
            }

            readonly object? IEnumerator.Current => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _index = -1;
                _count = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int index = _index + 1;
                int count = _count;
                if (index < count)
                {
                    _index = index;
                    return index >= 0;
                }
                return false;
            }

            public void Reset() => _index = -1;
        }
    }
}
