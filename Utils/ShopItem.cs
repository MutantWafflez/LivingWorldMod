using System;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Utilities
{
	public class ShopItem : TagSerializable
	{
		public static readonly Func<TagCompound, ShopItem> DESERIALIZER = Load;

		public readonly int itemId;
		public int stackSize;

		public ShopItem(int itemId, int stackSize)
		{
			this.itemId = itemId;
			this.stackSize = stackSize;
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
    }
}