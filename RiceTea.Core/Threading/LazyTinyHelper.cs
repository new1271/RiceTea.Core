using System;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Threading;

internal static class LazyTinyHelper
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T InitializeAndReturn<T>(ref T? location, Func<T>? factory, bool threadSafe, Lock? syncLock) where T : class
    {
        T? result;
        if (!threadSafe) // 對應 LazyThreadSafetyMode.None
        {
            result = InitializeOrThrow(factory);
            location = result;
            return result;
        }
        if (syncLock is null) // 對應 LazyThreadSafetyMode.PublicationOnly
        {
            result = Atomics.Read(ref location);
            if (result is null)
            {
                result = InitializeOrThrow(factory);
                T? oldResult = Atomics.CompareExchange(ref location, result, null);
                if (oldResult is not null)
                {
                    (result as IDisposable)?.Dispose();
                    result = oldResult;
                }
            }
            return result;
        }
        // 對應 LazyThreadSafetyMode.ExecutionAndPublication
        lock (syncLock)
        {
            result = location;
            if (result is null)
            {
                result = InitializeOrThrow(factory);
                location = result;
            }
        }
        return result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T InitializeOrThrow(Func<T>? factory)
        {
            if (factory is null)
                return Activator.CreateInstance<T>();
            return NullSafetyHelper.ThrowIfNull(factory.Invoke());
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T InitializeAndReturn<T, TState>(ref T? location, Func<TState, T> factory, TState state, bool threadSafe, Lock? syncLock) where T : class
    {
        T? result;
        if (!threadSafe) // 對應 LazyThreadSafetyMode.None
        {
            result = InitializeOrThrow(factory, state);
            location = result;
            return result;
        }
        if (syncLock is null) // 對應 LazyThreadSafetyMode.PublicationOnly
        {
            result = Atomics.Read(ref location);
            if (result is null)
            {
                result = InitializeOrThrow(factory, state);
                T? oldResult = Atomics.CompareExchange(ref location, result, null);
                if (oldResult is not null)
                {
                    (result as IDisposable)?.Dispose();
                    result = oldResult;
                }
            }
            return result;
        }
        // 對應 LazyThreadSafetyMode.ExecutionAndPublication
        lock (syncLock)
        {
            result = location;
            if (result is null)
            {
                result = InitializeOrThrow(factory, state);
                location = result;
            }
        }
        return result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T InitializeOrThrow(Func<TState, T> factory, TState state) 
            => NullSafetyHelper.ThrowIfNull(factory.Invoke(state));
    }
}
