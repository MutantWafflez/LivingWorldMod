using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.Tiles;
using LivingWorldMod.Globals.Systems;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Tiles.Blocks;

public class StarshardCloudTile : BaseTile {
    public override Color? TileColorOnMap => Color.LightYellow;

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = true;
        Main.tileNoSunLight[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileMergeDirt[Type] = false;
        Main.tileMerge[Type][TileID.Cloud] = true;
        Main.tileMerge[Type][TileID.RainCloud] = true;
        Main.tileMerge[Type][TileID.SnowCloud] = true;
        Main.tileMerge[TileID.Cloud][Type] = true;
        Main.tileMerge[TileID.RainCloud][Type] = true;
        Main.tileMerge[TileID.SnowCloud][Type] = true;

        TileID.Sets.Clouds[Type] = true;
        TileID.Sets.MergesWithClouds[Type] = true;

        MineResist = 1.34f;

        DustType = DustID.Cloud;
    }

    public override bool HasWalkDust() => true;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        //All-multiplied by 0.5f since the color at full capacity is a bit overbearing
        r = BlockLightSystem.Instance.starCloudColor.R / 255f * 0.5f;
        g = BlockLightSystem.Instance.starCloudColor.G / 255f * 0.5f;
        b = BlockLightSystem.Instance.starCloudColor.B / 255f * 0.5f;
    }
}

public class StarshardCloudItem : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 50;
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.DirtBlock);
        Item.value = Item.buyPrice(copper: 75);
        Item.placeStyle = 0;
        Item.createTile = ModContent.TileType<StarshardCloudTile>();
    }
}