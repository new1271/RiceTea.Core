using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;

#pragma warning disable CS8500

namespace RiceTea.Core.Helpers;

public static partial class ArrayHelper
{
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] CreateUninitializedArray<T>(int length)
    {
#if NET472_OR_GREATER
        return new T[length];
#else
        return GC.AllocateUninitializedArray<T>(length);
#endif
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([InlineParameter] T?[] array) => array is null || array.Length == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasNullItem<T>(T[] array) where T : class
    {
        fixed (T* ptr = array)
            return SequenceHelper.Contains((nint*)ptr, MathHelper.MakeUnsigned(array.Length), 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasNullItem<T>(T[] array, int startIndex, int count) where T : class
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (startIndex + count > array.Length)
            ArgumentOutOfRangeException.Throw(startIndex >= array.Length ? nameof(startIndex) : nameof(count));
        fixed (T* ptr = array)
            return SequenceHelper.Contains((nint*)ptr + startIndex, unchecked((nuint)count), 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasNonNullItem<T>(T?[] array) where T : class
    {
        fixed (T* ptr = array)
            return SequenceHelper.ContainsExclude((nint*)ptr, MathHelper.MakeUnsigned(array.Length), 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasNonNullItem<T>(T?[] array, int startIndex, int count) where T : class
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (startIndex + count > array.Length)
            ArgumentOutOfRangeException.Throw(startIndex >= array.Length ? nameof(startIndex) : nameof(count));
        fixed (T* ptr = array)
            return SequenceHelper.ContainsExclude((nint*)ptr + startIndex, unchecked((nuint)count), 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T? FindFirstNullItem<T>(T?[] array) where T : class
    {
        fixed (T* ptr = array)
        {
            int index = SequenceHelper.IndexOf((nint*)ptr, array.Length, 0);
            return index == -1 ? null : array[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T? FindFirstNullItem<T>(T?[] array, int startIndex, int count) where T : class
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (startIndex + count > array.Length)
            ArgumentOutOfRangeException.Throw(startIndex >= array.Length ? nameof(startIndex) : nameof(count));
        fixed (T* ptr = array)
        {
            int index = SequenceHelper.IndexOf((nint*)ptr + startIndex, count, 0);
            return index == -1 ? null : array[startIndex + index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T? FindFirstNonNullItem<T>(T?[] array) where T : class
    {
        fixed (T* ptr = array)
        {
            int index = SequenceHelper.IndexOfExclude((nint*)ptr, array.Length, 0);
            return index == -1 ? null : array[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T? FindFirstNonNullItem<T>(T?[] array, int startIndex, int count) where T : class
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (startIndex + count > array.Length)
            ArgumentOutOfRangeException.Throw(startIndex >= array.Length ? nameof(startIndex) : nameof(count));
        fixed (T* ptr = array)
        {
            int index = SequenceHelper.IndexOfExclude((nint*)ptr + startIndex, count, 0);
            return index == -1 ? null : array[startIndex + index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] CopyItemsToArray<T, TEnumerable>(TEnumerable elements) where T : class? where TEnumerable : IEnumerable<T>
    {
        using ArrayPool<T>.RentScope scope = ArrayPool<T>.Shared.EnterRentScopeAndCapture(elements);
        int count = scope.Count;
        if (count <= 0)
            return Array.Empty<T>();
        T[] result = new T[count];
        CopyItemsToArrayCore(ref UnsafeHelper.GetArrayDataReference(result), in scope.GetReferenceOfFirstElement(), (nuint)scope.Count);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] CopyItemsToArrayUnsafe<T>(ref readonly T elementsRef, int count)
    {
        if (count <= 0)
            return Array.Empty<T>();
        T[] result = new T[count];
        CopyItemsToArrayCore(ref UnsafeHelper.GetArrayDataReference(result), in elementsRef, MathHelper.MakeUnsigned(count));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CopyItemsToArrayCore<T>(ref T destination, ref readonly T sourceArrayRef, nuint length)
    {
        int i;
        for (i = 0; length >= 4; length -= 4, i += 4)
        {
            SetValueWithOffset(ref destination, in sourceArrayRef, i);
            SetValueWithOffset(ref destination, in sourceArrayRef, i + 1);
            SetValueWithOffset(ref destination, in sourceArrayRef, i + 2);
            SetValueWithOffset(ref destination, in sourceArrayRef, i + 3);
        }
        switch (length)
        {
            case 3:
                SetValueWithOffset(ref destination, in sourceArrayRef, i + 2);
                goto case 2;
            case 2:
                SetValueWithOffset(ref destination, in sourceArrayRef, i + 1);
                goto case 1;
            case 1:
                SetValueWithOffset(ref destination, in sourceArrayRef, i);
                break;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SetValueWithOffset(ref T destination, ref readonly T sourceArrayRef, int offset)
            => UnsafeHelper.AddTypedOffset(ref destination, offset) = UnsafeHelper.AddTypedOffsetAsReadOnly(in sourceArrayRef, offset);
    }
}
