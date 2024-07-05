using LivingWorldMod.Globals.BaseTypes.Walls;

namespace LivingWorldMod.DataStructures.Classes.DebugModules;

/// <summary>
///     Module that places Skip Walls over all tiles without walls in the region.
/// </summary>
public class SkipWallModule : RegionModule {
    protected override void ApplyEffectOnRegion() {
        for (int x = 0; x <= bottomRight.X - topLeft.X; x++) {
            for (int y = 0; y <= bottomRight.Y - topLeft.Y; y++) {
                Tile requestedTile = Framing.GetTileSafely(x + topLeft.X, y + topLeft.Y);
                if (requestedTile.WallType == WallID.None) {
                    WorldGen.PlaceWall(x + topLeft.X, y + topLeft.Y, ModContent.WallType<SkipWall>());
                }
            }
        }

        Main.NewText("Walls Placed!");
    }
}

/// <summary>
///     Wall used for "skipping" over certain wall positions when used in conjunction with the
///     structure stick.
/// </summary>
public class SkipWall : BaseWall {
    public override string Texture => "Terraria/Images/Wall_" + WallID.StarsWallpaper;

    public override bool IsLoadingEnabled(Mod mod) => LWM.IsDebug;

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = false;
        Main.wallLight[Type] = true;

        WallID.Sets.AllowsWind[Type] = false;
    }
}