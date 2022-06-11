using LivingWorldMod.Content.Items.Placeables.Building;
using LivingWorldMod.Content.Walls.Building;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.Walls.Building {
    public class SkywareWallItem : BaseItem {
        public override void SetStaticDefaults() {
            SacrificeTotal = 40;
        }

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DirtWall);
            Item.placeStyle = 0;
            Item.value = Item.buyPrice(copper: 15);
            Item.createWall = ModContent.WallType<SkywareWall>();
        }

        public override void AddRecipes() {
            CreateRecipe(4)
                .AddIngredient<SkywareBlockItem>()
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}