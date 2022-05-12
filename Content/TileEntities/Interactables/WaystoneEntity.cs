using System.IO;
using System.Linq;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TileEntities.Interactables {
    /// <summary>
    /// Tile Entity for the Waystone tiles.
    /// </summary>
    public class WaystoneEntity : BaseTileEntity {
        public bool isActivated;

        public WaystoneType waystoneType;

        public Color WaystoneColor {
            get {
                return waystoneType switch {
                    WaystoneType.Desert => Color.Yellow,
                    WaystoneType.Jungle => Color.LimeGreen,
                    WaystoneType.Mushroom => Color.DarkBlue,
                    WaystoneType.Caverns => Color.Lavender,
                    WaystoneType.Ice => Color.LightBlue,
                    _ => Color.White
                };
            }
        }

        public override int ValidTileID => ModContent.TileType<WaystoneTile>();

        private int _activationVFXStage;
        private int _activationVFXTimer;
        private int _activationVFXSecondaryTimer;
        private bool _doingActivationVFX;

        public override bool? PreValidTile(int i, int j) {
            Tile tile = Framing.GetTileSafely(i, j);

            return tile.HasTile && tile.TileType == ValidTileID && tile.TileFrameX == 18 * 2 * (int)waystoneType;
        }

        public override void Update() {
            // Only run code during the activation sequence (and the player is tabbed in)
            if (!_doingActivationVFX) {
                return;
            }

            // Get top left of the tile position, then move it to the center
            Vector2 tileCenter = WorldPosition + new Vector2(18, 24);

            float circleRadius = 160f;

            if (_activationVFXStage == 0) {
                // Generate circle of dust. Would use the Utils method that was made for this, but this is "special" drawing
                for (int x = 0; x <= _activationVFXTimer; x++) {
                    Dust.NewDustPerfect(tileCenter - new Vector2(0, circleRadius).RotatedBy(MathHelper.ToRadians(x * 20f)), DustID.GoldCoin, newColor: WaystoneColor);
                }

                if (++_activationVFXSecondaryTimer > 18) {
                    // Every 18 frames, add a particle to the circle
                    _activationVFXSecondaryTimer = 0;
                    _activationVFXTimer++;

                    SoundEngine.PlaySound(SoundID.Item100, tileCenter - new Vector2(0, circleRadius).RotatedBy(MathHelper.ToRadians(_activationVFXTimer * 20f)));
                }

                if (_activationVFXTimer >= 18) {
                    // After 18 particles are created, move to next stage
                    _activationVFXStage = 1;
                    _activationVFXTimer = 0;
                    _activationVFXSecondaryTimer = 0;
                }
            }
            else if (_activationVFXStage == 1) {
                // Drag circle to the center of the tile
                int circlePullThreshold = 30;
                int finaleThreshold = 5;

                DustUtils.CreateCircle(tileCenter, circleRadius * (1f - _activationVFXTimer / (float)circlePullThreshold), DustID.GoldCoin, newColor: WaystoneColor, angleChange: 20f);

                // Step RAPIDLY closer
                _activationVFXTimer++;

                // After center of tile is reached, complete activation
                if (_activationVFXTimer == circlePullThreshold) {
                    // Play finale sound and give text confirmation
                    SoundEngine.PlaySound(SoundID.Item113, tileCenter);

                    Main.NewText(LocalizationUtils.GetLWMTextValue("Event.WaystoneActivation"), Color.Yellow);

                    isActivated = true;
                }
                else if (_activationVFXTimer > circlePullThreshold + finaleThreshold) {
                    // Internally end sequence
                    _doingActivationVFX = false;
                    _activationVFXStage = 0;
                    _activationVFXTimer = 0;
                    _activationVFXSecondaryTimer = 0;
                }
            }
        }

        public override void OnNetPlace() {
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }

        public override void NetSend(BinaryWriter writer) {
            writer.Write(_doingActivationVFX);
            writer.Write(isActivated);
            writer.Write((int)waystoneType);
        }

        public override void NetReceive(BinaryReader reader) {
            _doingActivationVFX = reader.ReadBoolean();
            isActivated = reader.ReadBoolean();
            waystoneType = (WaystoneType)reader.ReadInt32();
        }

        public override void SaveData(TagCompound tag) {
            tag["active"] = isActivated;
            tag["type"] = (int)waystoneType;
        }

        public override void LoadData(TagCompound tag) {
            isActivated = tag.GetBool("active");
            waystoneType = (WaystoneType)tag.GetInt("type");
        }

        /// <summary>
        /// Should be called whenever the tile that is entity is associated with is right clicked. Does different things depending on if
        /// this Waystone is activated or not.
        /// </summary>
        public void AttemptWaystoneActivation() {
            if (isActivated || _doingActivationVFX) {
                return;
            }

            _doingActivationVFX = true;
            _activationVFXStage = 0;
            _activationVFXTimer = 0;
            _activationVFXSecondaryTimer = 0;
        }

        /// <summary>
        /// Method use for manual placing of a Waystone Entity. Primary usage is within WorldGen. Will return false
        /// if the placement is invalid, true otherwise.
        /// </summary>
        /// <param name="i"> x location to attempt entity placement. </param>
        /// <param name="j"> y location to attempt entity placement. </param>
        /// <param name="type"> What type of waystone to place at this location </param>
        /// <param name="isActivated"> Whether or not this waystone should be activated or not. Defaults to false. </param>
        /// <returns></returns>
        public bool ManualPlace(int i, int j, WaystoneType type, bool isActivated = false) {
            // First, double check that tile is a Waystone tile
            if (Framing.GetTileSafely(i, j).TileType != ValidTileID) {
                return false;
            }

            // Then, place tile entity and assign its type
            ModContent.GetInstance<WaystoneEntity>().Place(i, j);
            if (TileEntityUtils.TryFindModEntity(i, j, out WaystoneEntity retrievedEntity)) {
                retrievedEntity.waystoneType = type;
                retrievedEntity.isActivated = isActivated;
                return true;
            }
            else {
                return false;
            }
        }
    }
}