using System;
using System.Runtime.CompilerServices;

using InlineIL;

using InlineMethod;

#pragma warning disable CS8500

namespace RiceTea.Core.Helpers;

[SkipLocalsInit]
public static unsafe partial class UnsafeHelper
{
    public const int PointerSizeConstant_Indeterminate = 0;

    public const int PointerSizeConstant
#if ANYCPU
            = PointerSizeConstant_Indeterminate;
#elif B32_ARCH
            = sizeof(uint);
#elif B64_ARCH
            = sizeof(ulong);
#else
            = PointerSizeConstant_Indeterminate;
#endif

    public static int PointerSize
    {
        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PointerSizeConstant switch
        {
            PointerSizeConstant_Indeterminate => sizeof(void*),
            _ => PointerSizeConstant,
        };
    }

    public static uint PointerSizeUnsigned
    {
        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PointerSizeConstant switch
        {
            PointerSizeConstant_Indeterminate => SizeOf<nuint>(),
            _ => PointerSizeConstant,
        };
    }

    public static void* PointerMinValue
    {
        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (void*)0;
    }

    public static void* PointerMaxValue
    {
        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (void*)-1;
    }

    public static partial T GetAllBitsSetValue<T>() where T : unmanaged;

    public static partial T GetMinValue<T>() where T : unmanaged;

    public static partial T GetMaxValue<T>() where T : unmanaged;

    public static partial bool IsSignedIntegerType<T>();

    public static partial bool IsUnsignedIntegerType<T>();

    public static partial bool IsFloatingPointType<T>();

    public static partial bool IsPrimitiveType<T>();

    public static partial bool IsUnmanagedType<T>();

    public static partial bool IsSignedIntegerType(Type type);

    public static partial bool IsUnsignedIntegerType(Type type);

    public static partial bool IsFloatingPointType(Type type);

    public static partial bool IsUnmanagedType(Type type);

    public static partial bool IsPrimitiveType(Type type);

    public static partial T Max<T>(T a, T b);

    public static partial T MaxUnsigned<T>(T a, T b);

    public static partial T Min<T>(T a, T b);

    public static partial T MinUnsigned<T>(T a, T b);

    public static partial T Read<T>(void* source);

    public static partial T Read<T>(ref readonly byte source);

    public static partial T ReadUnaligned<T>(void* source);

    public static partial T ReadUnaligned<T>(ref readonly byte source);

    public static partial void Write<T>(void* destination, T value);

    public static partial void WriteUnaligned<T>(void* destination, T value);

    public static partial void CopyBlock(void* destination, void* source, uint byteCount);

    public static partial void CopyBlock(void* destination, void* source, nuint byteCount);

    public static partial void CopyBlock(ref byte destination, ref readonly byte source, uint byteCount);

    public static partial void CopyBlock(ref byte destination, ref readonly byte source, nuint byteCount);

    public static partial void CopyBlockUnaligned(void* destination, void* source, uint byteCount);

    public static partial void CopyBlockUnaligned(void* destination, void* source, nuint byteCount);

    public static partial void CopyBlockUnaligned(ref byte destination, ref readonly byte source, uint byteCount);

    public static partial void CopyBlockUnaligned(ref byte destination, ref readonly byte source, nuint byteCount);

    public static partial TTo As<TFrom, TTo>(TFrom source);

    public static partial ref TTo As<TFrom, TTo>(ref TFrom source);

    public static partial T As<T>(object source) where T : class;

    public static partial void InitBlock(ref byte location, byte value, uint count);

    public static partial void InitBlock(ref byte location, byte value, nuint count);

    public static partial void InitBlock(void* ptr, byte value, uint size);

    public static partial void InitBlock(void* ptr, byte value, nuint size);

    public static partial void InitBlockUnaligned(ref byte location, byte value, uint size);

    public static partial void InitBlockUnaligned(ref byte location, byte value, nuint size);

    public static partial void InitBlockUnaligned(void* ptr, byte value, uint size);

    public static partial void InitBlockUnaligned(void* ptr, byte value, nuint size);

    public static partial nint ByteOffset<T>(ref readonly T origin, ref readonly T target);

