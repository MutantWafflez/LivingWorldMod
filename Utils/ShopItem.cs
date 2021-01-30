using System;
using System.IO;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Utilities
{
    public class ShopItem : TagSerializable, BinarySerializable
    {
        public static readonly Func<TagCompound, ShopItem> DESERIALIZER = Load;

        public int itemId { get; private set; }
        public int stackSize;

        public ShopItem() {}
        public ShopItem(int itemId, int stackSize)
        {
            this.itemId = itemId;
            this.stackSize = stackSize;
        }

        public ShopItem Clone()
        {
            return new ShopItem(itemId, stackSize);
        }

        public TagCompound SerializeData()
        {
            return new TagCompound
            {
                {"itemid", itemId},
                {"stack", stackSize}
            };
        }

        public static ShopItem Load(TagCompound tag)
        {
            return new ShopItem(tag.GetInt("itemid"), tag.GetInt("stack"));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(itemId);
            writer.Write(stackSize);
        }

        public void Read(BinaryReader reader)
        {
            itemId = reader.ReadInt32();
            stackSize = reader.ReadInt32();
        }
    }
}