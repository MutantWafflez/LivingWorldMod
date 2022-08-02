using System.IO;
using LivingWorldMod.Content.MiscEntities;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.TileEntities.Interactables {
    /// <summary>
    /// Tile Entity for the Waystone tiles.
    /// </summary>
    public class WaystoneEntity : TEModdedPylon {
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

        public override Vector2 EntityDimensions => new Vector2(32, 48); //2x3 tiles 

        private int _activationTimer;
        private bool _doingActivationVFX;

        public override void Update() {
            // Update is only called on server; we don't need to do the visual flair for the server, so we just wait until that is done, then update all clients accordingly
            if (Main.netMode != NetmodeID.MultiplayerClient && _doingActivationVFX) {
                if (++_activationTimer > WaystoneActivationEntity.FullActivationWaitTime) {
                    _doingActivationVFX = false;
                    _activationTimer = 0;
                    isActivated = true;

                    NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
                }
            }
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            TileObjectData tileData = TileObjectData.GetTileData(type, style, alternate);
            int topLeftX = i - tileData.Origin.X;
            int topLeftY = j - tileData.Origin.Y;

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, tileData.Width, tileData.Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: Type);
                return -1;
            }

            if (ManualPlace(topLeftX, topLeftY, (WaystoneType)style) && TileEntityUtils.TryFindModEntity(topLeftX, topLeftY, out WaystoneEntity entity)) {
                return entity.ID;
            }

            return -1;
        }

        public override void OnNetPlace() {
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }

        public override void NetSend(BinaryWriter writer) {
            writer.Write(isActivated);
            writer.Write((int)waystoneType);
        }

        public override void NetReceive(BinaryReader reader) {
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
        /// Should be called whenever the tile that is entity is associated with is right clicked & activated in SINGLERPLAYER or ON THE SERVER. Shouldn't
        /// be called on any multiplayer client; we handle that with our own packets.
        /// </summary>
        public void ActivateWaystoneEntity() {
            if (isActivated || _doingActivationVFX) {
                return;
            }

            _doingActivationVFX = true;
            _activationTimer = 0;
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
            if (Framing.GetTileSafely(i, j).TileType != ModContent.TileType<WaystoneTile>()) {
                return false;
            }

            // Then, place tile entity and assign its type
            Place(i, j);
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