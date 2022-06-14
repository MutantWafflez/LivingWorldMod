using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Structs;
using Microsoft.Xna.Framework;
using System.IO;
using LivingWorldMod.Common.Systems.UI;
using LivingWorldMod.Content.Tiles.Interactables;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TileEntities.Interactables {
    /// <summary>
    /// Tile Entity within each village shrine of each type, which mainly handles whether or not a
    /// specified player is close enough to the specified shrine to be considered "within the village."
    /// </summary>
    [LegacyName("HarpyShrineEntity")]
    public class VillageShrineEntity : BaseTileEntity {
        public VillagerType shrineType;
        public Circle villageZone;

        public override int ValidTileID => ModContent.TileType<VillageShrineTile>();

        public const float DefaultVillageRadius = 1360f;

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

                //This method has the same functionality we want, so I'll just save the boilerplate
                OnNetPlace();
            }
        }

        public override void NetSend(BinaryWriter writer) {
            writer.Write((int)shrineType);

            writer.WriteVector2(villageZone.center);
            writer.Write(villageZone.radius);
        }

        public override void NetReceive(BinaryReader reader) {
            shrineType = (VillagerType)reader.ReadInt32();

            villageZone = new Circle(reader.ReadVector2(), reader.ReadSingle());
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

            InstantiateVillageZone();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            shrineType = (VillagerType)style;

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 4, 5);

                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }

            Point16 tileOrigin = ModContent.GetInstance<VillageShrineTile>().tileOrigin;
            int placedEntity = Place(i - tileOrigin.X, j - tileOrigin.Y);
            if (placedEntity != -1) {
                (ByID[placedEntity] as VillageShrineEntity)!.InstantiateVillageZone();
            }

            return placedEntity;
        }

        /// <summary>
        /// Called when the tile this entity is associated with is right clicked.
        /// </summary>
        public void RightClicked() {
            ModContent.GetInstance<VillageShrineUISystem>().OpenOrRegenShrineState(shrineType, Position.ToWorldCoordinates(32f, 40f));
        }

        /// <summary>
        /// Really simple method that just sets the village zone field to its proper values given
        /// the the tile entity's current position.
        /// </summary>
        private void InstantiateVillageZone() {
            villageZone = new Circle(WorldPosition + new Vector2(40f), DefaultVillageRadius);
        }
    }
}