    public static partial nuint ByteOffsetUnsigned<T>(ref readonly T origin, ref readonly T target);

    public static partial ref T AddByteOffset<T>(ref T source, nint byteOffset);

    public static partial ref T AddByteOffset<T>(ref T source, nuint byteOffset);

    public static partial ref T AddTypedOffset<T>(ref T source, nint elementOffset);

    public static partial ref T AddTypedOffset<T>(ref T source, nuint elementOffset);

    public static partial void SkipInit<T>(out T value);

    public static partial bool IsNullRef<T>(ref readonly T source);

    public static partial ref T NullRef<T>();

    public static partial ref T AsRef<T>(T* source);

    public static partial ref T AsRefIn<T>(in T source);

    public static partial ref T AsRefOut<T>(out T source);

    public static partial T* AsPointerRef<T>(ref T value);

    public static partial void** AsPointerRef(ref void* value);

    public static partial T* AsPointerIn<T>(in T value);

    public static partial void** AsPointerIn(in void* value);

    public static partial T* AsPointerOut<T>(out T value);

    public static partial void** AsPointerOut(out void* value);

    public static partial ref readonly char GetStringDataReference(string str);

    public static partial ref T GetArrayDataReference<T>(T[] array);

    public static partial int SizeOfSigned<T>();

    public static partial uint SizeOf<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T AddByteOffsetAsReadOnly<T>(ref readonly T source, nint byteOffset)
        => ref AddByteOffset(ref AsRefIn(in source), byteOffset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T AddByteOffsetAsReadOnly<T>(ref readonly T source, nuint byteOffset)
        => ref AddByteOffset(ref AsRefIn(in source), byteOffset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T AddTypedOffsetAsReadOnly<T>(ref readonly T source, nint elementOffset)
        => ref AddTypedOffset(ref AsRefIn(in source), elementOffset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T AddTypedOffsetAsReadOnly<T>(ref readonly T source, nuint elementOffset)
        => ref AddTypedOffset(ref AsRefIn(in source), elementOffset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIntegerType<T>() => IsSignedIntegerType<T>() || IsUnsignedIntegerType<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIntegerType(Type type) => IsSignedIntegerType(type) || IsUnsignedIntegerType(type);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGreaterThan<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Cgt();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGreaterThanUnsigned<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Cgt_Un();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGreaterThanOrEquals<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Clt();
        IL.Emit.Ldc_I4_0();
        IL.Emit.Ceq();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGreaterThanOrEqualsUnsigned<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Clt_Un();
        IL.Emit.Ldc_I4_0();
        IL.Emit.Ceq();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLessThan<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Clt();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLessThanUnsigned<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Clt_Un();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLessThanOrEquals<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Cgt();
        IL.Emit.Ldc_I4_0();
        IL.Emit.Ceq();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLessThanOrEqualsUnsigned<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Cgt_Un();
        IL.Emit.Ldc_I4_0();
        IL.Emit.Ceq();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Ceq();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NotEquals<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Ceq();
        IL.Emit.Ldc_I4_0();
        IL.Emit.Ceq();
        return IL.Return<bool>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Or<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Or();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T And<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.And();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Xor<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Xor();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Negate<T>(T value)
    {
        IL.Push(value);
        IL.Emit.Neg();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Not<T>(T value)
    {
        IL.Push(value);
        IL.Emit.Not();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Add<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Add();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Subtract<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Sub();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Multiply<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Mul();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Divide<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Div();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DivideUnsigned<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Div_Un();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T LeftShift<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Shl();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T RightShift<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Shr();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T RightShiftUnsigned<T>(T a, T b)
    {
        IL.Push(a);
        IL.Push(b);
        IL.Emit.Shr_Un();
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Clamp<T>(T value, T min, T max)
        => Max(min, Min(max, value));

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ClampUnsigned<T>(T value, T min, T max)
        => MaxUnsigned(min, MinUnsigned(max, value));

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ClampUnordered<T>(T value, T a, T b)
        => Min(Max(a, b), Max(Min(a, b), value));

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ClampUnorderedUnsigned<T>(T value, T a, T b)
        => MinUnsigned(MaxUnsigned(a, b), MaxUnsigned(MinUnsigned(a, b), value));
}
