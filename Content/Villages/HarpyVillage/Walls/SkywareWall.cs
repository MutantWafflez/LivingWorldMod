using LivingWorldMod.Content.Walls;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Walls;

public class SkywareWall : BaseWall {
    public override Color? WallColorOnMap => Color.Cyan;

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        DustType = DustID.BlueMoss;

        base.SetStaticDefaults();
    }
}