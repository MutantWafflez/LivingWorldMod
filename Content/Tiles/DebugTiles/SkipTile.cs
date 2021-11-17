using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.Tiles.DebugTiles {
    /// <summary>
    /// Tile used for "skipping" over certain tile positions when used in conjunction with the
    /// structure stick.
    /// </summary>
    public class SkipTile : DebugTile {
        public override string Texture => "Terraria/Images/Tiles_" + TileID.TeamBlockWhite;

        public override void SetStaticDefaults() {
            Main.tileNoAttach[Type] = false;
            Main.tileNoSunLight[Type] = false;
            Main.tileLavaDeath[Type] = false;
            Main.tileWaterDeath[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;

            TileID.Sets.HousingWalls[Type] = false;
        }
    }
}