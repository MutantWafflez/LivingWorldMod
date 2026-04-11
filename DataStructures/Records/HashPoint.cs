using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.DataStructures.Records;

/// <summary>
///     Re-implementation of the <see cref="Point" /> type that has a better hash-code implementation, more re-usability with a generic definition, built-in geometry functions similar to
///     <see cref="Microsoft.Xna.Framework.Vector2" />, and conversions between pre-existing Xna types for use within Vanilla. This is strictly a vertex in 2-dimension space.
/// </summary>
[DebuggerDisplay("{DebugDisplayString,nq}")]
[Serializable]
public readonly record struct HashPoint<T>(T X, T Y) where T : struct, INumber<T> {
    public static HashPoint<T> NegativeOne {
        get;
    } = new (Unsafe.BitCast<int, T>(-1));

    public static HashPoint<T> Zero {
        get;
    } = new (Unsafe.BitCast<int, T>(0));

    private string DebugDisplayString => $"{X.ToString()} {Y.ToString()}";

    public HashPoint(T value) : this(value, value) { }

    public static HashPoint<T> operator +(HashPoint<T> point1, HashPoint<T> point2) => new (point1.X + point2.X, point1.Y + point2.Y);

    public static HashPoint<T> operator -(HashPoint<T> point1, HashPoint<T> point2) => new (point1.X - point2.X, point1.Y - point2.Y);

    public static HashPoint<T> operator *(HashPoint<T> point1, HashPoint<T> point2) => new (point1.X * point2.X, point1.Y * point2.Y);

    public static HashPoint<T> operator /(HashPoint<T> point1, HashPoint<T> point2) => new (point1.X / point2.X, point1.Y / point2.Y);

    public static explicit operator Point(HashPoint<T> hashPoint) => new (Unsafe.BitCast<T, int>(hashPoint.X), Unsafe.BitCast<T, int>(hashPoint.Y));

    public static explicit operator HashPoint<T>(Point point) => new (Unsafe.BitCast<int, T>(point.X), Unsafe.BitCast<int, T>(point.Y));

    public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode();

    public override string ToString() => $"{{X:{X.ToString()} Y:{Y.ToString()}}}";
}