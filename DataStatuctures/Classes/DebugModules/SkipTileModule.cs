using LivingWorldMod.Content.Tiles.DebugTiles;

namespace LivingWorldMod.DataStatuctures.Classes.DebugModules;

/// <summary>
/// Module that places Skip Tiles over all air tiles in the region.
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