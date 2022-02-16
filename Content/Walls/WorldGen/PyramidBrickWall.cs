using Terraria.ID;

namespace LivingWorldMod.Content.Walls.WorldGen {
    /// <summary>
    /// Brick wall that looks identical to vanilla's sandstone brick walls, but are unbreakable since they gen within the
    /// Revamped Pyramid.
    /// </summary>
    public class PyramidBrickWall : BaseWall {
        public override string Texture => "Terraria/Images/Wall_" + WallID.SandstoneBrick;

        public override void SetStaticDefaults() {
            DustType = DustID.Sand;
        }

        public override bool CanExplode(int i, int j) => false;

        public override void KillWall(int i, int j, ref bool fail) {
            //Unbreakable
            fail = true;
        }
    }
}