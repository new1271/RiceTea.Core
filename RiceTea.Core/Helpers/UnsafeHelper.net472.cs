#if NET472_OR_GREATER
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using InlineIL;

using InlineMethod;

#pragma warning disable CS8500

namespace RiceTea.Core.Helpers;

public static unsafe partial class UnsafeHelper
{
    private static readonly bool _isMono = PlatformHelper.IsMono;

    public static partial T GetAllBitsSetValue<T>() where T : unmanaged
        => sizeof(T) switch
        {
            8 => As<ulong, T>(ulong.MaxValue),
            4 => As<uint, T>(uint.MaxValue),
            2 => As<ushort, T>(ushort.MaxValue),
            1 => As<byte, T>(byte.MaxValue),
            _ => GetAllBitsValue_Other<T>()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetAllBitsValue_Other<T>() where T : unmanaged
    {
        T result;
        InitBlockUnaligned(&result, 0xFF, SizeOf<T>());
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T GetMinValue<T>() where T : unmanaged
        => sizeof(T) switch
        {
            1 => GetMinValue_1<T>(),
            2 => GetMinValue_2<T>(),
            4 => GetMinValue_4<T>(),
            8 => GetMinValue_8<T>(),
            16 => GetMinValue_16<T>(),
            _ => GetMinValueSlow<T>()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMinValue_1<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(sbyte))
            return As<sbyte, T>(sbyte.MinValue);
        if (typeof(T) == typeof(byte))
            return As<byte, T>(byte.MinValue);
        return GetMinValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMinValue_2<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(short))
            return As<short, T>(short.MinValue);
        if (typeof(T) == typeof(ushort))
            return As<ushort, T>(ushort.MinValue);
        return GetMinValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMinValue_4<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(int) || typeof(T) == typeof(nint))
            return As<int, T>(int.MinValue);
        if (typeof(T) == typeof(uint) || typeof(T) == typeof(nuint))
            return As<uint, T>(uint.MinValue);
        if (typeof(T) == typeof(float))
            return As<float, T>(float.MinValue);
        return GetMinValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMinValue_8<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(long) || typeof(T) == typeof(nint))
            return As<long, T>(long.MinValue);
        if (typeof(T) == typeof(ulong) || typeof(T) == typeof(nuint))
            return As<ulong, T>(ulong.MinValue);
        if (typeof(T) == typeof(double))
            return As<double, T>(double.MinValue);
        return GetMinValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMinValue_16<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(decimal))
            return As<decimal, T>(decimal.MinValue);
        return GetMinValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T GetMaxValue<T>() where T : unmanaged
        => sizeof(T) switch
        {
            1 => GetMaxValue_1<T>(),
            2 => GetMaxValue_2<T>(),
            4 => GetMaxValue_4<T>(),
            8 => GetMaxValue_8<T>(),
            16 => GetMaxValue_16<T>(),
            _ => GetMaxValueSlow<T>()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMaxValue_1<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(sbyte))
            return As<sbyte, T>(sbyte.MaxValue);
        if (typeof(T) == typeof(byte))
            return As<byte, T>(byte.MaxValue);
        return GetMaxValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMaxValue_2<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(short))
            return As<short, T>(short.MaxValue);
        if (typeof(T) == typeof(ushort))
            return As<ushort, T>(ushort.MaxValue);
        return GetMaxValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMaxValue_4<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(int) || typeof(T) == typeof(nint))
            return As<int, T>(int.MaxValue);
        if (typeof(T) == typeof(uint) || typeof(T) == typeof(nuint))
            return As<uint, T>(uint.MaxValue);
        if (typeof(T) == typeof(float))
            return As<float, T>(float.MaxValue);
        return GetMaxValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMaxValue_8<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(long) || typeof(T) == typeof(nint))
            return As<long, T>(long.MaxValue);
        if (typeof(T) == typeof(ulong) || typeof(T) == typeof(nuint))
            return As<ulong, T>(ulong.MaxValue);
        if (typeof(T) == typeof(double))
            return As<double, T>(double.MaxValue);
        return GetMaxValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetMaxValue_16<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(decimal))
            return As<decimal, T>(decimal.MaxValue);
        return GetMaxValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T GetMinValueSlow<T>() where T : unmanaged => MinHelper<T>.MinValue;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T GetMaxValueSlow<T>() where T : unmanaged => MaxHelper<T>.MaxValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsSignedIntegerType<T>()
            => (typeof(T) == typeof(sbyte)) ||
                   (typeof(T) == typeof(short)) ||
                   (typeof(T) == typeof(int)) ||
                   (typeof(T) == typeof(long)) ||
                   (typeof(T) == typeof(nint));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsUnsignedIntegerType<T>()
            => (typeof(T) == typeof(byte)) ||
                   (typeof(T) == typeof(char)) ||
                   (typeof(T) == typeof(ushort)) ||
                   (typeof(T) == typeof(uint)) ||
                   (typeof(T) == typeof(ulong)) ||
                   (typeof(T) == typeof(nuint));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsFloatingPointType<T>()
            => (typeof(T) == typeof(float)) ||
                   (typeof(T) == typeof(double));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsSignedIntegerType(Type type)
    => (type == typeof(sbyte)) ||
           (type == typeof(short)) ||
           (type == typeof(int)) ||
           (type == typeof(long)) ||
           (type == typeof(nint));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsUnsignedIntegerType(Type type)
            => (type == typeof(byte)) ||
                   (type == typeof(char)) ||
                   (type == typeof(ushort)) ||
                   (type == typeof(uint)) ||
                   (type == typeof(ulong)) ||
                   (type == typeof(nuint));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsFloatingPointType(Type type)
            => (type == typeof(float)) ||
                   (type == typeof(double));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsPrimitiveType<T>()
        => sizeof(T) switch
        {
            1 => IsPrimitiveType_1<T>(),
            2 => IsPrimitiveType_2<T>(),
            4 => IsPrimitiveType_4<T>(),
            8 => IsPrimitiveType_8<T>(),
            _ => false,
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPrimitiveType_1<T>()
        => typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) || typeof(T) == typeof(bool);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPrimitiveType_2<T>()
        => typeof(T) == typeof(short) || typeof(T) == typeof(ushort) || typeof(T) == typeof(char);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPrimitiveType_4<T>()
        => typeof(T) == typeof(int) || typeof(T) == typeof(uint) ||
        typeof(T) == typeof(nint) || typeof(T) == typeof(nuint) ||
        typeof(T) == typeof(float);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPrimitiveType_8<T>()
        => typeof(T) == typeof(long) || typeof(T) == typeof(ulong) ||
        typeof(T) == typeof(nint) || typeof(T) == typeof(nuint) ||
        typeof(T) == typeof(double);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsPrimitiveType(Type type) => type.IsPrimitive;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsUnmanagedType<T>() => IsPrimitiveType<T>() || typeof(T).IsEnum || typeof(T).IsPointer || IsUnmanageTypeSlow<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsUnmanagedType(Type type) => IsPrimitiveType(type) || type.IsEnum || type.IsPointer || IsUnmanageTypeSlow(type);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsUnmanageTypeSlow<T>()
    {
        if (!typeof(T).IsValueType)
            return false;
        IL.Emit.Ldtoken(typeof(UnmanagedTypeHelper<T>));
        IL.Emit.Call(new MethodRef(typeof(RuntimeHelpers), nameof(RuntimeHelpers.RunClassConstructor)));
        return UnmanagedTypeHelper<T>.IsUnmanaged;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsUnmanageTypeSlow(Type type)
    {
        if (!type.IsValueType)
            return false;
        Type helperType = typeof(UnmanagedTypeHelper<>).MakeGenericType(type);
        RuntimeHelpers.RunClassConstructor(helperType.TypeHandle);
        return (bool)helperType.GetField(nameof(UnmanagedTypeHelper<>.IsUnmanaged), BindingFlags.Public | BindingFlags.Static).GetValue(null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T Max<T>(T a, T b)
    {
        if (IsSignedIntegerType<T>())
        {
            T mask = Negate(As<bool, T>(IsGreaterThan(a, b)));
            return Or(And(a, mask), And(b, Not(mask)));
        }
        else
        {
            const string Label = "jump";

            IL.Push(a);
            IL.Push(b);
            IL.Emit.Bgt_S(Label);
            IL.Push(b);
            IL.Emit.Ret();
            IL.MarkLabel(Label);
            IL.Push(a);
            IL.Emit.Ret();
        }
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T MaxUnsigned<T>(T a, T b)
    {
        if (IsUnsignedIntegerType<T>())
        {
            // a ^ ((a ^ b) & -(a < b))
            return Xor(a, And(Xor(a, b), Negate(As<bool, T>(IsLessThanUnsigned(a, b)))));
        }
        else
        {
            const string Label = "jump";

            IL.Push(a);
            IL.Push(b);
            IL.Emit.Bgt_Un_S(Label);
            IL.Push(b);
            IL.Emit.Ret();
            IL.MarkLabel(Label);
            IL.Push(a);
            IL.Emit.Ret();
        }
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T Min<T>(T a, T b)
    {
        if (IsSignedIntegerType<T>())
        {
            T mask = Negate(As<bool, T>(IsLessThan(a, b)));
            return Or(And(a, mask), And(b, Not(mask)));
        }
        else
        {
            const string Label = "jump";

            IL.Push(a);
            IL.Push(b);
            IL.Emit.Blt_S(Label);
            IL.Push(b);
            IL.Emit.Ret();
            IL.MarkLabel(Label);
            IL.Push(a);
            IL.Emit.Ret();
        }
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T MinUnsigned<T>(T a, T b)
    {
        if (IsUnsignedIntegerType<T>())
        {
            //a ^ ((a ^ b) & -(a > b))
            return Xor(a, And(Xor(a, b), Negate(As<bool, T>(IsGreaterThanUnsigned(a, b)))));
        }
        else
        {
            const string Label = "jump";

            IL.Push(a);
            IL.Push(b);
            IL.Emit.Blt_Un_S(Label);
            IL.Push(b);
            IL.Emit.Ret();
            IL.MarkLabel(Label);
            IL.Push(a);
            IL.Emit.Ret();
        }
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T Read<T>(void* source)
    {
        IL.Push(source);
        IL.Emit.Ldobj(typeof(T));
        return IL.Return<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T Read<T>(ref readonly byte source)
    {
        IL.PushInRef(in source);
        IL.Emit.Ldobj(typeof(T));
        return IL.Return<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T ReadUnaligned<T>(void* source)
    {
        IL.Push(source);
        IL.Emit.Unaligned(1);
        IL.Emit.Ldobj(typeof(T));
        return IL.Return<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T ReadUnaligned<T>(ref readonly byte source)
    {
        IL.PushInRef(in source);
        IL.Emit.Unaligned(1);
        IL.Emit.Ldobj(typeof(T));
        return IL.Return<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Write<T>(void* destination, T value)
    {
        IL.Push(destination);
        IL.Push(value);
        IL.Emit.Stobj(typeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void WriteUnaligned<T>(void* destination, T value)
    {
        IL.Push(destination);
        IL.Push(value);
        IL.Emit.Unaligned(1);
        IL.Emit.Stobj(typeof(T));
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlock(void* destination, void* source, uint byteCount)
    {
        IL.Push(destination);
        IL.Push(source);
        IL.Push(byteCount);
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlock(void* destination, void* source, nuint byteCount)
    {
        IL.Push(destination);
        IL.Push(source);
        IL.Push(byteCount);
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlock(ref byte destination, ref readonly byte source, uint byteCount)
    {
        IL.PushInRef(ref destination);
        IL.PushInRef(in source);
        IL.Push(byteCount);
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlock(ref byte destination, ref readonly byte source, nuint byteCount)
    {
        IL.PushInRef(ref destination);
        IL.PushInRef(in source);
        IL.Push(byteCount);
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlockUnaligned(void* destination, void* source, uint byteCount)
    {
        IL.Push(destination);
        IL.Push(source);
        IL.Push(byteCount);
        IL.Emit.Unaligned(1);
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlockUnaligned(void* destination, void* source, nuint byteCount)
    {
        IL.Push(destination);
        IL.Push(source);
        IL.Push(byteCount);
        IL.Emit.Unaligned(1);
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlockUnaligned(ref byte destination, ref readonly byte source, uint byteCount)
    {
        IL.PushInRef(ref destination);
        IL.PushInRef(in source);
        IL.Push(byteCount);
        IL.Emit.Unaligned(1);
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlockUnaligned(ref byte destination, ref readonly byte source, nuint byteCount)
    {
        IL.PushInRef(ref destination);
        IL.PushInRef(in source);
        IL.Push(byteCount);
        IL.Emit.Unaligned(1);
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial TTo As<TFrom, TTo>(TFrom source)
    {
        IL.Push(source);
        return IL.Return<TTo>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref TTo As<TFrom, TTo>(ref TFrom source)
    {
        IL.PushInRef(ref source!);
        return ref IL.ReturnRef<TTo>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T As<T>(object source) where T : class
    {
        IL.Push(source);
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlock(ref byte location, byte value, uint count)
    {
        IL.PushInRef(ref location);
        IL.Push(value);
        IL.Push(count);
        IL.Emit.Initblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlock(ref byte location, byte value, nuint count)
    {
        IL.PushInRef(ref location);
        IL.Push(value);
        IL.Push(count);
        IL.Emit.Initblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlock(void* ptr, byte value, uint size)
    {
        IL.Push(ptr);
        IL.Push(value);
        IL.Push(size);
        IL.Emit.Initblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlock(void* ptr, byte value, nuint size)
    {
        IL.Push(ptr);
        IL.Push(value);
        IL.Push(size);
        IL.Emit.Initblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlockUnaligned(ref byte location, byte value, uint size)
    {
        IL.PushInRef(ref location);
        IL.Push(value);
        IL.Push(size);
        IL.Emit.Unaligned(sizeof(byte));
        IL.Emit.Initblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlockUnaligned(ref byte location, byte value, nuint size)
    {
        IL.PushInRef(ref location);
        IL.Push(value);
        IL.Push(size);
        IL.Emit.Unaligned(sizeof(byte));
        IL.Emit.Initblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlockUnaligned(void* ptr, byte value, uint size)
    {
        IL.Push(ptr);
        IL.Push(value);
        IL.Push(size);
        IL.Emit.Unaligned(sizeof(byte));
        IL.Emit.Initblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlockUnaligned(void* ptr, byte value, nuint size)
    {
        IL.Push(ptr);
        IL.Push(value);
        IL.Push(size);
        IL.Emit.Unaligned(sizeof(byte));
        IL.Emit.Initblk();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint ByteOffset<T>(ref readonly T origin, ref readonly T target)
    {
        IL.PushInRef(in target);
        IL.PushInRef(in origin);
        IL.Emit.Sub();
        return IL.Return<nint>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint ByteOffsetUnsigned<T>(ref readonly T origin, ref readonly T target)
    {
        IL.PushInRef(in target);
        IL.PushInRef(in origin);
        IL.Emit.Sub();
        return IL.Return<nuint>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AddByteOffset<T>(ref T source, nint byteOffset)
    {
        IL.Push(ref source!);
        IL.Push(byteOffset);
        IL.Emit.Add();
        return ref IL.ReturnRef<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AddByteOffset<T>(ref T source, nuint byteOffset)
    {
        IL.Push(ref source!);
        IL.Push(byteOffset);
        IL.Emit.Add();
        return ref IL.ReturnRef<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AddTypedOffset<T>(ref T source, nint elementOffset)
    {
        IL.Push(ref source!);
        IL.Push(elementOffset);
        IL.Emit.Sizeof(typeof(T));
        IL.Emit.Mul();
        IL.Emit.Add();
        return ref IL.ReturnRef<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AddTypedOffset<T>(ref T source, nuint elementOffset)
    {
        IL.Push(ref source!);
        IL.Push(elementOffset);
        IL.Emit.Sizeof(typeof(T));
        IL.Emit.Mul();
        IL.Emit.Add();
        return ref IL.ReturnRef<T>();
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void SkipInit<T>(out T value)
    {
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsNullRef<T>(ref readonly T source)
    {
        IL.PushInRef(in source);
        IL.Emit.Ldc_I4_0();
        IL.Emit.Conv_U();
        IL.Emit.Ceq();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T NullRef<T>()
    {
        IL.Emit.Ldc_I4_0();
        IL.Emit.Conv_U();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AsRef<T>(T* source)
    {
        IL.Emit.Ldarg_0();
        return ref IL.ReturnRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AsRefIn<T>(in T source)
    {
        IL.Emit.Ldarg_0();
        return ref IL.ReturnRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AsRefOut<T>(out T source)
    {
        IL.PushOutRef(out source);
        return ref IL.ReturnRef<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T* AsPointerRef<T>(ref T value)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void** AsPointerRef(ref void* value)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T* AsPointerIn<T>(in T value)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void** AsPointerIn(in void* value)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T* AsPointerOut<T>(out T value)
    {
        IL.PushOutRef(out value);
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void** AsPointerOut(out void* value)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int SizeOfSigned<T>()
    {
        IL.Emit.Sizeof<T>();
        return IL.Return<int>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint SizeOf<T>()
    {
        IL.Emit.Sizeof<T>();
        return IL.Return<uint>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref readonly char GetStringDataReference(string str)
    {
        if (!_isMono)
            return ref FastRoute(str);

        return ref LegacyStringHelper.GetReference(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ref char FastRoute(string str) // 切割方法以誘導 JIT 內聯
        {
            DebugHelper.ThrowIf(RuntimeHelpers.OffsetToStringData < PointerSize);
            return ref AddByteOffset(ref As<byte, char>(ref As<RawData>(str).Data), RuntimeHelpers.OffsetToStringData - PointerSize);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T GetArrayDataReference<T>(T[] array)
    {
        if (!_isMono)
            return ref FastRoute(array);

        return ref LegacyArrayHelper.GetReference(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ref T FastRoute(T[] array) // 切割方法以誘導 JIT 內聯
            => ref AddByteOffset(ref As<byte, T>(ref As<RawData>(array).Data), PointerSize);
    }

    [StructLayout(LayoutKind.Sequential)]
    private sealed class RawData
    {
        public byte Data;
    }

    private static class LegacyStringHelper
    {
        private static readonly nuint Offset = GetFirstCharOffsetOfString();

        private static nuint GetFirstCharOffsetOfString()
        {
            string str = string.Empty;
            fixed (char* ptr = str)
            {
                ref byte firstField = ref As<RawData>(str).Data;
                return ByteOffsetUnsigned(ref firstField, ref As<char, byte>(ref AsRef(ptr)));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref char GetReference(string str)
            => ref As<byte, char>(ref AddByteOffset(ref As<RawData>(str).Data, Offset));
    }

    private static class LegacyArrayHelper
    {
        private static readonly nuint Offset = GetFirstElementOffsetOfArray();

        private static nuint GetFirstElementOffsetOfArray()
        {
            byte[] array = new byte[1] { default };
            return ByteOffsetUnsigned(ref As<RawData>(array).Data, ref array[0]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetReference<T>(T[] array)
            => ref As<byte, T>(ref AddByteOffset(ref As<RawData>(array).Data, Offset));
    }

    private static class MaxHelper<T> where T : unmanaged
    {
        public static readonly T MaxValue = GetMaxValue();

        private static T GetMaxValue()
        {
            Type type = typeof(T);
            FieldInfo? field = type.GetField(nameof(MaxValue), BindingFlags.Public | BindingFlags.Static);
            if (field is not null && field.IsInitOnly && field.FieldType == type)
                return (T)field.GetValue(null)!;
            PropertyInfo? prop = type.GetProperty(nameof(MaxValue), BindingFlags.Public | BindingFlags.Static);
            if (prop is not null && prop.CanRead && !prop.CanWrite && prop.PropertyType == type)
                return (T)prop.GetValue(null)!;
            throw new InvalidOperationException($"{typeof(T).FullName} doesn't have {nameof(MaxValue)}!");
        }
    }

    private static class MinHelper<T> where T : unmanaged
    {
        public static readonly T MinValue = GetMinValue();

        private static T GetMinValue()
        {
            Type type = typeof(T);
            FieldInfo? field = type.GetField(nameof(MinValue), BindingFlags.Public | BindingFlags.Static);
            if (field is not null && field.IsInitOnly && field.FieldType == type)
                return (T)field.GetValue(null)!;
            PropertyInfo? prop = type.GetProperty(nameof(MinValue), BindingFlags.Public | BindingFlags.Static);
            if (prop is not null && prop.CanRead && !prop.CanWrite && prop.PropertyType == type)
                return (T)prop.GetValue(null)!;
            throw new InvalidOperationException($"{typeof(T).FullName} doesn't have {nameof(MinValue)}!");
        }
    }

    // 此處只會有結構體類型進入
    private static class UnmanagedTypeHelper<T>
    {
        public static readonly bool IsUnmanaged = CheckIsUnmanaged();

        private static bool CheckIsUnmanaged()
        {
            foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!IsUnmanagedType(field.FieldType))
                    return false;
            }
            return true;
        }
    }
}
#endif