namespace LivingWorldMod.Utilities
{
	public class ShopItem
	{
		public readonly int itemId;
		public int stackSize;

		public ShopItem(int itemId, int stackSize)
		{
			this.itemId = itemId;
			this.stackSize = stackSize;
		}
	}
}