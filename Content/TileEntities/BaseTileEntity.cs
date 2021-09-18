using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TileEntities {

    /// <summary>
    /// Base Tile Entity for all other Tile Entities in the mod, which has a ValidTileID property
    /// for easy matching of tile to entity and vice-versa.
    /// </summary>
    public abstract class BaseTileEntity : ModTileEntity {

        /// <summary>
        /// The ID of the tile that this tile entity attaches to.
        /// </summary>
        public abstract int ValidTileID {
            get;
        }

        public Vector2 WorldPosition => Position.ToWorldCoordinates(0, 0);

        public override bool ValidTile(int i, int j) {
            Tile tile = Framing.GetTileSafely(i, j);
            return tile.IsActive && tile.type == ValidTileID && tile.frameX == 0 && tile.frameY == 0;
        }
    }
}