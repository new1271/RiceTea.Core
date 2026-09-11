using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Structures;

public struct RectU : IEquatable<RectU>
{
    public uint Left;
    public uint Top;
    public uint Right;
    public uint Bottom;

    public RectU(in RectU original) : this(original.Left, original.Top, original.Right, original.Bottom) { }

    public RectU(uint left, uint top, uint right, uint bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectU FromXYWH(PointU location, SizeU size) => FromXYWH(location.X, location.Y, size.Width, size.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectU FromXYWH(uint x, uint y, uint width, uint height) => new RectU(x, y, x + width, y + height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator RectU(RectangleF rectangle) => new RectU((uint)rectangle.Left, (uint)rectangle.Top, (uint)rectangle.Right, (uint)rectangle.Bottom);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator RectU(Rectangle rectangle) => FromXYWH(MathHelper.MakeUnsigned(rectangle.Left), MathHelper.MakeUnsigned(rectangle.Top),
        MathHelper.MakeUnsigned(rectangle.Right), MathHelper.MakeUnsigned(rectangle.Bottom));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator RectU(Rect rectangle) => new RectU(MathHelper.MakeUnsigned(rectangle.Left), MathHelper.MakeUnsigned(rectangle.Top),
        MathHelper.MakeUnsigned(rectangle.Right), MathHelper.MakeUnsigned(rectangle.Bottom));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator RectangleF(RectU rectangle) => RectangleF.FromLTRB(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Rectangle(RectU rectangle) => Rectangle.FromLTRB((int)rectangle.Left, (int)rectangle.Top, (int)rectangle.Right, (int)rectangle.Bottom);

    public uint X
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Left;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Left = value;
    }

    public uint Y
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Top;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Top = value;
    }

    public PointU Location
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => new PointU(Left, Top);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            Left = value.X;
            Top = value.Y;
        }
    }

    public SizeU Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => new SizeU(Width, Height);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    public readonly PointU TopLeft => new PointU(Left, Top);
    public readonly PointU TopRight => new PointU(Right, Top);
    public readonly PointU BottomLeft => new PointU(Left, Bottom);
    public readonly PointU BottomRight => new PointU(Right, Bottom);

    public uint Height
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Bottom - Top;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Bottom = Top + value;
    }

    public uint Width
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Right - Left;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Right = Left + value;
    }

    public readonly bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => IsEmptyLocation && IsEmptySize;
    }

    public readonly bool IsEmptyLocation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Left == 0 && Top == 0;
    }

    public readonly bool IsEmptySize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Left == Right && Top == Bottom;
    }

    public readonly bool IsValid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Left < Right && Top < Bottom;
    }

    public readonly bool Contains(int x, int y) => x >= Left && y >= Top && x <= Right && y <= Bottom;

    public readonly bool Contains(uint x, uint y) => x >= Left && y >= Top && x <= Right && y <= Bottom;

    public readonly bool Contains(PointU point) => Contains(point.X, point.Y);

    public readonly bool Contains(Point point) => Contains(point.X, point.Y);

    public readonly bool Contains(in RectangleF rect)
    {
        if (X <= rect.X && rect.X + rect.Width <= Right && Y <= rect.Y)
        {
            return rect.Y + rect.Height <= Bottom;
        }

        return false;
    }

    public readonly bool Contains(in Rectangle rect)
    {
        if (X <= rect.X && rect.X + rect.Width <= Right && Y <= rect.Y)
        {
            return rect.Y + rect.Height <= Bottom;
        }

        return false;
    }

    public readonly bool Contains(in RectU rect)
    {
        if (X <= rect.X && rect.Right <= Right && Y <= rect.Y)
        {
            return rect.Bottom <= Bottom;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out uint left, out uint top, out uint right, out uint bottom)
    {
        left = Left;
        top = Top;
        right = Right;
        bottom = Bottom;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals(object? obj) => obj is RectU other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe bool Equals(in RectU other) => EqualsCore(this, other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe bool Equals(RectU other) => EqualsCore(this, other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool EqualsCore(in RectU a, in RectU b)
    {
        const int V128ByteCount = 128 / 8;

        fixed (RectU* ptr = &a, ptr2 = &b)
        {
#if NET8_0_OR_GREATER
            if (Limits.UseVector128())
                return System.Runtime.Intrinsics.Vector128.Load((uint*)ptr) == System.Runtime.Intrinsics.Vector128.Load((uint*)ptr2);
#else
            if (Limits.UseVector())
            {
                switch (System.Numerics.Vector<byte>.Count)
                {
                    case V128ByteCount:
                        return _V128((byte*)ptr, (byte*)ptr2);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool _V128(byte* ptr, byte* ptr2)
                    => UnsafeHelper.Read<System.Numerics.Vector<uint>>(ptr) == UnsafeHelper.Read<System.Numerics.Vector<uint>>(ptr2);
            }
#endif      

            return SequenceHelper.Equals(ptr, ptr2, V128ByteCount);
        }
    }

    public override readonly int GetHashCode() => unchecked((int)(Left ^ Top ^ Right ^ Bottom));

    public override readonly string ToString()
        => $"{{ {nameof(Left)} = {Left}, {nameof(Top)} = {Top}, {nameof(Right)} = {Right}, {nameof(Bottom)} = {Bottom}) }}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in RectU a, in RectU b) => a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in RectU a, in RectU b) => !a.Equals(b);
}
