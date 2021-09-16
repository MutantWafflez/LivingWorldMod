using LivingWorldMod.Common.Players;
using LivingWorldMod.Custom.Enums;
using Terraria;

namespace LivingWorldMod.Content.TileEntities.VillageShrines {

    /// <summary>
    /// Tile Entity within each village shrine of each type, which mainly handles whether or not a
    /// specified player is close enough to the specified shrine to be considered "within the village."
    /// </summary>
    public abstract class VillageShrineEntity : BaseTileEntity {

        /// <summary>
        /// The village type that this shrine entity pertains to.
        /// </summary>
        public abstract VillagerType VillageType {
            get;
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