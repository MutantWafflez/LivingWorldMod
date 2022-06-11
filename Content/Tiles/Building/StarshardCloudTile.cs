using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.Items.Placeables.Building;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles.Building {
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

            ItemDrop = ModContent.ItemType<StarshardCloudItem>();
            DustType = DustID.Cloud;
        }

        public override bool HasWalkDust() => true;

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            //All-multiplied by 0.5f since the color at full capacity is a bit overbearing
            r = BlockLightSystem.starCloudColor.R / 255f * 0.5f;
            g = BlockLightSystem.starCloudColor.G / 255f * 0.5f;
            b = BlockLightSystem.starCloudColor.B / 255f * 0.5f;
        }
    }
}