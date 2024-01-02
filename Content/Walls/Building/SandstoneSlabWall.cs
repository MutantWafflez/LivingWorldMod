using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.Walls.Building;

public class SandstoneSlabWall : BaseWall {
    public override Color? WallColorOnMap => new Color(108, 103, 72);

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;
        Main.wallLargeFrames[Type] = Main.wallLargeFrames[WallID.StoneSlab];

        DustType = DustID.Sand;

        base.SetStaticDefaults();
    }
}