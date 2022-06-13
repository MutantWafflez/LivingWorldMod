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

        private static Circle Deserialize(TagCompound tag) => new Circle(
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

        public TagCompound SerializeData() => new TagCompound() {
            { nameof(center), center },
            { nameof(radius), radius }
        };

        public static bool operator ==(Circle first, Circle second) => first.center == second.center && first.radius == second.radius;

        public static bool operator !=(Circle first, Circle second) => !(first == second);
    }
}