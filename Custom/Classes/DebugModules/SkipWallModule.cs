using LivingWorldMod.Content.Walls.DebugWalls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Classes.DebugModules;

/// <summary>
/// Module that places Skip Walls over all tiles without walls in the region.
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