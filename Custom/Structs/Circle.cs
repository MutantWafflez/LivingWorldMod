using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Structs {
    /// <summary>
    /// Relatively simple Circle structure, that explains what it does on the tin.
    /// </summary>
    public struct Circle : TagSerializable {
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
        public bool ContainsPoint(Vector2 point) => Math.Abs(center.Distance(point)) < radius;

        /// <summary>
        /// Creates a copy of this Circle equivalent in tile coordinates, assuming that this circle is in world coordinates.
        /// Do note a potential loss of precision with the radius if it is not divisible by 16.
        /// </summary>
        public Circle ToTileCoordinates() => new(center.ToTileCoordinates().ToVector2(), (int)(radius / 16f));

        /// <summary>
        /// Creates a copy of this Circle equivalent in world coordinates, assuming that this circle is in tile coordinates.
        /// </summary>
        public Circle ToWorldCoordinates() => new(center.ToWorldCoordinates(), radius * 16f);

        /// <summary>
        /// Uses this Circle's data in order to create a Rectangle (square) that has each edge being tangential
        /// to one side of the Circle (AKA, a "hitbox" for the circle).
        /// </summary>
        public Rectangle ToRectangle() => new((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2));

        public TagCompound SerializeData() => new() {
            { nameof(center), center },
            { nameof(radius), radius }
        };

        public static bool operator ==(Circle first, Circle second) => first.center == second.center && first.radius == second.radius;

        public static bool operator !=(Circle first, Circle second) => !(first == second);
    }
}