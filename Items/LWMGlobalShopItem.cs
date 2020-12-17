using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LivingWorldMod.Items
{
	public class LWMGlobalShopItem : GlobalItem
	{
		/// <summary>
		/// If true, an X is drawn over the sprite in PostDrawInInventory.
		/// This is set in a PlayerHook after buying the last of an item.
		/// </summary>
		public bool isOutOfStock = false;

		/// <summary>
		/// Used to differentiate shop slots that were there upon opening the UI, and those that were created by selling items to an NPC. Set to false explicitly for sold items in the ModPlayer hook.
		/// </summary>
		public bool isOriginalShopSlot = true;

		public override bool InstancePerEntity => true;

		public override void SetDefaults(Item item)
		{
			// probably not necessary or smart to set this for every single item in the game
			//item.buyOnce = false;
		}

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			LWMGlobalShopItem myItem = (LWMGlobalShopItem) base.Clone(item, itemClone);
			myItem.isOutOfStock = isOutOfStock;
			myItem.isOriginalShopSlot = isOriginalShopSlot;
			return myItem;
		}

		public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor,
			Vector2 origin, float scale)
		{
			if(isOutOfStock)
				spriteBatch.Draw(ModContent.GetTexture("Terraria/CoolDown"), position, frame, drawColor, 0, origin, scale, SpriteEffects.None, 0);
		}
		
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			// buyOnce is only true in the shop window, so we don't want to modify the tooltips outside of that
			// also, for out of stock items, there is also no need for modification
			if (!isOriginalShopSlot || isOutOfStock) return;
            
			TooltipLine line = tooltips.FirstOrDefault(l => l.Name == "Price");
			if (line != null)
			{
				// construct text
				int copper = item.value % 100;
				int silver = (item.value / 100) % 100;
				int gold = (item.value / 100_00) % 100;
				int platinum = (item.value / 100_00_00) % 100;
				Color color;
				if (platinum > 0) color = Colors.CoinPlatinum;
				else if (gold > 0) color = Colors.CoinGold;
				else if (silver > 0) color = Colors.CoinSilver;
				else color = Colors.CoinCopper;
				line.overrideColor = color;
				line.text = "Buy Price: ";
				if (platinum > 0)
					line.text += platinum + " Platinum ";
				if (gold > 0)
					line.text += gold + " Gold ";
				if (silver > 0)
					line.text += silver + " Silver ";
				if (copper > 0)
					line.text += copper + " Copper ";
				/*if (platinum > 0)
					line.text += "[c/" + Colors.AlphaDarken(Colors.CoinPlatinum).Hex3() + ":" + platinum + " " + Language.GetText("Platinum").Value + "] ";
				if (gold > 0)
					line.text += "[c/" + Colors.AlphaDarken(Colors.CoinGold).Hex3() + ":" + gold + " " + Language.GetText("Gold").Value + "] ";
				if (silver > 0)
					line.text += "[c/" + Colors.AlphaDarken(Colors.CoinSilver).Hex3() + ":" + silver + " " + Language.GetText("Silver").Value + "] ";
				if (copper > 0)
					line.text += "[c/" + Colors.AlphaDarken(Colors.CoinCopper).Hex3() + ":" + copper + " " + Language.GetText("Copper").Value + "] ";*/
			}
		}
	}
}