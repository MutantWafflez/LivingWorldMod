using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Items
{
	public enum SlopeSetting
    {
		TYPE_CYCLE, SLOPE_TOP_RIGHT, TYPE_FLAT, SLOPE_BOTTOM_RIGHT, TYPE_REMOVE_WALL, SLOPE_BOTTOM_LEFT, TYPE_FULL, SLOPE_TOP_LEFT
    }

	public class ClockworkHammer : ModItem
	{
		public SlopeSetting SlopeSetting { get; set; } = SlopeSetting.TYPE_CYCLE;

		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Right click while holding to choose hammer preset");
		}

		public override void SetDefaults() {
			//item.CloneDefaults(ItemID.CopperHammer);
			item.damage = 20;
			item.melee = true;
			item.width = 60;
			item.height = 60;
			item.useTime = 15;
			item.useAnimation = 15;
			//item.hammer = 85;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = 7.5f;
			item.value = 105000;
			item.rare = ItemRarityID.LightRed;
			item.UseSound = mod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/ClockworkHammer");
			item.autoReuse = true;
		}
		
		// this marks this item to have an alternate / right-click action
		public override bool AltFunctionUse(Player player) {
			return true;
		}


		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			//recipe.AddIngredient(ItemID.Cog, 50);
			//recipe.AddIngredient(ItemID.GoldBar, 10);
			//recipe.AddIngredient(ItemID.Wire, 20);

			//recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

        public override bool CanUseItem(Player player)
        {
			//Console.WriteLine("Can use item check");
            if(player.altFunctionUse == 2)
            {
				//Console.WriteLine("showing hammer ui");
				if (LivingWorldMod.Instance.HasClockworkHammerUI())
					LivingWorldMod.Instance.CloseClockworkHammerUI();
				else
					LivingWorldMod.Instance.ShowClockworkHammerUI(this);
				return false; // pretend nothing happened
            }
			else
            {
				if (LivingWorldMod.Instance.HasClockworkHammerUI())
					return false; // no usage

				return true;
			}
        }

		public void ModifyTile(Tile tile)
        {
			switch(SlopeSetting)
            {
				case SlopeSetting.TYPE_CYCLE:
					byte curSlope = tile.slope();
					tile.slope((byte) ((curSlope + 1) % 6));
					break;
				case SlopeSetting.SLOPE_TOP_RIGHT:
					tile.slope(Tile.Type_SlopeUpRight);
					break;
				case SlopeSetting.TYPE_FLAT:
					tile.slope(Tile.Type_Halfbrick);
					break;
				case SlopeSetting.SLOPE_BOTTOM_RIGHT:
					tile.slope(Tile.Type_SlopeDownRight);
					break;
				case SlopeSetting.TYPE_REMOVE_WALL:
					
					break;
				case SlopeSetting.SLOPE_BOTTOM_LEFT:
					tile.slope(Tile.Type_SlopeDownLeft);
					break;
				case SlopeSetting.TYPE_FULL:
					tile.slope(Tile.Type_Solid);
					break;
				case SlopeSetting.SLOPE_TOP_LEFT:
					tile.slope(Tile.Type_SlopeUpLeft);
					break;
			}
        }
    }
}
