using LivingWorldMod.Content.Walls;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Walls;

public class SkywareFenceWall : BaseWall {
    public override Color? WallColorOnMap => Color.LightBlue;

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;
        WallID.Sets.AllowsWind[Type] = true;
        WallID.Sets.Transparent[Type] = true;

        DustType = DustID.t_LivingWood;

        base.SetStaticDefaults();
    }
}