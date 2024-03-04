using System;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.DataStructures.Structs;

/// <summary>
/// Relatively simple Circle structure, that explains what it does on the tin.
/// </summary>
public struct Circle : TagSerializable, IEquatable<Circle> {
    public static readonly Func<TagCompound, Circle> DESERIALIZER = Deserialize;

    private static Circle Deserialize(TagCompound tag) => new(
        tag.Get<Vector2>(nameof(center)),
        tag.GetFloat(nameof(radius))
    );

    public Vector2 center;
    public float radius;

    public Circle(Vector2 center, float radius) {
        this.center = center;
        this.radius = radius;
    }

    /// <summary>
    /// Returns whether or not the passed in point is within this Circle's radius.
    /// </summary>
    public readonly bool ContainsPoint(Vector2 point) => Math.Abs(center.Distance(point)) < radius;

    public readonly bool ContainsRectangle(Rectangle rect) => ContainsPoint(rect.TopLeft()) && ContainsPoint(rect.TopRight()) && ContainsPoint(rect.BottomLeft()) && ContainsPoint(rect.BottomRight());

    /// <summary>
    /// Creates a copy of this Circle equivalent in tile coordinates, assuming that this circle is in world coordinates.
    /// Do note a potential loss of precision with the radius if it is not divisible by 16.
    /// </summary>
    public readonly Circle ToTileCoordinates() => new(center.ToTileCoordinates().ToVector2(), (int)(radius / 16f));

    /// <summary>
    /// Creates a copy of this Circle equivalent in world coordinates, assuming that this circle is in tile coordinates.
    /// </summary>
    public readonly Circle ToWorldCoordinates() => new(center.ToWorldCoordinates(), radius * 16f);

    /// <summary>
    /// Uses this Circle's data in order to create a Rectangle (square) that has each edge being tangential
    /// to one side of the Circle (AKA, a "hitbox" for the circle).
    /// </summary>
    public readonly Rectangle ToRectangle() => new((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2));

    public readonly TagCompound SerializeData() => new() {
        { nameof(center), center },
        { nameof(radius), radius }
    };

    public bool Equals(Circle other) => center.Equals(other.center) && radius.Equals(other.radius);

    public override bool Equals(object obj) => obj is Circle other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(center, radius);

    public static bool operator ==(Circle left, Circle right) => left.Equals(right);

    public static bool operator !=(Circle left, Circle right) => !left.Equals(right);
}