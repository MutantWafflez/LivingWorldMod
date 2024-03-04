using LivingWorldMod.Content.Villages.HarpyVillage.Walls;
using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.Tiles;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Blocks;

public class SkywareBlockTile : BaseTile {
    public override Color? TileColorOnMap => Color.LightBlue;

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = true;
        Main.tileNoSunLight[Type] = true;
        Main.tileMergeDirt[Type] = false;

        MineResist = 1.15f;

        DustType = DustID.BlueMoss;
    }
}

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