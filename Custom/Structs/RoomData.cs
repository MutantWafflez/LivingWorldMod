using System;
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

        public Point16 topDoorPos;

        public Point16 rightDoorPos;

        public Point16 leftDoorPos;

        public Point16 downDoorPos;

        public RoomData(StructureData roomLayout, Point16 topDoorPos, Point16 rightDoorPos, Point16 leftDoorPos, Point16 downDoorPos) {
            this.roomLayout = roomLayout;
            this.topDoorPos = topDoorPos;
            this.rightDoorPos = rightDoorPos;
            this.leftDoorPos = leftDoorPos;
            this.downDoorPos = downDoorPos;
        }

        public static RoomData Deserialize(TagCompound tag) => new RoomData(
            tag.Get<StructureData>(nameof(roomLayout)),
            tag.Get<Point16>(nameof(topDoorPos)),
            tag.Get<Point16>(nameof(rightDoorPos)),
            tag.Get<Point16>(nameof(leftDoorPos)),
            tag.Get<Point16>(nameof(downDoorPos))
        );

        public TagCompound SerializeData() => new TagCompound() {
            { nameof(roomLayout), roomLayout },
            { nameof(topDoorPos), topDoorPos },
            { nameof(rightDoorPos), rightDoorPos },
            { nameof(leftDoorPos), leftDoorPos },
            { nameof(downDoorPos), downDoorPos }
        };
    }
}