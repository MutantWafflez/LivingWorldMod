﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TileEntities {
    /// <summary>
    /// Base Tile Entity for all other Tile Entities in the mod, which has a ValidTileID property
    /// for easy matching of tile to entity and vice-versa.
    /// </summary>
    public abstract class BaseTileEntity : ModTileEntity {
        /// <summary>
        /// The "dimensions" of the entity, in pixels.
        /// </summary>
        public virtual Vector2 EntityDimensions => new(16); //1x1 tiles, aka 16x16 pixels

        /// <summary>
        /// The ID of the tile that this tile entity attaches to.
        /// </summary>
        public abstract int ValidTileID {
            get;
        }

        /// <summary>
        /// The pixel position of this entity (aka NOT tile coordinates.)
        /// </summary>
        public Vector2 WorldPosition => Position.ToWorldCoordinates(0, 0);

        /// <summary>
        /// Called before <seealso cref="IsTileValidForEntity"/> that can be used for overriding the default
        /// valid tile check for some kind of special functionality. True means the tile will always
        /// be valid regardless of normal conditions, null means the tile will be valid based on the
        /// normal pre-defined conditions, and false will prevent the tile from being valid
        /// regardless of normal conditions.
        /// </summary>
        public virtual bool? PreValidTile(int i, int j) => null;

        public sealed override bool IsTileValidForEntity(int x, int y) {
            bool? preCheck = PreValidTile(x, y);

            if (!preCheck.HasValue) {
                Tile tile = Framing.GetTileSafely(x, y);
                return tile.HasTile && tile.TileType == ValidTileID && tile.TileFrameX == 0 && tile.TileFrameY == 0;
            }
            return preCheck.Value;
        }
    }
}