using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.DataStructures.Records;

/// <summary>
///     Re-implementation of the <see cref="Rectangle" /> type that has a better hash-code implementation, more re-usability with a generic definition, built-in geometry functions, and conversions
///     between
///     pre-existing XNA types for use within Vanilla. This is strictly a rectangle in 2-dimensional space.
/// </summary>
/// <remarks>
///     Similarly to <see cref="Point2D{T}" />, this type is not intended to replace <see cref="Rectangle" />, but use it in scenarios that the XNA type is deficient.
/// </remarks>
[DebuggerDisplay("{DebugDisplayString,nq}")]
[Serializable]
public readonly record struct Rectangle2D<T>(T X, T Y, T Width, T Height) where T : struct, INumber<T> {
    /// <summary>
    ///     Represents an "Empty" rectangle that has no position in space or size (all zero).
    /// </summary>
    public static readonly Rectangle2D<T> Empty = new(T.CreateTruncating(0), T.CreateTruncating(0), T.CreateTruncating(0), T.CreateTruncating(0));

    public static readonly Rectangle2D<T> NegativeOne = new(T.CreateTruncating(-1), T.CreateTruncating(-1), T.CreateTruncating(0), T.CreateTruncating(0));

    public T Right => X + Width;

    public T Bottom => Y + Height;

    public Point2D<T> TopLeft => new(X, Y);

    public Point2D<T> BottomRight => new(Right, Bottom);

    public Point2D<T> Center => new(X + Width / T.CreateTruncating(2), Y + Height / T.CreateTruncating(2));

    public Point2D<T> Size => new(Width, Height);

    private string DebugDisplayString => $"{X.ToString()} {Y.ToString()} {Width.ToString()} {Height.ToString()}";

    // Can't extend the default empty constructor
    public Rectangle2D(Point2D<T> cornerOne, Point2D<T> cornerTwo) : this(T.CreateTruncating(0), T.CreateTruncating(0), T.CreateTruncating(0), T.CreateTruncating(0)) {
        T minX = T.Min(cornerOne.X, cornerTwo.X);
        T maxX = T.Max(cornerOne.X, cornerTwo.X);
        T minY = T.Min(cornerOne.Y, cornerTwo.Y);
        T maxY = T.Max(cornerOne.Y, cornerTwo.Y);

        X = minX;
        Y = minY;
        Width = maxX - minX;
        Height = maxY - minY;
    }

    public static explicit operator Rectangle(Rectangle2D<T> rectangle) => new(
        int.CreateTruncating(rectangle.X),
        int.CreateTruncating(rectangle.Y),
        int.CreateTruncating(rectangle.Width),
        int.CreateTruncating(rectangle.Height)
    );

    public static explicit operator Rectangle2D<T>(Rectangle rectangle) => new(
        T.CreateTruncating(rectangle.X),
        T.CreateTruncating(rectangle.Y),
        T.CreateTruncating(rectangle.Width),
        T.CreateTruncating(rectangle.Height)
    );
    
    public static Rectangle2D<T> operator +(Rectangle2D<T> rect, Point2D<T> point) => rect with { X = rect.X + point.X, Y = rect.Y + point.Y };

    public static Rectangle2D<T> operator -(Rectangle2D<T> rect, Point2D<T> point) => rect with { X = rect.X - point.X, Y = rect.Y - point.Y };

    public override string ToString() => $"{{X:{X.ToString()} Y:{Y.ToString()} Width:{Width.ToString()} Height:{Height.ToString()}}}";

    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    public bool Contains(Point2D<T> point) => X <= point.X && point.X < X + Width && Y <= point.Y && point.Y < Y + Height;

    public bool Contains(Rectangle2D<T> other) => X <= other.X && other.X + other.Width <= X + Width && Y <= other.Y && other.Y + other.Height <= Y + Height;

    public Rectangle2D<T> Inflate(T horizontalValue, T verticalValue) => new(
        X - horizontalValue,
        Y - verticalValue,
        Width + horizontalValue * T.CreateTruncating(2),
        Height + verticalValue * T.CreateTruncating(2)
    );

    public bool Intersects(Rectangle2D<T> other) => other.X < Right && X < other.Right && other.Y < Bottom && Y < other.Bottom;

    /// <summary>
    ///     Creates a copy of this rectangle in World Coordinates, i.e. all fields multiplied by 16.
    /// </summary>
    public Rectangle2D<T> ToWorldCoordinates() => new(
        X * T.CreateTruncating(LWMUtils.TilePixelsSideLength),
        Y * T.CreateTruncating(LWMUtils.TilePixelsSideLength),
        Width * T.CreateTruncating(LWMUtils.TilePixelsSideLength),
        Height * T.CreateTruncating(LWMUtils.TilePixelsSideLength)
    );
}