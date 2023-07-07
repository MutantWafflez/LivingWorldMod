using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.Tiles.Building {
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
}