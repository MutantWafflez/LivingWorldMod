using System;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.DataStatuctures.Structs;

/// <summary>
/// Not fully complete re-creation of the <see cref="Rectangle"/> struct with float precision instead
/// of int precision.
/// </summary>
public struct PreciseRectangle : IEquatable<PreciseRectangle> {
    public Vector2 position;
    public Vector2 size;

    /// <summary>
    /// Not fully complete re-creation of the <see cref="Rectangle"/> struct with float precision instead
    /// of int precision.
    /// </summary>
    public PreciseRectangle(Vector2 position, Vector2 size) {
        this.position = position;
        this.size = size;
    }

    public PreciseRectangle(Rectangle rectangle) : this(new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.Width, rectangle.Height)) { }

    public readonly float X => position.X;

    public readonly float Y => position.Y;

    public readonly float Width => size.X;

    public readonly float Height => size.Y;

    public readonly Vector2 Center => new(X + Width / 2f, Y + Height / 2f);

    public bool Equals(PreciseRectangle other) => position.Equals(other.position) && size.Equals(other.size);

    public override bool Equals(object obj) => obj is PreciseRectangle other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(position, size);

    public static bool operator ==(PreciseRectangle left, PreciseRectangle right) => left.Equals(right);

    public static bool operator !=(PreciseRectangle left, PreciseRectangle right) => !(left == right);
}