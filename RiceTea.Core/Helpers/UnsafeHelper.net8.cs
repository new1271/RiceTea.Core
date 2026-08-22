#if NET8_0_OR_GREATER
using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using InlineIL;

using InlineMethod;

#pragma warning disable CS8500

namespace RiceTea.Core.Helpers;

public static unsafe partial class UnsafeHelper
{
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
    {
        if (IsUnsignedIntegerType<T>())
            return default;
        if (typeof(T) == typeof(sbyte))
            return As<sbyte, T>(sbyte.MinValue);
        if (typeof(T) == typeof(short))
            return As<short, T>(short.MinValue);
        if (typeof(T) == typeof(int))
            return As<int, T>(int.MinValue);
        if (typeof(T) == typeof(long))
            return As<long, T>(long.MinValue);
        if (typeof(T) == typeof(nint))
            return As<nint, T>(nint.MinValue);
        if (typeof(T) == typeof(float))
            return As<float, T>(float.MinValue);
        if (typeof(T) == typeof(double))
            return As<double, T>(double.MinValue);
        if (typeof(T) == typeof(Half))
            return As<Half, T>(Half.MinValue);
        if (typeof(T) == typeof(decimal))
            return As<decimal, T>(decimal.MinValue);
        return GetMinValueSlow<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T GetMaxValue<T>() where T : unmanaged
    {
        if (IsUnsignedIntegerType<T>())
            return Not<T>(default);
        if (typeof(T) == typeof(sbyte))
            return As<sbyte, T>(sbyte.MaxValue);
        if (typeof(T) == typeof(short))
            return As<short, T>(short.MaxValue);
        if (typeof(T) == typeof(int))
            return As<int, T>(int.MaxValue);
        if (typeof(T) == typeof(long))
            return As<long, T>(long.MaxValue);
        if (typeof(T) == typeof(nint))
            return As<nint, T>(nint.MaxValue);
        if (typeof(T) == typeof(float))
            return As<float, T>(float.MaxValue);
        if (typeof(T) == typeof(double))
            return As<double, T>(double.MaxValue);
        if (typeof(T) == typeof(Half))
            return As<Half, T>(Half.MaxValue);
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
                   (typeof(T) == typeof(double)) ||
                   (typeof(T) == typeof(Half));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsPrimitiveType<T>() => typeof(T).IsPrimitive;

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
                   (type == typeof(double)) ||
                   (type == typeof(Half));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsPrimitiveType(Type type) => type.IsPrimitive;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsUnmanagedType<T>() => !RuntimeHelpers.IsReferenceOrContainsReferences<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsUnmanagedType(Type type) => IsPrimitiveType(type) || type.IsEnum || type.IsPointer || IsUnmanageTypeSlow(type);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsUnmanageTypeSlow(Type type)
    {
        if (!type.IsValueType)
            return false;
        return (bool)typeof(RuntimeHelpers)
            .GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences), BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(type)
            .Invoke(null, null)!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T Max<T>(T a, T b)
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
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T MaxUnsigned<T>(T a, T b)
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
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T Min<T>(T a, T b)
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
        throw IL.Unreachable();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T MinUnsigned<T>(T a, T b)
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
        throw IL.Unreachable();
    }

    /// <inheritdoc cref="Unsafe.Read{T}(void*)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T Read<T>(void* source) => Unsafe.Read<T>(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T Read<T>(ref readonly byte source) => Unsafe.As<byte, T>(ref Unsafe.AsRef(in source));

    /// <inheritdoc cref="Unsafe.ReadUnaligned{T}(void*)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    public static partial T ReadUnaligned<T>(void* source) => Unsafe.ReadUnaligned<T>(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T ReadUnaligned<T>(ref readonly byte source) => Unsafe.As<byte, T>(ref Unsafe.AsRef(in source));

    /// <inheritdoc cref="Unsafe.Write{T}(void*, T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    public static partial void Write<T>(void* destination, T value) => Unsafe.Write(destination, value);

    /// <inheritdoc cref="Unsafe.WriteUnaligned{T}(void*, T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    public static partial void WriteUnaligned<T>(void* destination, T value) => Unsafe.WriteUnaligned(destination, value);

    /// <inheritdoc cref="Unsafe.CopyBlock(void*, void*, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    public static partial void CopyBlock(void* destination, void* source, uint byteCount) => Unsafe.CopyBlock(destination, source, byteCount);

    /// <inheritdoc cref="Unsafe.CopyBlock(void*, void*, uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlock(void* destination, void* source, nuint byteCount)
    {
        IL.Push(destination);
        IL.Push(source);
        IL.Push(byteCount);
        IL.Emit.Cpblk();
    }

    /// <inheritdoc cref="Unsafe.CopyBlock(ref byte, ref readonly byte, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    public static partial void CopyBlock(ref byte destination, ref readonly byte source, uint byteCount) => Unsafe.CopyBlock(ref destination, in source, byteCount);

    /// <inheritdoc cref="Unsafe.CopyBlock(ref byte, ref readonly byte, uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlock(ref byte destination, ref readonly byte source, nuint byteCount)
    {
        IL.PushInRef(ref destination);
        IL.PushInRef(in source);
        IL.Push(byteCount);
        IL.Emit.Cpblk();
    }

    /// <inheritdoc cref="Unsafe.CopyBlockUnaligned(void*, void*, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlockUnaligned(void* destination, void* source, uint byteCount) => Unsafe.CopyBlockUnaligned(destination, source, byteCount);

    /// <inheritdoc cref="Unsafe.CopyBlockUnaligned(void*, void*, uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlockUnaligned(void* destination, void* source, nuint byteCount)
    {
        IL.Push(destination);
        IL.Push(source);
        IL.Push(byteCount);
        IL.Emit.Unaligned(1);
        IL.Emit.Cpblk();
    }

    /// <inheritdoc cref="Unsafe.CopyBlockUnaligned(ref byte, ref readonly byte, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlockUnaligned(ref byte destination, ref readonly byte source, uint byteCount) => Unsafe.CopyBlockUnaligned(ref destination, in source, byteCount);

    /// <inheritdoc cref="Unsafe.CopyBlockUnaligned(ref byte, ref readonly byte, uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void CopyBlockUnaligned(ref byte destination, ref readonly byte source, nuint byteCount)
    {
        IL.PushInRef(ref destination);
        IL.PushInRef(in source);
        IL.Push(byteCount);
        IL.Emit.Unaligned(1);
        IL.Emit.Cpblk();
    }

    /// <inheritdoc cref="Unsafe.BitCast{TFrom, TTo}(TFrom)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial TTo As<TFrom, TTo>(TFrom source) => ReadUnaligned<TTo>(ref As<TFrom, byte>(ref source));

    /// <inheritdoc cref="Unsafe.As{TFrom, TTo}(ref TFrom)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref TTo As<TFrom, TTo>(ref TFrom source) => ref Unsafe.As<TFrom, TTo>(ref source);

    /// <inheritdoc cref="Unsafe.As{T}(object)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T As<T>(object source) where T : class => Unsafe.As<T>(source);

    /// <inheritdoc cref="Unsafe.InitBlock(ref byte, byte, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlock(ref byte location, byte value, uint count) => Unsafe.InitBlock(ref location, value, count);

    /// <inheritdoc cref="Unsafe.InitBlock(ref byte, byte, uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlock(ref byte location, byte value, nuint count)
    {
        IL.PushInRef(ref location);
        IL.Push(value);
        IL.Push(count);
        IL.Emit.Initblk();
    }

    /// <inheritdoc cref="Unsafe.InitBlock(void*, byte, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlock(void* ptr, byte value, uint size) => Unsafe.InitBlock(ptr, value, size);

    /// <inheritdoc cref="Unsafe.InitBlock(void*, byte, uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlock(void* ptr, byte value, nuint size)
    {
        IL.Push(ptr);
        IL.Push(value);
        IL.Push(size);
        IL.Emit.Initblk();
    }

    /// <inheritdoc cref="Unsafe.InitBlockUnaligned(ref byte, byte, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlockUnaligned(ref byte location, byte value, uint size) => Unsafe.InitBlockUnaligned(ref location, value, size);

    /// <inheritdoc cref="Unsafe.InitBlockUnaligned(ref byte, byte, uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlockUnaligned(ref byte location, byte value, nuint size)
    {
        IL.PushInRef(ref location);
        IL.Push(value);
        IL.Push(size);
        IL.Emit.Unaligned(sizeof(byte));
        IL.Emit.Initblk();
    }

    /// <inheritdoc cref="Unsafe.InitBlockUnaligned(void*, byte, uint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlockUnaligned(void* ptr, byte value, uint size) => Unsafe.InitBlockUnaligned(ptr, value, size);

    /// <inheritdoc cref="Unsafe.InitBlockUnaligned(void*, byte, uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void InitBlockUnaligned(void* ptr, byte value, nuint size)
    {
        IL.Push(ptr);
        IL.Push(value);
        IL.Push(size);
        IL.Emit.Unaligned(sizeof(byte));
        IL.Emit.Initblk();
    }

    /// <inheritdoc cref="Unsafe.ByteOffset{T}(ref readonly T, ref readonly T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nint ByteOffset<T>(ref readonly T origin, ref readonly T target) => Unsafe.ByteOffset(in origin, in target);

    /// <inheritdoc cref="Unsafe.ByteOffset{T}(ref readonly T, ref readonly T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial nuint ByteOffsetUnsigned<T>(ref readonly T origin, ref readonly T target) => (nuint)Unsafe.ByteOffset(in origin, in target);

    /// <inheritdoc cref="Unsafe.AddByteOffset{T}(ref T, nint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AddByteOffset<T>(ref T source, nint byteOffset)
        => ref Unsafe.AddByteOffset(ref source, byteOffset);

    /// <inheritdoc cref="Unsafe.AddByteOffset{T}(ref T, nuint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AddByteOffset<T>(ref T source, nuint byteOffset)
        => ref Unsafe.AddByteOffset(ref source, byteOffset);

    /// <inheritdoc cref="Unsafe.Add{T}(ref T, nint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AddTypedOffset<T>(ref T source, nint elementOffset)
        => ref Unsafe.Add(ref source, elementOffset);

    /// <inheritdoc cref="Unsafe.Add{T}(ref T, nuint)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AddTypedOffset<T>(ref T source, nuint elementOffset)
        => ref Unsafe.Add(ref source, elementOffset);

    /// <inheritdoc cref="Unsafe.SkipInit{T}(out T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void SkipInit<T>(out T value) => Unsafe.SkipInit(out value);

    /// <inheritdoc cref="Unsafe.IsNullRef{T}(ref readonly T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial bool IsNullRef<T>(ref readonly T source) => Unsafe.IsNullRef(in source);

    /// <inheritdoc cref="Unsafe.NullRef{T}()"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T NullRef<T>() => ref Unsafe.NullRef<T>();

    /// <inheritdoc cref="Unsafe.AsRef{T}(void*)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    public static partial ref T AsRef<T>(T* source) => ref Unsafe.AsRef<T>(source);

    /// <inheritdoc cref="Unsafe.AsRef{T}(ref readonly T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AsRefIn<T>(in T source) => ref Unsafe.AsRef(in source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T AsRefOut<T>(out T source)
    {
        IL.PushOutRef(out source);
        return ref IL.ReturnRef<T>();
    }

    /// <inheritdoc cref="Unsafe.AsPointer{T}(ref T)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T* AsPointerRef<T>(ref T value) => (T*)Unsafe.AsPointer(ref value);

    /// <inheritdoc cref="Unsafe.AsPointer{T}(ref T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void** AsPointerRef(ref void* value)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    /// <inheritdoc cref="Unsafe.AsPointer{T}(ref T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T* AsPointerIn<T>(in T value)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    /// <inheritdoc cref="Unsafe.AsPointer{T}(ref T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void** AsPointerIn(in void* value)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    /// <inheritdoc cref="Unsafe.AsPointer{T}(ref T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial T* AsPointerOut<T>(out T value)
    {
        IL.PushOutRef(out value);
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    /// <inheritdoc cref="Unsafe.AsPointer{T}(ref T)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void** AsPointerOut(out void* value)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    /// <inheritdoc cref="Unsafe.SizeOf{T}"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial int SizeOfSigned<T>() => Unsafe.SizeOf<T>();

    /// <inheritdoc cref="Unsafe.SizeOf{T}"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint SizeOf<T>() => (uint)Unsafe.SizeOf<T>();

    /// <inheritdoc cref="string.GetPinnableReference"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref readonly char GetStringDataReference(string str) => ref str.GetPinnableReference();

    /// <inheritdoc cref="MemoryMarshal.GetArrayDataReference{T}(T[])"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial ref T GetArrayDataReference<T>(T[] array) => ref MemoryMarshal.GetArrayDataReference(array);

    private static class MaxHelper<T> where T : unmanaged
    {
        public static readonly T MaxValue = GetMaxValue();

        private static T GetMaxValue()
        {
            Type type = typeof(T);
            if (TryGetValueFromStandardInterface(type, out T result))
                return result;
            FieldInfo? field = type.GetField(nameof(MaxValue), BindingFlags.Public | BindingFlags.Static);
            if (field is not null && field.IsInitOnly && field.FieldType == type)
                return (T)field.GetValue(null)!;
            PropertyInfo? prop = type.GetProperty(nameof(MaxValue), BindingFlags.Public | BindingFlags.Static);
            if (prop is not null && prop.CanRead && !prop.CanWrite && prop.PropertyType == type)
                return (T)prop.GetValue(null)!;
            throw new InvalidOperationException($"{typeof(T).FullName} doesn't have {nameof(MaxValue)}!");

            static bool TryGetValueFromStandardInterface(Type type, out T result)
            {
                try
                {
                    result = (T)typeof(ConstraintedMinMaxHelper<>)
                        .MakeGenericType(type)
                        .GetField(nameof(ConstraintedMinMaxHelper<>.MaxValue), BindingFlags.Public | BindingFlags.Static)!
                        .GetValue(null)!;
                    return true;
                }
                catch (Exception)
                {
                    result = default;
                    return false;
                }
            }
        }
    }

    private static class MinHelper<T> where T : unmanaged
    {
        public static readonly T MinValue = GetMinValue();

        private static T GetMinValue()
        {
            Type type = typeof(T);
            if (TryGetValueFromStandardInterface(type, out T result))
                return result;
            FieldInfo? field = type.GetField(nameof(MinValue), BindingFlags.Public | BindingFlags.Static);
            if (field is not null && field.IsInitOnly && field.FieldType == type)
                return (T)field.GetValue(null)!;
            PropertyInfo? prop = type.GetProperty(nameof(MinValue), BindingFlags.Public | BindingFlags.Static);
            if (prop is not null && prop.CanRead && !prop.CanWrite && prop.PropertyType == type)
                return (T)prop.GetValue(null)!;
            throw new InvalidOperationException($"{typeof(T).FullName} doesn't have {nameof(MinValue)}!");

            static bool TryGetValueFromStandardInterface(Type type, out T result)
            {
                try
                {
                    result = (T)typeof(ConstraintedMinMaxHelper<>)
                        .MakeGenericType(type)
                        .GetField(nameof(ConstraintedMinMaxHelper<>.MinValue), BindingFlags.Public | BindingFlags.Static)!
                        .GetValue(null)!;
                    return true;
                }
                catch (Exception)
                {
                    result = default;
                    return false;
                }
            }
        }
    }

    private static class ConstraintedMinMaxHelper<T> where T : unmanaged, IMinMaxValue<T>
    {
        public static readonly T MinValue = T.MinValue;
        public static readonly T MaxValue = T.MaxValue;
    }
}
#endif