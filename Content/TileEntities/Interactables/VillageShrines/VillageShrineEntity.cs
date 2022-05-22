using LivingWorldMod.Common.Players;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.TileEntities.Interactables.VillageShrines {
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
        /// The distance (in PIXELS) from the center of the shrine that is considered within the village.
        /// </summary>
        public virtual float VillageRadius => 1200; //75 tiles (75 * 16)

        /// <summary>
        /// The displacement distance (in PIXELS) from the original center of the shrine that will
        /// be considered the "origin" of the village.
        /// </summary>
        public virtual Vector2 VillageOriginDisplacement => Vector2.Zero;

        /// <summary>
        /// The type of dust to be used when drawing the dust circle that denotes what is inside of
        /// the village this entity/shrine is tied to. Defaults to dirt dust.
        /// </summary>
        public virtual int VillageZoneDustType => DustID.Dirt;

        public override Vector2 EntityDimensions => new Vector2(64f, 80f); // 4 x 5 tiles

        /// <summary>
        /// The origin of the village. Without any displacement, this is in the center of this
        /// entity's respective shrine tile.
        /// </summary>
        public Vector2 VillageOriginPosition => WorldPosition + EntityDimensions / 2f + VillageOriginDisplacement;

        public override void Update() {
            for (int i = 0; i < Main.maxPlayers; i++) {
                if (Main.player[i].Distance(VillageOriginPosition) <= VillageRadius) {
                    Main.player[i].GetModPlayer<BiomePlayer>().currentVillageBiome = VillageType;
                }
            }

            if (Main.netMode != NetmodeID.Server && showVillageZone) {
                CreateVillageZoneCircle();
            }
        }

        public void CreateVillageZoneCircle() {
            Dust dust = Dust.NewDustPerfect(VillageOriginPosition, VillageZoneDustType);
            dust.active = false;
            dust.noGravity = true;
            dust.scale = 1.25f;
            DustUtils.CreateCircle(dust.position, VillageRadius, dust);
        }

        public void RightClicked() {
            showVillageZone = !showVillageZone;
        }
    }
}