using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Collections;

public sealed class UnwrappableList<T> : CustomListBase<T>
{
    public UnwrappableList(int capacity = 4) : base(CreateArray(capacity), initialCount: 0) { }

    public UnwrappableList(IEnumerable<T> items) : base(CreateArrayFromItems(items)) { }

    public UnwrappableList(T[] items) : base(CreateArrayFromItems(items)) { }

    [Inline(InlineBehavior.Remove)]
    private static T[] CreateArray(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity == 0)
            return Array.Empty<T>();
        return new T[capacity];
    }

    [Inline(InlineBehavior.Remove)]
    private static T[] CreateArrayFromItems(IEnumerable<T> items)
        => items switch
        {
            T[] _array => CreateArrayFromItems(_array),
            UnwrappableList<T> _list => CreateArrayFromItems(_list.Unwrap()),
            ICollection<T> _items => CreateArrayFromItems(_items),
            _ => CreateArrayFromItemsCore(items),
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T[] CreateArrayFromItemsCore(IEnumerable<T> items)
    {
        int count = items.Count();
        if (count < 0)
            return Array.Empty<T>();
        T[] result = new T[count];
        IEnumerator<T> enumerator = items.GetEnumerator();
        for (int i = 0; i < count && enumerator.MoveNext(); i++)
            result[i] = enumerator.Current;
        enumerator.Dispose();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T[] CreateArrayFromItems(ICollection<T> items)
    {
        int count = items.Count;
        if (count < 0)
            return Array.Empty<T>();
        T[] result = new T[count];
        items.CopyTo(result, 0);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T[] CreateArrayFromItems(T[] items)
    {
        int length = items.Length;
        if (length < 0)
            return Array.Empty<T>();
        T[] result = new T[length];
        Array.Copy(items, result, length);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                OutOfMemoryException.Throw();
            newCapacity = Limits.MaxArrayLength;
        }
        else
            newCapacity = MathHelper.Max(capacity * 2, capacityAtLeast);

        T[] newArray = new T[newCapacity];
        Array.Copy(array, newArray, capacity);
        _array = newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] Unwrap() => _array;
}
