using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.Walls.Building;

public class SkywareWall : BaseWall {
    public override Color? WallColorOnMap => Color.Cyan;

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = DustID.BlueMoss;

        base.SetStaticDefaults();
    }
}