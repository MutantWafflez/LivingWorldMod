#if DEBUG

using LivingWorldMod.Content.Items.DebugItems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles.DebugTiles {

    /// <summary>
    /// Tile used for "skipping" over certain tile positions when used in conjunction with the
    /// structure stick.
    /// </summary>
    public class SkipTile : BaseTile {
        public override string Texture => "Terraria/Images/Tiles_" + TileID.TeamBlockWhite;

        public override void SetStaticDefaults() {
            Main.tileNoAttach[Type] = false;
            Main.tileNoSunLight[Type] = false;
            Main.tileLavaDeath[Type] = false;
            Main.tileWaterDeath[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;

            TileID.Sets.HousingWalls[Type] = false;

            ItemDrop = ModContent.ItemType<SkipTileItem>();
        }
    }
}

#endif