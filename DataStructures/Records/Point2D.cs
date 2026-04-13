using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace LivingWorldMod.DataStructures.Records;

/// <summary>
///     Re-implementation of the <see cref="Point" /> type that has a better hash-code implementation, more re-usability with a generic definition, built-in geometry functions, and conversions between
///     pre-existing XNA types for use within Vanilla. This is strictly a point in 2-dimensional space.
/// </summary>
/// <remarks>
///     This data structure is not intended as a complete replacement of <see cref="Point" />, <see cref="Point16" />, or <see cref="Vector2" />. It merely exists for scenarios where those types are
///     insufficient; otherwise those types are still prefered for code readability and usability within Vanilla.
/// </remarks>
[DebuggerDisplay("{DebugDisplayString,nq}")]
[Serializable]
public record struct Point2D<T>(T X, T Y) where T : struct, INumber<T> {
    public static Point2D<T> NegativeOne {
        get;
    } = new (T.CreateTruncating(-1));

    public static Point2D<T> Zero {
        get;
    } = new (T.CreateTruncating(0));

    private string DebugDisplayString => $"{X.ToString()} {Y.ToString()}";

    public Point2D(T value) : this(value, value) { }

    public static Point2D<T> operator +(Point2D<T> point, Point2D<T> point2) => new (point.X + point2.X, point.Y + point2.Y);

    public static Point2D<T> operator -(Point2D<T> point, Point2D<T> point2) => new (point.X - point2.X, point.Y - point2.Y);

    public static Point2D<T> operator *(Point2D<T> point, Point2D<T> point2) => new (point.X * point2.X, point.Y * point2.Y);

    public static Point2D<T> operator /(Point2D<T> point, Point2D<T> point2) => new (point.X / point2.X, point.Y / point2.Y);

    public static Point2D<T> operator +(Point2D<T> point, T scalar) => new (point.X + scalar, point.Y + scalar);

    public static Point2D<T> operator -(Point2D<T> point, T scalar) => new (point.X - scalar, point.Y - scalar);

    public static Point2D<T> operator *(Point2D<T> point, T scalar) => new (point.X * scalar, point.Y * scalar);

    public static Point2D<T> operator /(Point2D<T> point, T scalar) => new (point.X / scalar, point.Y / scalar);

    public static Point2D<T> operator %(Point2D<T> point, T scalar) => new (point.X % scalar, point.Y % scalar);

    public static explicit operator Vector2(Point2D<T> point) => new (float.CreateTruncating(point.X), float.CreateTruncating(point.Y));

    public static explicit operator Point2D<T>(Vector2 vector) => new (T.CreateTruncating(vector.X), T.CreateTruncating(vector.Y));

    public static explicit operator Point16(Point2D<T> point) => new (ushort.CreateTruncating(point.X), ushort.CreateTruncating(point.Y));

    public static explicit operator Point2D<T>(Point16 point) => new (T.CreateTruncating(point.X), T.CreateTruncating(point.Y));

    public static explicit operator Point(Point2D<T> point) => new (int.CreateTruncating(point.X), int.CreateTruncating(point.Y));

    public static explicit operator Point2D<T>(Point point) => new (T.CreateTruncating(point.X), T.CreateTruncating(point.Y));

    public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode();

    public override string ToString() => $"{{X:{X.ToString()} Y:{Y.ToString()}}}";

    /// <summary>
    ///     Creates a new instance of this point converted to the <see cref="TOther" /> type instead of <see cref="T" />.
    /// </summary>
    public readonly Point2D<TOther> Convert<TOther>() where TOther : struct, INumber<TOther> => new (TOther.CreateTruncating(X), TOther.CreateTruncating(Y));

    /// <summary>
    ///     Returns the distance between this point and another point in the form of a double, regardless of the type associated with this point.
    /// </summary>
    public readonly double Distance(Point2D<T> other) => Math.Sqrt(double.CreateTruncating(DistanceSquared(other)));

    /// <summary>
    ///     Returns the un-sqrt'd distance between this point and another, i.e. the distance squared.
    /// </summary>
    public readonly T DistanceSquared(Point2D<T> other) {
        Point2D<T> diff = this - other;
        return diff.X * diff.X + diff.Y * diff.Y;
    }

    /// <inheritdoc cref="ToWorldCoordinates(float, float)" />
    public readonly Vector2 ToWorldCoordinates(float adjustValue = 0f) => ToWorldCoordinates(adjustValue, adjustValue);

    /// <summary>
    ///     Creates a <see cref="Vector2" /> equivalent of this Point adjusted to its "world coordinates" equivalent, i.e. multiplying its value by <see cref="LWMUtils.TilePixelsSideLength" />. Also
    ///     incorporates adjustment arguments for small tweaks beyond it.
    /// </summary>
    public readonly Vector2 ToWorldCoordinates(float adjustX, float adjustY) => (Vector2)(this * T.CreateTruncating(LWMUtils.TilePixelsSideLength)) + new Vector2(adjustX, adjustY);

    /// <summary>
    ///     Creates a copy of this Point adjusted to its "tile coordinates" equivalent, i.e. diving its value by <see cref="LWMUtils.TilePixelsSideLength" />.
    /// </summary>
    public readonly Point2D<T> ToTileCoordinates() => this / T.CreateTruncating(LWMUtils.TilePixelsSideLength);
}