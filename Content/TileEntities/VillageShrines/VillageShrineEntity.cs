using LivingWorldMod.Common.Players;
using LivingWorldMod.Custom.Enums;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TileEntities.VillageShrines {

    /// <summary>
    /// Tile Entity within each villager shrine of each type, which mainly handles whether or not a
    /// specified player is close enough to the specified shrine to be considered "within the village."
    /// </summary>
    public abstract class VillageShrineEntity : ModTileEntity {

        /// <summary>
        /// The village type that this shrine entity pertains to.
        /// </summary>
        public abstract VillagerType VillageType {
            get;
        }

        /// <summary>
        /// The ID of the tile that this villager shrine entity attaches to.
        /// </summary>
        public abstract int ShrineTileID {
            get;
        }

        public override bool ValidTile(int i, int j) {
            Tile tile = Framing.GetTileSafely(i, j);
            return tile.IsActive && tile.type == ShrineTileID && tile.frameX == 0 && tile.frameY == 0;
        }

        public override void Update() {
            for (int i = 0; i < Main.maxPlayers; i++) {
                if (Main.player[i].Distance(Position.ToWorldCoordinates(0, 0)) <= 50 * 16) {
                    Main.player[i].GetModPlayer<BiomePlayer>().currentVillageBiome = VillageType;
                }
            }
        }
    }
}