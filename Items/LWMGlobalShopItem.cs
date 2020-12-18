using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Items
{
	/// <summary>
	/// This class is used for Shop items that have a limited stock. The fields are first set in Villager.SetupShop(),
	/// and then managed further later on in some buy/sell ModPlayer hooks. 
	/// </summary>
	public class LWMGlobalShopItem : GlobalItem
	{
		/// <summary>
		/// If true, an X is drawn over the sprite in PostDrawInInventory.
		/// This is set in a PlayerHook after buying the last of an item.
		/// </summary>
		public bool isOutOfStock;

		/// <summary>
		/// Used to differentiate shop slots that were there upon opening the UI, and those that were created by selling items to an NPC. Set to true when setting up shop.
		/// </summary>
		public bool isOriginalShopSlot;

		/// <summary>
		/// This is the ShopItem instance stored in the dailyShop array of a Villager. Keeping a reference to it
		/// means that in the buy/sell ModPlayer hooks, we can decrement the stack size stored in the Villager's shop
		/// array, so that next time the shop is opened, the remaining stock persists.
		/// </summary>
		private ShopItem dailyStock;

		#region Instance Management
		
		public override bool InstancePerEntity => true;
		
		public override void SetDefaults(Item item)
		{
			// probably not necessary or smart to set this for every single item in the game
			//item.buyOnce = false;
			isOutOfStock = false;
			isOriginalShopSlot = false;
		}
		
		#endregion Instance Management

		// called on opening a shop window
		public void SetPersistentStack(ShopItem dailyStock)
		{
			this.dailyStock = dailyStock;
		}
		
		// called on purchase
		public void UpdateInventory(int stackSize)
		{
			dailyStock.stackSize = stackSize;
		}
		
		#region Display Management

		public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor,
			Vector2 origin, float scale)
		{
			if (isOutOfStock)
			{
				Texture2D itemTexture = Main.itemTexture[item.type];
				Texture2D overTexture = ModContent.GetTexture("Terraria/CoolDown");
				position.X += itemTexture.Width / 2f * scale;
				position.X -= overTexture.Width / 2f;
				position.Y += itemTexture.Height / 2f * scale;
				position.Y -= overTexture.Height / 2f;
				spriteBatch.Draw(overTexture, position, drawColor);
			}
		}
		
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			// don't modify tooltips of non-shop slots or slots with only one item (includes out of stock)
			if (!isOriginalShopSlot || item.stack <= 1)
				return;
            
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
				// line.text = item.stack > 1 ? "Buy Price (per item): " : "Buy Price: ";
				line.text = "Buy Price: ";
				if (platinum > 0)
					line.text += platinum + " platinum ";
				if (gold > 0)
					line.text += gold + " gold ";
				if (silver > 0)
					line.text += silver + " silver ";
				if (copper > 0)
					line.text += copper + " copper ";
				/*if (platinum > 0)
					line.text += "[c/" + Colors.AlphaDarken(Colors.CoinPlatinum).Hex3() + ":" + platinum + " " + Language.GetText("Platinum").Value + "] ";
				if (gold > 0)
					line.text += "[c/" + Colors.AlphaDarken(Colors.CoinGold).Hex3() + ":" + gold + " " + Language.GetText("Gold").Value + "] ";
				if (silver > 0)
					line.text += "[c/" + Colors.AlphaDarken(Colors.CoinSilver).Hex3() + ":" + silver + " " + Language.GetText("Silver").Value + "] ";
				if (copper > 0)
					line.text += "[c/" + Colors.AlphaDarken(Colors.CoinCopper).Hex3() + ":" + copper + " " + Language.GetText("Copper").Value + "] ";*/
				if(item.stack > 1)
					line.text += "per item";
			}
		}
		
		#endregion Display Management
	}
}