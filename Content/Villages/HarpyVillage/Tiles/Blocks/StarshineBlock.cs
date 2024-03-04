using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.Tiles;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Blocks;

public class StarshineBlockTile : BaseTile {
    public override Color? TileColorOnMap => Color.DarkBlue;

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = true;
        Main.tileNoSunLight[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileMerge[Type][ModContent.TileType<SunslabBlockTile>()] = true;
        Main.tileMerge[Type][TileID.Sunplate] = true;
        Main.tileMerge[TileID.Sunplate][Type] = true;

        MineResist = 1.34f;

        DustType = DustID.BlueTorch;
    }

    public override bool HasWalkDust() => true;
}

public class StarshineBlockItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 50;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.DirtBlock);
        Item.value = Item.buyPrice(silver: 1);
        Item.placeStyle = 0;
        Item.createTile = ModContent.TileType<StarshineBlockTile>();
    }
}