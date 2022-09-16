using System;
using System.Collections.Generic;
using LivingWorldMod.Custom.Enums;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Structs {
    /// <summary>
    /// "Extension" of the <seealso cref="StructureData"/> struct that has additional information for rooms in the
    /// Revamped Pyramid.
    /// </summary>
    public struct RoomData : TagSerializable {
        public static readonly Func<TagCompound, RoomData> DESERIALIZER = Deserialize;

        public StructureData roomLayout;

        public Dictionary<PyramidDoorDirection, Point16> doorData = new Dictionary<PyramidDoorDirection, Point16>();

        public byte gridWidth;

        public byte gridHeight;

        public RoomData(StructureData roomLayout, Point16 topDoorPos, Point16 rightDoorPos, Point16 downDoorPos, Point16 leftDoorPos, byte gridWidth, byte gridHeight) {
            this.roomLayout = roomLayout;
            doorData[PyramidDoorDirection.Top] = topDoorPos;
            doorData[PyramidDoorDirection.Right] = rightDoorPos;
            doorData[PyramidDoorDirection.Down] = downDoorPos;
            doorData[PyramidDoorDirection.Left] = leftDoorPos;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
        }

        public static RoomData Deserialize(TagCompound tag) => new RoomData(
            tag.Get<StructureData>(nameof(roomLayout)),
            tag.Get<Point16>("topDoorPos"),
            tag.Get<Point16>("rightDoorPos"),
            tag.Get<Point16>("downDoorPos"),
            tag.Get<Point16>("leftDoorPos"),
            tag.GetByte(nameof(gridWidth)),
            tag.GetByte(nameof(gridHeight))
        );

        public TagCompound SerializeData() => new TagCompound() {
            { nameof(roomLayout), roomLayout },
            { "topDoorPos", doorData[PyramidDoorDirection.Top] },
            { "rightDoorPos", doorData[PyramidDoorDirection.Right] },
            { "downDoorPos", doorData[PyramidDoorDirection.Down] },
            { "leftDoorPos", doorData[PyramidDoorDirection.Left] },
            { nameof(gridWidth), gridWidth },
            { nameof(gridHeight), gridHeight }
        };
    }
}