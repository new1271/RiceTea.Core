using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

#pragma warning disable CS8500

namespace RiceTea.Core;

public static class Cells
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>([NotNullIfNotNull(nameof(location2))] ref T? location1, [NotNullIfNotNull(nameof(location1))] ref T? location2)
        => (location1, location2) = (location2, location1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Swap<T>(ref T* location1, ref T* location2)
    {
        T* temp = location1;
        location1 = location2;
        location2 = temp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(location))]
    public static T? Exchange<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value)
    {
        T? result = location;
        location = value;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void* Exchange(ref void* location, void* value)
    {
        void* result = location;
        location = value;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* Exchange<T>(ref T* location, T* value)
    {
        T* result = location;
        location = value;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(location))]
    public static T? CompareExchange<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value, T? comparand)
        => UnsafeHelper.IsPrimitiveType<T>() ? 
        CompareExchangeFast(ref location, value, comparand) :
        CompareExchangeSlow(ref location, value, comparand, EqualityComparer<T>.Default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void* CompareExchange(ref void* location, void* value, void* comparand)
    {
        void* oldValue = location;
#if NET8_0_OR_GREATER
        location = (oldValue == comparand) ? value : oldValue; // 強制 JIT 生成 cmp + cmovne
#else
        nuint mask = UnsafeHelper.Negate(MathHelper.BooleanToNativeUnsigned(location == comparand));
        location = (void*)((nuint)oldValue & ~mask | (nuint)value & mask);
#endif
        return oldValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* CompareExchange<T>(ref T* location, T* value, T* comparand)
    {
        T* oldValue = location;
#if NET8_0_OR_GREATER
        location = (oldValue == comparand) ? value : oldValue; // 強制 JIT 生成 cmp + cmovne
#else
        nuint mask = UnsafeHelper.Negate(MathHelper.BooleanToNativeUnsigned(location == comparand));
        location = (T*)((nuint)oldValue & ~mask | (nuint)value & mask);
#endif
        return oldValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(location))]
    public static T? CompareExchange<T, TComparer>([NotNullIfNotNull(nameof(value))] ref T? location, T? value, T? comparand, TComparer comparer) 
        where TComparer : IEqualityComparer<T>
    {
        if (typeof(TComparer) == typeof(EqualityComparer<T>) && 
            UnsafeHelper.IsPrimitiveType<T>() &&
            ReferenceEquals(comparer, EqualityComparer<T>.Default))
            return CompareExchangeFast(ref location, value, comparand);

        return CompareExchangeSlow(ref location, value, comparand, comparer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(location))]
    private static T? CompareExchangeFast<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value, T? comparand)
    {
        T? oldValue = location;
#if NET8_0_OR_GREATER
        location = UnsafeHelper.Equals(location, comparand) ? value : oldValue; // 強制 JIT 生成 cmp + cmovne
#else
        T mask = UnsafeHelper.Negate(UnsafeHelper.As<bool, T>(UnsafeHelper.Equals(location, comparand)));
        location = UnsafeHelper.Or(UnsafeHelper.And(oldValue, UnsafeHelper.Not(mask)), UnsafeHelper.And(value, mask));
#endif
        return oldValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(location))]
    private static T? CompareExchangeSlow<T, TComparer>([NotNullIfNotNull(nameof(value))] ref T? location, T? value, T? comparand, TComparer comparer)
        where TComparer : IEqualityComparer<T>
    {
        T? oldValue = location;
#if NET8_0_OR_GREATER
        bool shouldChange;
        if (oldValue is null)
            shouldChange = comparand is null;
        else
            shouldChange = comparand is not null && comparer.Equals(oldValue, comparand);
        location = shouldChange ? value : oldValue;
#else
        if (oldValue is null)
        {
            if (comparand is null)
                goto Change;
        }
        else
        {
            if (comparand is not null && comparer.Equals(oldValue, comparand))
                goto Change;
        }

        goto Return;

    Change:
        location = value;
        
    Return:
#endif
        return oldValue;
    }
}
