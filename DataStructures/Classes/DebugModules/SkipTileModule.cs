using LivingWorldMod.Globals.BaseTypes.Tiles;

namespace LivingWorldMod.DataStructures.Classes.DebugModules;

/// <summary>
///     Module that places Skip Tiles over all air tiles in the region.
/// </summary>
public class SkipTileModule : RegionModule {
    protected override void ApplyEffectOnRegion() {
        for (int x = 0; x <= bottomRight.X - topLeft.X; x++) {
            for (int y = 0; y <= bottomRight.Y - topLeft.Y; y++) {
                Tile requestedTile = Framing.GetTileSafely(x + topLeft.X, y + topLeft.Y);
                if (!requestedTile.HasTile) {
                    WorldGen.PlaceTile(bottomRight.X + x, bottomRight.Y + y, ModContent.TileType<SkipTile>());
                }
            }
        }

        Main.NewText("Tiles Placed!");
    }
}

/// <summary>
///     Tile used for "skipping" over certain tile positions when used in conjunction with the
///     structure stick.
/// </summary>
public class SkipTile : BaseTile {
    public override string Texture => "Terraria/Images/Tiles_" + TileID.TeamBlockWhite;

    public override bool IsLoadingEnabled(Mod mod) => LWM.IsDebug;

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