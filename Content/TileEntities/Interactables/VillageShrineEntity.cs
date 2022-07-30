﻿using System.Collections.Generic;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Structs;
using System.IO;
using System.Linq;
using LivingWorldMod.Common.Systems.UI;
using LivingWorldMod.Content.Tiles.Interactables;
using LivingWorldMod.Content.UI.VillageShrine;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using LivingWorldMod.Custom.Utilities;
using Terraria.Chat;
using Terraria.Localization;

namespace LivingWorldMod.Content.TileEntities.Interactables {
    /// <summary>
    /// Tile Entity within each village shrine of each type, which mainly handles whether or not a
    /// specified player is close enough to the specified shrine to be considered "within the village."
    /// </summary>
    [LegacyName("HarpyShrineEntity")]
    public class VillageShrineEntity : BaseTileEntity {
        public VillagerType shrineType;

        public Circle villageZone;

        public int remainingRespawnItems;

        public int remainingRespawnTime;

        public int respawnTimeCap;

        //This isn't updated on the server, and is manually updated by the client in order
        //for parity between client and server.
        public int clientTimer;

        public override int ValidTileID => ModContent.TileType<VillageShrineTile>();

        public int CurrentHousedVillagersCount {
            get;
            private set;
        }

        public int CurrentValidHouses {
            get;
            private set;
        }

        private int _syncTimer;

        private List<Point16> _houseLocations;

        public const float DefaultVillageRadius = 90f * 16f;

        public const int EmptyVillageRespawnTime = 60 * 60 * 15;

        public const int FullVillageRespawnTime = 60 * 60 * 3;

        public override bool? PreValidTile(int i, int j) {
            Tile tile = Framing.GetTileSafely(i, j);

            return tile.HasTile && tile.TileType == ValidTileID && tile.TileFrameX % 72 == 0 && tile.TileFrameY == 0;
        }

        public override void Update() {
            //This is only here for backwards compatibility, if someone is loading a world from where
            //the shrines were the older HarpyShrineEntity type, then their VillageZone values will
            //be default and thus need to be fixed.
            if (villageZone == default) {
                InstantiateVillageZone();

                SyncDataToClients();
            }
            int villagerNPCType = NPCUtils.VillagerTypeToNPCType(shrineType);
            Circle tileVillageZone = villageZone.ToTileCoordinates();

            //Sync from server to clients every 10 seconds
            if (--_syncTimer <= 0) {
                _syncTimer = 60 * 10;

                CurrentHousedVillagersCount = HousingUtils.NPCCountHousedInZone(tileVillageZone, villagerNPCType);
                if (_houseLocations is null || !HousingUtils.LocationsValidForHousing(_houseLocations, villagerNPCType)) {
                    _houseLocations = HousingUtils.GetValidHousesInZone(tileVillageZone, villagerNPCType);
                    CurrentValidHouses = _houseLocations.Count;
                }

                respawnTimeCap = (int)MathHelper.Lerp(EmptyVillageRespawnTime, FullVillageRespawnTime, CurrentValidHouses > 0 ? CurrentHousedVillagersCount / (float)CurrentValidHouses : 0f);
                remainingRespawnTime = (int)MathHelper.Clamp(remainingRespawnTime, 0f, respawnTimeCap);

                SyncDataToClients();

                return;
            }

            remainingRespawnTime = (int)MathHelper.Clamp(remainingRespawnTime - 1, 0f, respawnTimeCap);

            //Natural Respawn Item regenerating
            if (remainingRespawnTime <= 0 && remainingRespawnItems <= CurrentValidHouses) {
                remainingRespawnTime = respawnTimeCap;
                remainingRespawnItems++;

                SyncDataToClients();
            }

            //NPC respawning
            if (CurrentHousedVillagersCount < CurrentValidHouses && remainingRespawnItems > 0) {
                Rectangle housingRectangle = tileVillageZone.ToRectangle();

                for (int i = 0; i < housingRectangle.Width; i += 2) {
                    for (int j = 0; j < housingRectangle.Height; j += 2) {
                        Point position = new Point(housingRectangle.X + i, housingRectangle.Y + j);

                        if (WorldGen.InWorld(position.X, position.Y) && WorldGen.StartRoomCheck(position.X, position.Y) && WorldGen.RoomNeeds(villagerNPCType)) {
                            WorldGen.ScoreRoom(npcTypeAskingToScoreRoom: villagerNPCType);

                            //A "high score" of 0 or less means the room is occupied or the score otherwise failed
                            if (WorldGen.hiScore <= 0) {
                                continue;
                            }

                            int npc = NPC.NewNPC(new EntitySource_SpawnNPC(), WorldGen.bestX * 16, WorldGen.bestY * 16, villagerNPCType);

                            Main.npc[npc].homeTileX = WorldGen.bestX;
                            Main.npc[npc].homeTileY = WorldGen.bestY;

                            Color arrivalColor = new Color(50, 125, 255);
                            string arrivalText = LocalizationUtils.GetLWMTextValue($"Event.VillagerRespawned.{shrineType}", new object[] { Main.npc[npc].GivenOrTypeName });
                            if (Main.netMode == NetmodeID.Server) {
                                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(arrivalText), arrivalColor);
                            }
                            else {
                                Main.NewText(arrivalText, arrivalColor);
                            }

                            remainingRespawnItems--;
                            CurrentHousedVillagersCount++;
                            if (CurrentHousedVillagersCount < CurrentValidHouses && remainingRespawnItems > 0) {
                                continue;
                            }

                            SyncDataToClients();
                            return;
                        }
                    }
                }

                SyncDataToClients();
            }
        }

