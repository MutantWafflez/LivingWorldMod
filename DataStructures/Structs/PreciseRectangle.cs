using System;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.DataStructures.Structs;

/// <summary>
///     Not fully complete re-creation of the <see cref="Rectangle" /> struct with float precision instead
///     of int precision.
/// </summary>
public struct PreciseRectangle : IEquatable<PreciseRectangle> {
    public Vector2 position;
    public Vector2 size;

    public readonly float X => position.X;

    public readonly float Y => position.Y;

    public readonly float Width => size.X;

    public readonly float Height => size.Y;

    public readonly Vector2 Center => new(X + Width / 2f, Y + Height / 2f);

    public readonly Vector2 TopRight => position + new Vector2(size.X, 0);

    public readonly Vector2 BottomLeft => position + new Vector2(0, size.Y);

    public readonly Vector2 BottomRight => position + new Vector2(size.X, size.Y);

    /// <summary>
    ///     Not fully complete re-creation of the <see cref="Rectangle" /> struct with float precision instead
    ///     of int precision.
    /// </summary>
    public PreciseRectangle(Vector2 position, Vector2 size) {
        this.position = position;
        this.size = size;
    }

    public PreciseRectangle(Rectangle rectangle) : this(new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.Width, rectangle.Height)) { }

    public static bool operator ==(PreciseRectangle left, PreciseRectangle right) => left.Equals(right);

    public static bool operator !=(PreciseRectangle left, PreciseRectangle right) => !(left == right);

    public override bool Equals(object obj) => obj is PreciseRectangle other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(position, size);

    public readonly Rectangle ToWorldCoordinates() => new(
        (int)(position.X / 16f),
        (int)(position.Y / 16f),
        (int)(size.X / 16f),
        (int)(size.Y / 16f)
    );

    public bool Equals(PreciseRectangle other) => position.Equals(other.position) && size.Equals(other.size);
}