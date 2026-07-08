using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RiceTea.Core.Helpers;

#pragma warning disable CS8824

public static class DisposeHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisposeAll<T>(T?[]? array) where T : IDisposable
    {
        if (array is null)
            return;
        DisposeAllUnsafe(ref UnsafeHelper.GetArrayDataReference(array), MathHelper.MakeUnsigned(array.Length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisposeAllWeak<T>(T?[]? array)
    {
        if (array is null)
            return;
        DisposeAllWeakUnsafe(ref UnsafeHelper.GetArrayDataReference(array), MathHelper.MakeUnsigned(array.Length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisposeAllUnsafe<T>(ref readonly T? arrayReference, nint length) where T : IDisposable
    {
        if (UnsafeHelper.IsNullRef(in arrayReference) || length <= 0)
            return;
        DisposeAllUnsafe(in arrayReference, MathHelper.MakeUnsigned(length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisposeAllUnsafe<T>(ref readonly T? arrayReference, nuint length) where T : IDisposable
    {
        if (UnsafeHelper.IsNullRef(in arrayReference) || length == 0)
            return;
        nuint limit = length;
        for (nuint i = 0; limit >= 4; limit -= 4, i += 4)
        {
            Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, i));
            Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, i + 1));
            Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, i + 2));
            Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, i + 3));
        }
        switch (limit)
        {
            case 3:
                Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, length - 3));
                goto case 2;
            case 2:
                Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, length - 2));
                goto case 1;
            case 1:
                Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, length - 1));
                break;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Dispose(T? disposable) => disposable?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisposeAllWeakUnsafe<T>(ref readonly T? arrayReference, nint length)
    {
        if (UnsafeHelper.IsNullRef(in arrayReference) || length <= 0)
            return;
        DisposeAllWeakUnsafe(in arrayReference, MathHelper.MakeUnsigned(length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisposeAllWeakUnsafe<T>(ref readonly T? arrayReference, nuint length)
    {
        if (UnsafeHelper.IsNullRef(in arrayReference) || length == 0)
            return;
        nuint limit = length;
        for (nuint i = 0; limit >= 4; limit -= 4, i += 4)
        {
            Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, i));
            Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, i + 1));
            Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, i + 2));
            Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, i + 3));
        }
        switch (limit)
        {
            case 3:
                Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, length - 3));
                goto case 2;
            case 2:
                Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, length - 2));
                goto case 1;
            case 1:
                Dispose(UnsafeHelper.AddTypedOffsetAsReadOnly(in arrayReference, length - 1));
                break;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Dispose(T? disposable) => (disposable as IDisposable)?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDispose<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value = null) where T : class?, IDisposable?
    {
        T? oldObject = location;
        location = value;
        if (ReferenceEquals(oldObject, value) || oldObject is null)
            return;
        oldObject.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDispose<T>([NotNullIfNotNull(nameof(value))] ref T location, in T value = default) where T : struct, IDisposable
    {
        T oldObject = location;
        location = value;
        oldObject.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDispose<T>([NotNullIfNotNull(nameof(value))] ref T? location, in T value = default) where T : struct, IDisposable
    {
        T? oldObjectOptional = location;
        location = value;
        oldObjectOptional?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDisposeWeak<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value = null) where T : class
    {
        T? oldObject = location;
        location = value;
        if (ReferenceEquals(oldObject, value) || oldObject is not IDisposable disposable)
            return;
        disposable.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDispose<T>([NotNullIfNotNull(nameof(value))] ref T[]? location, T[]? value = null) where T : class?, IDisposable?
    {
        T[]? oldObject = location;
        location = value;
        if (ReferenceEquals(oldObject, value) || oldObject is null)
            return;
        for (int i = 0, count = oldObject.Length; i < count; i++)
        {
            oldObject[i]?.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDisposeWeak<T>([NotNullIfNotNull(nameof(value))] ref T[]? location, T[]? value = null) where T : class?
    {
        T[]? oldObject = location;
        location = value;
        if (ReferenceEquals(oldObject, value) || oldObject is null)
            return;
        for (int i = 0, count = oldObject.Length; i < count; i++)
        {
            (oldObject[i] as IDisposable)?.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDisposeInterlocked<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value = null) where T : class, IDisposable
    {
        T? oldObject = Interlocked.Exchange(ref location, value);
        if (ReferenceEquals(oldObject, value) || oldObject is null)
            return;
        oldObject.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDisposeInterlockedWeak<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value = null) where T : class
    {
        T? oldObject = Interlocked.Exchange(ref location, value);
        if (ReferenceEquals(oldObject, value) || oldObject is not IDisposable disposable)
            return;
        disposable.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDisposeInterlocked<T>([NotNullIfNotNull(nameof(value))] ref T[]? location, T[]? value = null) where T : class?, IDisposable?
    {
        T[]? disposingObject = Interlocked.Exchange(ref location, value);
        if (disposingObject is null)
            return;
        for (int i = 0, count = disposingObject.Length; i < count; i++)
        {
            disposingObject[i]?.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapDisposeInterlockedWeak<T>([NotNullIfNotNull(nameof(value))] ref T[]? location, T[]? value = null) where T : class?
    {
        T[]? disposingObject = Interlocked.Exchange(ref location, value);
        if (disposingObject is null)
            return;
        for (int i = 0, count = disposingObject.Length; i < count; i++)
        {
            (disposingObject[i] as IDisposable)?.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NullSwapOrDispose<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value) where T : class, IDisposable
    {
        T? oldObject = Interlocked.CompareExchange(ref location, value, null);
        if (oldObject is null || value is null || ReferenceEquals(oldObject, value))
            return;
        value.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NullSwapOrDispose<T>([NotNullIfNotNull(nameof(value))] ref T[]? location, T[]? value) where T : class?, IDisposable
    {
        T[]? oldObject = InterlockedHelper.CompareExchange(ref location, value, null);
        if (oldObject is null || value is null || ReferenceEquals(oldObject, value))
            return;
        foreach (T item in value)
            item?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NullSwapOrDisposeWeak<T>([NotNullIfNotNull(nameof(value))] ref T? location, T? value) where T : class
    {
        T? oldObject = Interlocked.CompareExchange(ref location, value, null);
        if (oldObject is null || ReferenceEquals(oldObject, value))
            return;
        (value as IDisposable)?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NullSwapOrDisposeWeak<T>([NotNullIfNotNull(nameof(value))] ref T[]? location, T[]? value) where T : class
    {
        T[]? oldObject = Interlocked.CompareExchange(ref location, value, null);
        if (oldObject is null || ReferenceEquals(oldObject, value))
            return;
        foreach (T item in value!)
            (item as IDisposable)?.Dispose();
    }
}
