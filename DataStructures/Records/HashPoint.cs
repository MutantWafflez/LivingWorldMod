using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace LivingWorldMod.DataStructures.Records;

/// <summary>
///     Re-implementation of the <see cref="Point" /> type that has a better hash-code implementation, more re-usability with a generic definition, built-in geometry functions that reference
///     <see cref="Microsoft.Xna.Framework.Vector2" />, and conversions between pre-existing Xna types for use within Vanilla. This is strictly a vertex in 2-dimensional space.
/// </summary>
[DebuggerDisplay("{DebugDisplayString,nq}")]
[Serializable]
public record struct HashPoint<T>(T X, T Y) where T : struct, INumber<T> {
    public static HashPoint<T> NegativeOne {
        get;
    } = new (T.CreateTruncating(-1));

    public static HashPoint<T> Zero {
        get;
    } = new (T.CreateTruncating(0));

    private string DebugDisplayString => $"{X.ToString()} {Y.ToString()}";

    public HashPoint(T value) : this(value, value) { }

    public static HashPoint<T> operator +(HashPoint<T> point1, HashPoint<T> point2) => new (point1.X + point2.X, point1.Y + point2.Y);

    public static HashPoint<T> operator -(HashPoint<T> point1, HashPoint<T> point2) => new (point1.X - point2.X, point1.Y - point2.Y);

    public static HashPoint<T> operator *(HashPoint<T> point1, HashPoint<T> point2) => new (point1.X * point2.X, point1.Y * point2.Y);

    public static HashPoint<T> operator /(HashPoint<T> point1, HashPoint<T> point2) => new (point1.X / point2.X, point1.Y / point2.Y);

    public static explicit operator Vector2(HashPoint<T> hashPoint) => new (float.CreateTruncating(hashPoint.X), float.CreateTruncating(hashPoint.Y));

    public static implicit operator Point(HashPoint<T> hashPoint) => new (int.CreateTruncating(hashPoint.X), int.CreateTruncating(hashPoint.Y));

    public static implicit operator HashPoint<T>(Point point) => new (T.CreateTruncating(point.X), T.CreateTruncating(point.Y));

    public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode();

    public override string ToString() => $"{{X:{X.ToString()} Y:{Y.ToString()}}}";

    /// <summary>
    ///     Returns the distance between this point and another point in the form of a double, regardless of the type associated with this point.
    /// </summary>
    public double Distance(HashPoint<T> other) => Math.Sqrt(double.CreateTruncating(DistanceSquared(other)));

    /// <summary>
    ///     Returns the un-sqrt'd distance between this point and another, i.e. the distance squared.
    /// </summary>
    public T DistanceSquared(HashPoint<T> other) {
        HashPoint<T> diff = this - other;
        return diff.X * diff.X + diff.Y * diff.Y;
    }
}