using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Custom.Utilities {
    /// <summary>
    /// Utilities class that holds methods that deal specifically within the realm of Tile Entities.
    /// </summary>
    public static class TileEntityUtils {
        /// <summary>
        /// Tries to find an entity of the specified Type. Returns whether or not it found the
        /// entity or not.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="x"> The x coordinate of the potential entity. </param>
        /// <param name="y"> The y coordinate of the potential entity. </param>
        /// <param name="entity"> The potential entity. </param>
        public static bool TryFindModEntity<T>(int x, int y, out T entity) where T : ModTileEntity {
            TileEntity.ByPosition.TryGetValue(new Point16(x, y), out TileEntity retrievedEntity);

            if (retrievedEntity is T castEntity) {
                entity = castEntity;
                return true;
            }

            entity = null;
            return false;
        }

        /// <summary>
        /// Tries to find an entity of the specified Type. Returns whether or not it found the
        /// entity or not.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="ID"> The ID of the potential entity. </param>
        /// <param name="entity"> The potential entity. </param>
        public static bool TryFindModEntity<T>(int ID, out T entity) where T : ModTileEntity {
            TileEntity retrievedEntity = TileEntity.ByID[ID];

            if (retrievedEntity is T castEntity) {
                entity = castEntity;
                return true;
            }

            entity = null;
            return false;
        }
    }
}