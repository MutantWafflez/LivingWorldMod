using LivingWorldMod.Content.Items.Walls.Building;
using LivingWorldMod.Content.Tiles.Building;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Placeables.Building {

    public class SkywareBlockItem : BaseItem {

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DirtBlock);
            Item.value = Item.buyPrice(copper: 75);
            Item.placeStyle = 0;
            Item.createTile = ModContent.TileType<SkywareBlockTile>();
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient<SkywareWallItem>(4)
                .Register();
        }
    }
}