using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Threading;

internal static class StateHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetValue<T>(ref readonly T valueRef, ref readonly nuint versionRef) where T : struct
    {
        nuint version = Atomics.Read(in versionRef);
        if (CheckVersionInLocked(version % 2))
            goto Slow;
        T value = valueRef;
        if (version != Atomics.Read(in versionRef))
            goto Slow;
        return value;

    Slow:
        return GetValueSlow(in valueRef, in versionRef, ref version);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetValue<T>(ref readonly T valueRef, ref readonly nuint versionRef, out nuint version) where T : struct // 方法內容完全相同，只是傳回值多了個 version
    {
        version = Atomics.Read(in versionRef);
        if (CheckVersionInLocked(version % 2))
            goto Slow;
        T value = valueRef;
        if (version != Atomics.Read(in versionRef))
            goto Slow;
        return value;

    Slow:
        return GetValueSlow(in valueRef, in versionRef, ref version);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // 避免汙染上層內聯
    private static T GetValueSlow<T>(ref readonly T valueRef, ref readonly nuint versionRef, ref nuint version) where T : struct
    {
        T value;
        SpinWait wait = new SpinWait();
        do
        {
            wait.SpinOnce();
            value = valueRef;
            nuint currentVersion = Atomics.Read(in versionRef);
            if (version == currentVersion && !CheckVersionInLocked(version % 2))
                return value;
            version = currentVersion;
        } while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetValue<T>(ref T valueRef, ref nuint versionRef, T value) where T : struct // 上層會保證獨佔，這裡壓根不需要防止多執行緒競爭，只需要保證寫入能及時被看見即可
    {
        nuint version = Atomics.Read(ref versionRef);
        DebugHelper.ThrowIf(CheckVersionInLocked(version % 2));

        Atomics.Write(ref versionRef, version + 1);
        valueRef = value;
        Atomics.Write(ref versionRef, version + 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TrySetValue<T>(ref T valueRef, ref nuint versionRef, T value, nuint version) where T : struct // 上層會保證獨佔，這裡壓根不需要防止多執行緒競爭，只需要保證寫入能及時被看見即可
    {
        nuint currentVersion = Atomics.Read(ref versionRef);
        if (currentVersion != version)
            return false;

        DebugHelper.ThrowIf(CheckVersionInLocked(version % 2));

        Atomics.Write(ref versionRef, version + 1);
        valueRef = value;
        Atomics.Write(ref versionRef, version + 2);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CheckVersionInLocked(nuint version) => MathHelper.ToBooleanUnsafe(version % 2);
}
