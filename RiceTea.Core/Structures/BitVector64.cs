using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RiceTea.Core.Structures;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct BitVector64 : IComparable<BitVector64>, IEquatable<BitVector64>
{
    private ulong _data;

    public BitVector64(ulong data) => _data = data;

    public bool this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            if (index < 0 || index > 63)
                throw new IndexOutOfRangeException();
            ulong mask = 1UL << index;
            return (_data & mask) == mask;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (index < 0 || index > 63)
                throw new IndexOutOfRangeException();
            ulong mask = 1UL << index;
            if (value)
                _data |= mask;
            else
                _data &= ~mask;
        }
    }

    public bool this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            if (index > 63)
                throw new IndexOutOfRangeException();
            ulong mask = 1UL << (int)index;
            return (_data & mask) == mask;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (index > 63)
                throw new IndexOutOfRangeException();
            ulong mask = 1UL << (int)index;
            if (value)
                _data |= mask;
            else
                _data &= ~mask;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() => _data = ulong.MinValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set() => _data = ulong.MaxValue;

    public bool InterlockedGet(int index)
    {
        if (index < 0 || index > 63)
            throw new IndexOutOfRangeException();
        ulong mask = 1UL << index;
        return (Atomics.Read(ref _data) & mask) == mask;
    }

    public bool InterlockedGet(uint index)
    {
        if (index > 63)
            throw new IndexOutOfRangeException();
        ulong mask = 1UL << (int)index;
        return (Atomics.Read(ref _data) & mask) == mask;
    }

    public bool InterlockedSet(int index, bool value)
    {
        if (index < 0 || index > 63)
            throw new IndexOutOfRangeException();
        ulong mask = 1UL << index;
        if (value)
            return (Atomics.Or(ref _data, mask) & mask) == mask;
        else
            return (Atomics.And(ref _data, ~mask) & mask) == mask;
    }

    public bool InterlockedSet(uint index, bool value)
    {
        if (index > 63)
            throw new IndexOutOfRangeException();
        ulong mask = 1UL << (int)index;
        if (value)
            return (Atomics.Or(ref _data, mask) & mask) == mask;
        else
            return (Atomics.And(ref _data, ~mask) & mask) == mask;
    }

    public void InterlockedReset()
        => Atomics.Write(ref _data, ulong.MinValue);

    public void InterlockedSet()
        => Atomics.Write(ref _data, ulong.MaxValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Exchange(ulong value)
    {
        ulong result = _data;
        _data = value;
        return result;
    }

    public ulong InterlockedExchange(ulong value)
        => Atomics.Exchange(ref _data, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ulong(BitVector64 bitVector) => bitVector._data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BitVector64(ulong value) => new BitVector64 { _data = value };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BitVector64 a, BitVector64 b) => a._data == b._data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BitVector64 a, BitVector64 b) => a._data != b._data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitVector64 operator ~(BitVector64 a) => new BitVector64(~a._data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitVector64 operator &(BitVector64 a, BitVector64 b) => new BitVector64(a._data & b._data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitVector64 operator |(BitVector64 a, BitVector64 b) => new BitVector64(a._data | b._data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitVector64 operator ^(BitVector64 a, BitVector64 b) => new BitVector64(a._data ^ b._data);

    public readonly override bool Equals(object? obj) => obj is BitVector64 other && Equals(other);

    public readonly bool Equals(BitVector64 vector) => _data == vector._data;

    public readonly int CompareTo(BitVector64 other) => _data.CompareTo(other._data);

    public readonly override int GetHashCode() => _data.GetHashCode();

    public readonly override string ToString() => $"0x{_data:X8}";
}
