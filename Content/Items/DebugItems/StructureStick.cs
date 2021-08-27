#if DEBUG

using LivingWorldMod.Custom.Utilities;
using System;
using System.Collections.Generic;
using LivingWorldMod.Custom.Structs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.Items.DebugItems {

    public class StructureStick : BaseItem {
        private Point16 topLeft = Point16.NegativeOne;

        private Point16 bottomRight = Point16.NegativeOne;

        private bool isSaving;

        public override string Texture => "Terraria/Images/Item_" + ItemID.DrumStick;

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DrumStick);
            Item.rare = ItemRarityID.Quest;
            Item.autoReuse = false;
            Item.useTime = 30;
            Item.useAnimation = 30;
        }

        public override bool? UseItem(Player player) {
            if (player.altFunctionUse == 0) {
                if (topLeft == Point16.NegativeOne) {
                    topLeft = new Point16((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));
                    Main.NewText("Top Left Set to: " + topLeft.X + ", " + topLeft.Y);
                }
                else if (bottomRight == Point16.NegativeOne) {
                    bottomRight = new Point16((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));
                    Main.NewText("Bottom Right Set to: " + bottomRight.X + ", " + bottomRight.Y);
                }
                else {
                    topLeft = Point16.NegativeOne;
                    bottomRight = Point16.NegativeOne;
                    Main.NewText("Points Reset");
                }
                return true;
            }
            else if (player.altFunctionUse == 2 && !isSaving && topLeft != Point16.NegativeOne && bottomRight != Point16.NegativeOne) {
                Main.NewText("Saving Structure...");
                SaveStructure();
                return true;
            }

            return null;
        }

        public override bool AltFunctionUse(Player player) => true;

        private void SaveStructure() {
            isSaving = true;
            List<List<TileData>> tileData = new List<List<TileData>>();

            for (int x = 0; x <= bottomRight.X - topLeft.X; x++) {
                tileData.Add(new List<TileData>());
                for (int y = 0; y <= bottomRight.Y - topLeft.Y; y++) {
                    Tile requestedTile = Framing.GetTileSafely(x + topLeft.X, y + topLeft.Y);
                    tileData[x].Add(new TileData(requestedTile));
                }
            }

            StructureData structData = new StructureData(tileData.Count, tileData[0].Count, tileData);

            string outputPath = IOUtilities.GetLWMFilePath() + $"/StructureOutput_{DateTime.Now.ToShortTimeString().Replace(':', '_').Replace(' ', '_')}.struct";

            TagIO.ToFile(new TagCompound() { { "structureData", structData } }, outputPath);

            Main.NewText("Structure Copied to File!");
            isSaving = false;
        }
    }
}

#endif