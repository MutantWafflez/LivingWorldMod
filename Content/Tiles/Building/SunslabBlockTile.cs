using LivingWorldMod.Content.Items.Placeables.Building;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles.Building {

    //TODO: Figure out weird tile framing with this tile. It mismatches the intended pattern
    public class SunslabBlockTile : BaseTile {

        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = true;
            Main.tileNoSunLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][ModContent.TileType<StarshineBlockTile>()] = true;
            Main.tileMerge[Type][TileID.Sunplate] = true;
            Main.tileMerge[TileID.Sunplate][Type] = true;

            MineResist = 1.34f;

            ItemDrop = ModContent.ItemType<SunslabBlockItem>();

            DustType = DustID.GoldCoin;
            SoundType = SoundID.Tink;

            AddMapEntry(Color.Yellow);
        }

        public override bool HasWalkDust() => true;
    }
}