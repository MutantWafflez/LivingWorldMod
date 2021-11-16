using System;
using System.Linq;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TileEntities.Interactables {
    
    /// <summary>
    /// Tile Entity for the Waystone tiles.
    /// </summary>
    public class WaystoneEntity : BaseTileEntity {

        public bool isActivated;

        public WaystoneType waystoneType;

        /// <summary>
        /// Base entity of WaystoneEntity, used for placing more instances of itself.
        /// </summary>
        public static WaystoneEntity BaseEntity => TileEntitySystem.GetBaseEntityInstance<WaystoneEntity>();

        private int activationVFXStage;
        private int activationVFXTimer;
        private int activationVFXSecondaryTimer;
        private bool doingActivationVFX;
        
        public override int ValidTileID => ModContent.TileType<WaystoneTile>();

        /// <summary>
        /// Called when a waystone tile is placed. Places a waystone entity at the tile's location.
        /// </summary>
        public static int WaystonePlaced(int i, int j, int type, int style, int direction, int alternate) {
            WaystoneSystem waystoneSystem = ModContent.GetInstance<WaystoneSystem>();

            // Place tile entity and assign its type
            int waystoneEntityID = BaseEntity.Place(i, j);
            ((WaystoneEntity)ByID[waystoneEntityID]).waystoneType = (WaystoneType)style;

            // Add to data list
            waystoneSystem.waystoneData.Add(new WaystoneInfo(new Point16(i, j), (WaystoneType)style, false));

            return waystoneEntityID;
        }

        public override void Update() {
            // Only run code during the activation sequence (and the player is tabbed in)
            if (!doingActivationVFX) {
                return;
            }

            // Get top left of the tile position, then move it to the center
            Vector2 tileCenter = WorldPosition + new Vector2(18, 24);

            float circleRadius = 160f;

            if (activationVFXStage == 0) {
                // Generate circle of dust. Would use the Utils method that was made for this, but this is "special" drawing
                for (int x = 0; x <= activationVFXTimer; x++) {
                    Dust.NewDustPerfect(tileCenter - new Vector2(0, circleRadius).RotatedBy(MathHelper.ToRadians(x * 20f)), DustID.GoldCoin);
                }
                
                if (++activationVFXSecondaryTimer > 18) {
                    // Every 18 frames, add a particle to the circle
                    activationVFXSecondaryTimer = 0;
                    activationVFXTimer++;
                    
                    SoundEngine.PlaySound(SoundID.Item100, tileCenter - new Vector2(0, circleRadius).RotatedBy(MathHelper.ToRadians(activationVFXTimer * 20f)));
                }

                if (activationVFXTimer >= 18) {
                    // After 18 particles are created, move to next stage
                    activationVFXStage = 1;
                    activationVFXTimer = 0;
                    activationVFXSecondaryTimer = 0;
                }
            }
            else if (activationVFXStage == 1) {
                // Drag circle to the center of the tile
                int circlePullThreshold = 30;
                int finaleThreshold = 5;
                
                DustUtils.CreateCircle(tileCenter, circleRadius * (1f - (activationVFXTimer / (float)circlePullThreshold)), DustID.GoldCoin, 20f);

                // Step RAPIDLY closer
                activationVFXTimer++;

                // After center of tile is reached, complete activation
                if (activationVFXTimer == circlePullThreshold) {
                    // Play finale sound and give text confirmation
                    SoundEngine.PlaySound(SoundID.Item113, tileCenter);
                    
                    Main.NewText(LocalizationUtils.GetLWMTextValue("Event.WaystoneActivation"), Color.Yellow);

                    //Activate this waystone on the map
                    if (ModContent.GetInstance<WaystoneSystem>().waystoneData.FirstOrDefault(waystone => waystone.tileLocation == Position) is { } info) {
                        info.isActivated = true;
                    }
                    isActivated = true;
                }
                else if (activationVFXTimer > circlePullThreshold + finaleThreshold) {
                    // Internally end sequence
                    doingActivationVFX = false;
                    activationVFXStage = 0;
                    activationVFXTimer = 0;
                    activationVFXSecondaryTimer = 0;
                }
            }
        }

        public override void OnKill() {
            // Remove this entry when the tile is killed. Shouldn't be possible, but if it *does* happen, we handle it
            WaystoneSystem waystoneSystem = ModContent.GetInstance<WaystoneSystem>();
            if (waystoneSystem.waystoneData.FirstOrDefault(waystone => waystone.tileLocation == Position) is { } info) {
                waystoneSystem.waystoneData.Remove(info);
            }
        }

        public void RightClicked() {
            if (isActivated || doingActivationVFX) {
                return;
            }
            
            doingActivationVFX = true;
            activationVFXStage = 0;
            activationVFXTimer = 0;
            activationVFXSecondaryTimer = 0;
        }
    }
}