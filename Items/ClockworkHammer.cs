using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace LivingWorldMod.Items
{
	public class ClockworkHammer : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Right click while holding to choose hammer preset");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.CopperHammer);
			item.damage = 20;
			item.melee = true;
			item.width = 60;
			item.height = 60;
			item.useTime = 15;
			item.useAnimation = 15;
			item.hammer = 85;
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
			recipe.AddIngredient(ItemID.Cog, 50);
			recipe.AddIngredient(ItemID.GoldBar, 10);
			recipe.AddIngredient(ItemID.Wire, 20);

			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

        public override bool CanUseItem(Player player)
        {
            if(player.altFunctionUse == 2)
            {
				LivingWorldMod.Instance.ToggleClockworkHammerUI();
            }
			else
            {
				// use animation?
            }
			return true;
        }
    }
}
