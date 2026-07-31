using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Native;

public abstract unsafe partial class NativeMemoryPool
{
    protected const uint MinimumMemoryBlockSize = 16;

    private static readonly NativeMemoryPool _shared = CreateSharedPool();

    public static NativeMemoryPool Empty => EmptyImpl.Instance;
    public static NativeMemoryPool Shared => _shared;

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeMemoryBlock Rent() => Rent(MinimumMemoryBlockSize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeMemoryBlock Rent(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity == 0)
            return NativeMemoryBlock.Empty;
        return RentCore(unchecked((nuint)capacity));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeMemoryBlock Rent(long capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity == 0)
            return NativeMemoryBlock.Empty;
        return RentCore(unchecked((nuint)capacity));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeMemoryBlock Rent(nint capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity == 0)
            return NativeMemoryBlock.Empty;
        return RentCore(unchecked((nuint)capacity));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeMemoryBlock Rent(uint capacity)
    {
        if (capacity == 0)
            return NativeMemoryBlock.Empty;
        return RentCore(capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeMemoryBlock Rent(ulong capacity)
    {
        if (capacity == 0)
            return NativeMemoryBlock.Empty;
        return RentCore(unchecked((nuint)capacity));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeMemoryBlock Rent(nuint capacity)
    {
        if (capacity == 0)
            return NativeMemoryBlock.Empty;
        return RentCore(capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NativeMemoryBlock RentCore(nuint capacity) => new NativeMemoryBlock(RentCore(ref capacity), capacity);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypedNativeMemoryBlock<T> Rent<T>() where T : unmanaged
        => Rent<T>(MinimumMemoryBlockSize * UnsafeHelper.SizeOf<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypedNativeMemoryBlock<T> Rent<T>(int capacity) where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity == 0)
            return TypedNativeMemoryBlock<T>.Empty;
        return RentCore<T>(unchecked((nuint)capacity) * UnsafeHelper.SizeOf<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypedNativeMemoryBlock<T> Rent<T>(long capacity) where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity == 0)
            return TypedNativeMemoryBlock<T>.Empty;
        return RentCore<T>(unchecked((nuint)capacity) * UnsafeHelper.SizeOf<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypedNativeMemoryBlock<T> Rent<T>(nint capacity) where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity == 0)
            return TypedNativeMemoryBlock<T>.Empty;
        return RentCore<T>(unchecked((nuint)capacity) * UnsafeHelper.SizeOf<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypedNativeMemoryBlock<T> Rent<T>(uint capacity) where T : unmanaged
    {
        if (capacity == 0)
            return TypedNativeMemoryBlock<T>.Empty;
        return RentCore<T>(capacity * UnsafeHelper.SizeOf<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypedNativeMemoryBlock<T> Rent<T>(ulong capacity) where T : unmanaged
    {
        if (capacity == 0)
            return TypedNativeMemoryBlock<T>.Empty;
        return RentCore<T>(unchecked((nuint)capacity) * UnsafeHelper.SizeOf<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypedNativeMemoryBlock<T> Rent<T>(nuint capacity) where T : unmanaged
    {
        if (capacity == 0)
            return TypedNativeMemoryBlock<T>.Empty;
        return RentCore<T>(capacity * UnsafeHelper.SizeOf<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TypedNativeMemoryBlock<T> RentCore<T>(nuint capacity) where T : unmanaged
    {
        capacity *= UnsafeHelper.SizeOf<T>();
        return new TypedNativeMemoryBlock<T>((T*)RentCore(ref capacity), capacity / UnsafeHelper.SizeOf<T>());
    }

    protected abstract void* RentCore(ref nuint capacity);

    public void Return(in NativeMemoryBlock memoryBlock)
    {
        void* ptr = memoryBlock.NativePointer;
        if (ptr == null)
            return;
        ReturnCore(ptr, memoryBlock.Length);
    }

    public void Return(in NativeMemoryBlock memoryBlock, bool clearContent)
    {
        void* ptr = memoryBlock.NativePointer;
        if (ptr == null)
            return;
        nuint length = memoryBlock.Length;
        if (clearContent)
            UnsafeHelper.InitBlockUnaligned(ptr, 0, length);
        ReturnCore(ptr, length);
    }

    public void Return<T>(in TypedNativeMemoryBlock<T> memoryBlock) where T : unmanaged
    {
        T* ptr = memoryBlock.NativePointer;
        if (ptr == null)
            return;
        ReturnCore(ptr, memoryBlock.Length * UnsafeHelper.SizeOf<T>());
    }

    public void Return<T>(in TypedNativeMemoryBlock<T> memoryBlock, bool clearContent) where T : unmanaged
    {
        T* ptr = memoryBlock.NativePointer;
        if (ptr == null)
            return;
        nuint length = memoryBlock.Length;
        if (clearContent)
            UnsafeHelper.InitBlockUnaligned(ptr, 0, length * UnsafeHelper.SizeOf<T>());
        ReturnCore(ptr, length);
    }

    protected abstract void ReturnCore(void* ptr, nuint length);

    private static partial NativeMemoryPool CreateSharedPool();
}