using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Tiles.Interactables.VillageShrines;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TileEntities.Interactables.VillageShrines {
    /// <summary>
    /// Tile Entity within each village shrine of each type, which mainly handles whether or not a
    /// specified player is close enough to the specified shrine to be considered "within the village."
    /// </summary>
    [LegacyName("HarpyShrineEntity")]
    public class VillageShrineEntity : BaseTileEntity {
        public override int ValidTileID => ModContent.TileType<VillageShrineTile>();

        public override bool? PreValidTile(int i, int j) {
            Tile tile = Framing.GetTileSafely(i, j);

            return tile.HasTile && tile.TileType == ValidTileID && tile.TileFrameX % 72 == 0 && tile.TileFrameY == 0;
        }
    }
}