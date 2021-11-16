using System;
using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Classes {

    /// <summary>
    /// Class that holds data on a given Waystone, including position and type.
    /// </summary>
    public class WaystoneInfo : TagSerializable {

        public static readonly Func<TagCompound, WaystoneInfo> DESERIALIZER = Deserialize;

        /// <summary>
        /// Actual position of the tile, for adding, removing, and handling.
        /// </summary>
        public Point16 tileLocation;

        /// <summary>
        /// Location of the tile to place the icon on the map.
        /// </summary>
        public Vector2 iconLocation;

        /// <summary>
        /// This waystone's type.
        /// </summary>
        public WaystoneType waystoneType;

        /// <summary>
        /// Whether or not this waystone is active.
        /// </summary>
        public bool isActivated;

        public WaystoneInfo(Point16 tileLocation, WaystoneType waystoneType, bool isActivated) {
            this.tileLocation = tileLocation;
            this.iconLocation = tileLocation.ToVector2() + new Vector2(1f, 1.5f);
            this.waystoneType = waystoneType;
            this.isActivated = isActivated;
        }

        public TagCompound SerializeData() {
            return new TagCompound() {
                { "location", tileLocation },
                { "type", waystoneType },
                { "isActive", isActivated }
            };
        }

        public static WaystoneInfo Deserialize(TagCompound tag) {
            return new WaystoneInfo(tag.Get<Point16>("location"), (WaystoneType)tag.GetAsInt("type"), tag.GetBool("isActive"));
        }
    }
}
