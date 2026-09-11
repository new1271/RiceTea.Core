using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Structures;

[SkipLocalsInit]
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct BitVector256 : IComparable<BitVector256>, IEquatable<BitVector256>
{
    private const uint SizeInBytes = 256u;
    private const int ArrayCount = unchecked((int)(SizeInBytes / 8 / sizeof(ulong)));

    private fixed ulong _data[ArrayCount];

    public bool this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            if (index < 0 || index > 255)
                throw new IndexOutOfRangeException();
            ulong section = _data[(index & 255) >> 6];
            ulong mask = 1UL << (index & 63);
            return (section & mask) == mask;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (index < 0 || index > 255)
                throw new IndexOutOfRangeException();
            ref ulong section = ref _data[(index & 255) >> 6];
            ulong mask = 1UL << (index & 63);
            if (value)
                section |= mask;
            else
                section &= ~mask;
        }
    }

    public bool this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            if (index > 255)
                throw new IndexOutOfRangeException();
            ulong section = _data[(index & 255) >> 6];
            ulong mask = 1UL << (int)(index & 63);
            return (section & mask) == mask;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (index > 255)
                throw new IndexOutOfRangeException();
            ref ulong section = ref _data[(index & 255) >> 6];
            ulong mask = 1UL << (int)(index & 63);
            if (value)
                section |= mask;
            else
                section &= ~mask;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        fixed (ulong* data = _data)
            UnsafeHelper.InitBlock(data, byte.MinValue, SizeInBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set()
    {
        fixed (ulong* data = _data)
            UnsafeHelper.InitBlock(data, byte.MaxValue, SizeInBytes);
    }

    public readonly override bool Equals(object? obj) => obj is BitVector256 other && Equals(in other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(in BitVector256 vector) => EqualsCore(this, vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(BitVector256 vector) => EqualsCore(this, vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsCore(in BitVector256 vector, in BitVector256 vector2)
    {
        const int V256ByteCount = 256 / 8;
        const int V128ByteCount = 128 / 8;

        fixed (BitVector256* ptr = &vector, ptr2 = &vector2)
        {
#if NET8_0_OR_GREATER
            if (Limits.UseVector256())
                return System.Runtime.Intrinsics.Vector256.Load((byte*)ptr) == System.Runtime.Intrinsics.Vector256.Load((byte*)ptr2);
            if (Limits.UseVector128())
                return (System.Runtime.Intrinsics.Vector128.Load((byte*)ptr) == System.Runtime.Intrinsics.Vector128.Load((byte*)ptr2)) &&
                    (System.Runtime.Intrinsics.Vector128.Load((byte*)ptr + V128ByteCount) == System.Runtime.Intrinsics.Vector128.Load((byte*)ptr2 + V128ByteCount));
#else
            if (Limits.UseVector())
            {
                switch (System.Numerics.Vector<byte>.Count)
                {
                    case V256ByteCount:
                        return _V256((byte*)ptr, (byte*)ptr2);
                    case V128ByteCount:
                        return _V128((byte*)ptr, (byte*)ptr2);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool _V256(byte* ptr, byte* ptr2)
                    => UnsafeHelper.Read<System.Numerics.Vector<byte>>(ptr) == UnsafeHelper.Read<System.Numerics.Vector<byte>>(ptr2);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool _V128(byte* ptr, byte* ptr2)
                    => (UnsafeHelper.Read<System.Numerics.Vector<byte>>(ptr) == UnsafeHelper.Read<System.Numerics.Vector<byte>>(ptr2)) &&
                    (UnsafeHelper.Read<System.Numerics.Vector<byte>>(ptr + V128ByteCount) == UnsafeHelper.Read<System.Numerics.Vector<byte>>(ptr2 + V128ByteCount));
            }
#endif      

            return SequenceHelper.Equals(ptr, ptr2, V256ByteCount);
        }
    }

    public readonly int CompareTo(BitVector256 other)
    {
        for (int i = 0; i < ArrayCount; i++)
        {
            int result = _data[i].CompareTo(other._data[i]);
            if (result != 0)
                return result;
        }
        return 0;
    }

    public readonly override int GetHashCode() => (_data[0] ^ _data[1] ^ _data[2] ^ _data[3]).GetHashCode();

    public readonly override string ToString() => $"0x{_data[0]:X}{_data[1]:X}{_data[2]:X}{_data[3]:X}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SecuritySafeCritical]
    public static bool operator ==(in BitVector256 a, in BitVector256 b) => a.Equals(in b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SecuritySafeCritical]
    public static bool operator !=(in BitVector256 a, in BitVector256 b) => !a.Equals(in b);

    public static BitVector256 operator ~(in BitVector256 a)
    {
        BitVector256 result = new BitVector256();
        for (int i = 0; i < ArrayCount; i++)
            result._data[i] = ~a._data[i];
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SecuritySafeCritical]
    public static BitVector256 operator &(BitVector256 a, BitVector256 b)
    {
        BitVector256 result = new BitVector256();
        ulong* iterator = a._data;
        ulong* iterator2 = b._data;
        ulong* iterator3 = result._data;
        for (int i = 0; i < ArrayCount; i++, iterator++, iterator2++, iterator3++)
        {
            *iterator3 = *iterator & *iterator2;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SecuritySafeCritical]
    public static BitVector256 operator |(BitVector256 a, BitVector256 b)
    {
        BitVector256 result = new BitVector256();
        ulong* iterator = a._data;
        ulong* iterator2 = b._data;
        ulong* iterator3 = result._data;
        for (int i = 0; i < ArrayCount; i++, iterator++, iterator2++, iterator3++)
        {
            *iterator3 = *iterator | *iterator2;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SecuritySafeCritical]
    public static BitVector256 operator ^(BitVector256 a, BitVector256 b)
    {
        BitVector256 result = new BitVector256();
        ulong* iterator = a._data;
        ulong* iterator2 = b._data;
        ulong* iterator3 = result._data;
        for (int i = 0; i < ArrayCount; i++, iterator++, iterator2++, iterator3++)
        {
            *iterator3 = *iterator ^ *iterator2;
        }
        return result;
    }
}
