using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Structs {

    /// <summary>
    /// Struct that holds the data for a specified structure, used for loading and saving structures
    /// from files to generate in the world.
    /// </summary>
    public readonly struct StructureData : TagSerializable {
        public static readonly Func<TagCompound, StructureData> DESERIALIZER = Deserialize;

        public readonly int structureWidth;

        public readonly int structureHeight;

        public readonly List<List<TileData>> structureTileData;

        public StructureData(int structureWidth, int structureHeight, List<List<TileData>> structureTileData) {
            this.structureWidth = structureWidth;
            this.structureHeight = structureHeight;
            this.structureTileData = structureTileData;
        }

        public static StructureData Deserialize(TagCompound tag) {
            return new StructureData(
                tag.GetInt(nameof(structureWidth)),
                tag.GetInt(nameof(structureHeight)),
                tag.Get<List<List<TileData>>>(nameof(structureTileData))
            );
        }

        public TagCompound SerializeData() {
            return new TagCompound() {
                {nameof(structureWidth), structureWidth},
                {nameof(structureHeight), structureHeight},
                {nameof(structureTileData), structureTileData}
            };
        }
    }
}