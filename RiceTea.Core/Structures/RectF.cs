using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Structures;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct RectF : IEquatable<RectF>
{
    public static readonly RectF Empty = default;
    public static readonly RectF Infinite = new RectF(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity);

    public float Left;
    public float Top;
    public float Right;
    public float Bottom;

    public RectF(in RectF original) : this(original.Left, original.Top, original.Right, original.Bottom) { }

    public RectF(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF FromXYWH(PointF location, SizeF size) => FromXYWH(location.X, location.Y, size.Width, size.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF FromXYWH(float x, float y, float width, float height) => new RectF(x, y, x + width, y + height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator RectF(RectangleF rectangle) => new RectF(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator RectF(Rectangle rectangle) => new RectF(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator RectF(Rect rectangle) => new RectF(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator RectangleF(RectF rectangle) => RectangleF.FromLTRB(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Rectangle(RectF rectangle) => Rectangle.FromLTRB((int)rectangle.Left, (int)rectangle.Top, (int)rectangle.Right, (int)rectangle.Bottom);

    public float X
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Left;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Left = value;
    }

    public float Y
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Top;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Top = value;
    }

    public PointF Location
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => new PointF(Left, Top);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            Left = value.X;
            Top = value.Y;
        }
    }

    public SizeF Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => new SizeF(Width, Height);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    public readonly PointF TopLeft => new PointF(Left, Top);
    public readonly PointF TopRight => new PointF(Right, Top);
    public readonly PointF BottomLeft => new PointF(Left, Bottom);
    public readonly PointF BottomRight => new PointF(Right, Bottom);

    public float Height
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Bottom - Top;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Bottom = Top + value;
    }

    public float Width
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Offset(PointF point) => Offset(point.X, point.Y);

    public void Offset(float x, float y)
    {
        Left += x;
        Right += x;
        Top += y;
        Bottom += y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OffsetNegative(PointF point) => OffsetNegative(point.X, point.Y);

    public void OffsetNegative(float x, float y)
    {
        Left -= x;
        Right -= x;
        Top -= y;
        Bottom -= y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(int x, int y) => x >= Left && y >= Top && x <= Right && y <= Bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(float x, float y) => x >= Left && y >= Top && x <= Right && y <= Bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(Point point) => Contains(point.X, point.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(PointF point) => Contains(point.X, point.Y);

    public readonly bool Contains(in Rectangle rect)
    {
        if (X <= rect.X && rect.X + rect.Width <= Right && Y <= rect.Y)
        {
            return rect.Y + rect.Height <= Bottom;
        }

        return false;
    }

    public readonly bool Contains(in RectangleF rect)
    {
        if (X <= rect.X && rect.X + rect.Width <= Right && Y <= rect.Y)
        {
            return rect.Y + rect.Height <= Bottom;
        }

        return false;
    }

    public readonly bool Contains(in RectF rect)
    {
        if (X <= rect.X && rect.Right <= Right && Y <= rect.Y)
        {
            return rect.Bottom <= Bottom;
        }

        return false;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out float left, out float top, out float right, out float bottom)
    {
        left = Left;
        top = Top;
        right = Right;
        bottom = Bottom;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals(object? obj) => obj is RectF other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe bool Equals(in RectF other) => EqualsCore(this, other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe bool Equals(RectF other) => EqualsCore(this, other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool EqualsCore(in RectF a, in RectF b)
    {
        fixed (RectF* ptr = &a, ptr2 = &b)
        {
#if NET8_0_OR_GREATER
            if (Limits.UseVector128())
                return System.Runtime.Intrinsics.Vector128.Load((float*)ptr) == System.Runtime.Intrinsics.Vector128.Load((float*)ptr2);
#else
            if (Limits.UseVector())
            {        
                const int V128ByteCount = 128 / 8;

                switch (System.Numerics.Vector<byte>.Count)
                {
                    case V128ByteCount:
                        return _V128((byte*)ptr, (byte*)ptr2);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool _V128(byte* ptr, byte* ptr2)
                    => (UnsafeHelper.Read<System.Numerics.Vector<float>>(ptr) == UnsafeHelper.Read<System.Numerics.Vector<float>>(ptr2));
            }
#endif
        }

        return a.Left == b.Left && a.Top == b.Top && a.Right == b.Right && a.Bottom == b.Bottom;
    }
    public override readonly unsafe int GetHashCode()
    {
        float left = Left, top = Top, right = Right, bottom = Bottom;
        return *(int*)&left ^ *(int*)&top ^ *(int*)&right ^ *(int*)&bottom;
    }

    public override readonly string ToString()
        => $"{{ {nameof(Left)} = {Left}, {nameof(Top)} = {Top}, {nameof(Right)} = {Right}, {nameof(Bottom)} = {Bottom}) }}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in RectF a, in RectF b) => a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in RectF a, in RectF b) => !a.Equals(b);
}