        public override void NetSend(BinaryWriter writer) {
            writer.Write((int)shrineType);

            writer.WriteVector2(villageZone.center);
            writer.Write(villageZone.radius);

            writer.Write(remainingRespawnItems);
            writer.Write(remainingRespawnTime);
            writer.Write(respawnTimeCap);

            writer.Write(CurrentHousedVillagersCount);
            writer.Write(CurrentValidHouses);
        }

        public override void NetReceive(BinaryReader reader) {
            shrineType = (VillagerType)reader.ReadInt32();

            villageZone = new Circle(reader.ReadVector2(), reader.ReadSingle());
            remainingRespawnItems = reader.ReadInt32();
            remainingRespawnTime = reader.ReadInt32();
            respawnTimeCap = reader.ReadInt32();

            CurrentHousedVillagersCount = reader.ReadInt32();
            CurrentValidHouses = reader.ReadInt32();

            clientTimer = 0;
        }

        public override void OnNetPlace() {
            SyncDataToClients(false);
        }

        public override void SaveData(TagCompound tag) {
            tag["ShrineType"] = (int)shrineType;
            tag["RemainingItems"] = remainingRespawnItems;
            tag["RemainingTime"] = remainingRespawnTime;
        }

        public override void LoadData(TagCompound tag) {
            shrineType = (VillagerType)tag.GetInt("ShrineType");
            remainingRespawnItems = tag.GetInt("RemainingItems");
            remainingRespawnTime = tag.GetInt("RemainingTime");
            respawnTimeCap = EmptyVillageRespawnTime;

            InstantiateVillageZone();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 4, 5);

                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }

            int placedEntity = Place(i, j);
            if (TileEntityUtils.TryFindModEntity(placedEntity, out VillageShrineEntity entity)) {
                entity.InstantiateVillageZone();
                entity.shrineType = (VillagerType)style;
                entity.remainingRespawnTime = EmptyVillageRespawnTime;
                entity.respawnTimeCap = EmptyVillageRespawnTime;
            }

            return placedEntity;
        }

        /// <summary>
        /// Called when the tile this entity is associated with is right clicked.
        /// </summary>
        public void RightClicked() {
            VillageShrineUISystem shrineSystem = ModContent.GetInstance<VillageShrineUISystem>();

            switch (shrineSystem.correspondingInterface.CurrentState) {
                case null:
                case VillageShrineUIState state when state.CurrentEntity.Position != Position:
                    shrineSystem.OpenOrRegenShrineState(Position);
                    break;
                case VillageShrineUIState:
                    shrineSystem.CloseShrineState();
                    break;
            }
        }

        /// <summary>
        /// Forcefully triggers a village housing recalculation during the next update &amp; instantly syncs said information.
        /// Should be called on the Server in MP.
        /// </summary>
        public void ForceRecalculateAndSync() {
            _syncTimer = 0;
            _houseLocations = null;
        }

        /// <summary>
        /// Really simple method that just sets the village zone field to its proper values given
        /// the the tile entity's current position.
        /// </summary>
        private void InstantiateVillageZone() {
            villageZone = new Circle(Position.ToWorldCoordinates(32f, 40f), DefaultVillageRadius);
        }

        /// <summary>
        /// Little helper method that syncs this tile entity from Server to clients.
        /// </summary>
        /// <param name="doServerCheck"> Whether or not to check if the current Netmode is a Server. </param>
        private void SyncDataToClients(bool doServerCheck = true) {
            if (doServerCheck && Main.netMode != NetmodeID.Server) {
                return;
            }

            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }
    }
}