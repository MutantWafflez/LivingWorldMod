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