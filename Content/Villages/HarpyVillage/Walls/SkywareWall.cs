using LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Blocks;
using LivingWorldMod.Content.Walls;
using LivingWorldMod.Globals.BaseTypes.Items;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Walls;

public class SkywareWall : BaseWall {
    public override Color? WallColorOnMap => Color.Cyan;

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = DustID.BlueMoss;

        base.SetStaticDefaults();
    }
}

public class SkywareWallItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 40;
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