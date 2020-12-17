using LivingWorldMod.Items;
using Terraria;

namespace LivingWorldMod.Utilities
{
	// haven't used this yet, but this is what I'm thinking of doing for defining each store item for a slot for a day
	public struct ShopItem
	{
		public readonly int itemId;
		public int stackSize;

		public ShopItem(int itemId, int stackSize)
		{
			this.itemId = itemId;
			this.stackSize = stackSize;
		}
	}
	
	public class DailyShopData
	{

		private ShopItem[] currentShop;
		
		public DailyShopData(ShopItem[] currentShop)
		{
			this.currentShop = currentShop;
		}

		public void SetupShop(Chest shop, ref int nextSlot)
		{
			foreach (ShopItem itemSlot in currentShop)
			{
				Item item = shop.item[nextSlot];
				item.SetDefaults(itemSlot.itemId);
				item.stack = itemSlot.stackSize;
				item.buyOnce = true;
				LWMGlobalShopItem globalItem = item.GetGlobalItem<LWMGlobalShopItem>();
				globalItem.shopInstance = this;
				globalItem.isOriginalShopSlot = true;
				globalItem.isOutOfStock = item.stack <= 0;
				// set stack size to 1 so it doesn't remove the item completely
				if (item.stack <= 0) item.stack = 1;
				++nextSlot;
			}
		}

		// called on purchase
		public void UpdateInventory(int itemIndex, int stackSize)
		{
			if (itemIndex < currentShop.Length)
				currentShop[itemIndex].stackSize = stackSize;
		}
	}
}