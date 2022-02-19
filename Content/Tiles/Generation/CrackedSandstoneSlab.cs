using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.Tiles.Generation {
    public class CrackedSandstoneSlab : BaseTile {
        public override void SetStaticDefaults() {
            Main.tileMerge[Type][TileID.SandStoneSlab] = true;
            Main.tileMerge[TileID.SandStoneSlab][Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileCracked[Type] = true;
            Main.tileSolid[Type] = true;

            TileID.Sets.CrackedBricks[Type] = true;
            TileID.Sets.HousingWalls[Type] = false;

            MinPick = 40;
            DustType = DustID.Sand;
            VanillaFallbackOnModDeletion = TileID.SandStoneSlab;

            AddMapEntry(Color.LightYellow);
        }

        public override bool Dangersense(int i, int j, Player player) => true;
    }
}