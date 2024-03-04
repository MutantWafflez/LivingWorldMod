using LivingWorldMod.Content.Villages.HarpyVillage.Walls;
using LivingWorldMod.Globals.BaseTypes.Items;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Blocks;

public class SkywareBlockItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 50;
    }

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