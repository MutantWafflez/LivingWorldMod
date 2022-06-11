﻿using System.IO;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.Tiles.Interactables.VillageShrines;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TileEntities.Interactables.VillageShrines {
    /// <summary>
    /// Tile Entity within each village shrine of each type, which mainly handles whether or not a
    /// specified player is close enough to the specified shrine to be considered "within the village."
    /// </summary>
    [LegacyName("HarpyShrineEntity")]
    public class VillageShrineEntity : BaseTileEntity {
        public VillagerType shrineType;
        public override int ValidTileID => ModContent.TileType<VillageShrineTile>();

        public override bool? PreValidTile(int i, int j) {
            Tile tile = Framing.GetTileSafely(i, j);

            return tile.HasTile && tile.TileType == ValidTileID && tile.TileFrameX % 72 == 0 && tile.TileFrameY == 0;
        }

        public override void NetSend(BinaryWriter writer) {
            writer.Write((int)shrineType);
        }

        public override void NetReceive(BinaryReader reader) {
            shrineType = (VillagerType)reader.ReadInt32();
        }

        public override void OnNetPlace() {
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }

        public override void SaveData(TagCompound tag) {
            tag["ShrineType"] = (int)shrineType;
        }

        public override void LoadData(TagCompound tag) {
            shrineType = (VillagerType)tag.GetInt("ShrineType");
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            shrineType = (VillagerType)style;

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 4, 5);

                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }

            Point16 tileOrigin = ModContent.GetInstance<VillageShrineTile>().tileOrigin;
            int placedEntity = Place(i - tileOrigin.X, j - tileOrigin.Y);
            return placedEntity;
        }
    }
}