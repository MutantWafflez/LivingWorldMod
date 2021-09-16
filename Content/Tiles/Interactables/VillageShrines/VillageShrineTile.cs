using System;
using LivingWorldMod.Content.TileEntities.VillageShrines;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Interactables.VillageShrines {

    /// <summary>
    /// Abstract class that exists to be inherited from to create different type of shrines for each
    /// type of existing villager type.
    /// </summary>
    public abstract class VillageShrineTile : BaseTile {

        /// <summary>
        /// The item that will drop from this shrine tile when it is broken.
        /// </summary>
        public abstract int ItemDropType {
            get;
        }

        /// <summary>
        /// The instance of the specific shrine tile entity that is tied to this shrine type, which
        /// handles biome management and villager housing, amongst other things.
        /// </summary>
        public abstract VillageShrineEntity ShrineEntity { get; }

        /// <summary>
        /// The "origin" of the tile with primary usage dealing with placement of the shrine and its
        /// respective tile entity.
        /// </summary>
        public virtual Point16 TileOrigin => new Point16(1, 2);

        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileLavaDeath[Type] = false;

            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.Origin = TileOrigin;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16 };
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            AnimationFrameHeight = 90;

            //TODO: Proper localization
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Village Shrine");
            AddMapEntry(new Color(255, 255, 0), name);
        }

        public override bool HasSmartInteract() => true;

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Rectangle dropZone = new Rectangle(i * 16, j * 16, 4, 5);

            Item.NewItem(dropZone, ItemDropType);

            VillageShrineEntity entity = ShrineEntity;

            //The i and j coordinates when breaking are the top left of the tile, and upon placing the entity is centered around the tile origin, so we must adjust the coordinates to be where the entity actually is
            entity.Kill(i + TileOrigin.X, j + TileOrigin.Y);
        }

        public override void PlaceInWorld(int i, int j, Item item) {
            VillageShrineEntity entity = ShrineEntity;

            if (entity.Find(i, j) < 0) {
                int entityID = entity.Place(i, j);

                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendData(MessageID.TileEntitySharing, ignoreClient: Main.myPlayer, number: entityID);
                }
            }
        }
    }
}