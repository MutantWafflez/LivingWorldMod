using System.Collections.Generic;
using LivingWorldMod.Content.Items.Placeables.Interactables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.Tiles.Interactables {

    public class VillageShrineTile : BaseTile {

        /// <summary>
        /// Handle little dictionary used to determine what item should be dropped based on the
        /// frameX of the tile.
        /// </summary>
        public Dictionary<int, int> placeStyleToItemType;

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
            TileObjectData.newTile.Origin = new Point16(1, 4);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16 };
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            placeStyleToItemType = new Dictionary<int, int>() {
                {0, ModContent.ItemType<HarpyShrineItem>()}
            };

            AnimationFrameHeight = 90;

            //TODO: Proper localization
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Village Shrine");
            AddMapEntry(new Color(255, 255, 0), name);
        }

        public override bool HasSmartInteract() => true;

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Rectangle dropZone = new Rectangle(i * 16, (j + 4) * 16, 4, 5);

            Item.NewItem(dropZone, placeStyleToItemType[frameX]);
        }
    }
}