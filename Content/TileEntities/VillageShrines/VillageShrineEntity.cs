using LivingWorldMod.Common.Players;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.TileEntities.VillageShrines {

    /// <summary>
    /// Tile Entity within each village shrine of each type, which mainly handles whether or not a
    /// specified player is close enough to the specified shrine to be considered "within the village."
    /// </summary>
    public abstract class VillageShrineEntity : BaseTileEntity {

        /// <summary>
        /// Whether or not to draw the outline of the area that is considered within the village. Is
        /// client-side/single player only.
        /// </summary>
        public bool showVillageZone;

        /// <summary>
        /// The village type that this shrine entity pertains to.
        /// </summary>
        public abstract VillagerType VillageType {
            get;
        }

        /// <summary>
        /// The distance (in TILES) from the center of the shrine that is considered within the village.
        /// </summary>
        public virtual float VillageTileRadius => 75;

        /// <summary>
        /// The type of dust to be used when drawing the dust circle that denotes what is inside of
        /// the village this entity/shrine is tied to. Defaults to dirt dust.
        /// </summary>
        public virtual int VillageZoneDustType => DustID.Dirt;

        /// <summary>
        /// The "dimensions" of the entity, in pixels.
        /// </summary>
        public Vector2 EntityDimensions => new Vector2(4 * 16, 5 * 16);

        public override void Update() {
            for (int i = 0; i < Main.maxPlayers; i++) {
                if (Main.player[i].Distance(WorldPosition + EntityDimensions / 2f) <= VillageTileRadius * 16) {
                    Main.player[i].GetModPlayer<BiomePlayer>().currentVillageBiome = VillageType;
                }
            }

            if (Main.netMode != NetmodeID.Server && showVillageZone) {
                CreateVillageZoneCircle();
            }
        }

        public void CreateVillageZoneCircle() {
            Dust dust = Dust.NewDustPerfect(WorldPosition + EntityDimensions / 2f, VillageZoneDustType);
            dust.noGravity = true;
            dust.scale = 1.25f;
            DustUtilities.CreateCircle(dust.position, VillageTileRadius * 16, dust);
        }

        public void RightClicked() {
            showVillageZone = !showVillageZone;
        }
    }
}