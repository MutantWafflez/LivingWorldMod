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
		TYPE_CYCLE, SLOPE_TOP_RIGHT, TYPE_FLAT, SLOPE_BOTTOM_RIGHT, TYPE_NULL, SLOPE_BOTTOM_LEFT, TYPE_FULL, SLOPE_TOP_LEFT
    }

	public class ClockworkHammer : ModItem
	{
		public SlopeSetting SlopeSetting { get; set; } = SlopeSetting.TYPE_CYCLE;

		private Vector2 tilePos = new Vector2();

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
				// because this method is called over and over again, don't do the toggle
				if (!LivingWorldMod.Instance.HasClockworkHammerUI())
					LivingWorldMod.Instance.ShowClockworkHammerUI(this);
				//	LivingWorldMod.Instance.CloseClockworkHammerUI();
				return false; // pretend nothing happened
            }
			else
            {
				if (LivingWorldMod.Instance.HasClockworkHammerUI())
					return false; // no usage

				// do the thing
				tilePos.X = Main.mouseX + Main.screenPosition.X;
				// handle reverse gravity
				if (player.gravDir == 1f) tilePos.Y = Main.mouseY + Main.screenPosition.Y;
				else tilePos.Y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY;

				tilePos /= 16f;
				Tile tile = Main.tile[(int) tilePos.X, (int) tilePos.Y];
				if(tile.active())
					ModifyTile(tile);

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
					tile.halfBrick(tile.slope() != Tile.Type_Solid);
					break;
				case SlopeSetting.SLOPE_TOP_RIGHT:
					tile.slope(Tile.Type_SlopeUpRight);
					tile.halfBrick(true);
					break;
				case SlopeSetting.TYPE_FLAT:
					tile.slope(Tile.Type_Halfbrick);
					tile.halfBrick(true);
					break;
				case SlopeSetting.SLOPE_BOTTOM_RIGHT:
					tile.slope(Tile.Type_SlopeDownRight);
					tile.halfBrick(true);
					break;
				case SlopeSetting.TYPE_NULL:
					// no function for now
					return;
				case SlopeSetting.SLOPE_BOTTOM_LEFT:
					tile.slope(Tile.Type_SlopeDownLeft);
					tile.halfBrick(true);
					break;
				case SlopeSetting.TYPE_FULL:
					tile.slope(Tile.Type_Solid);
					tile.halfBrick(false);
					break;
				case SlopeSetting.SLOPE_TOP_LEFT:
					tile.slope(Tile.Type_SlopeUpLeft);
					tile.halfBrick(true);
					break;
			}

			Main.PlaySound(SoundID.Dig);
        }
    }
}
