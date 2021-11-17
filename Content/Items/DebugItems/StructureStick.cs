using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.Items.DebugItems {
    public class StructureStick : DebugItem {
        public override string Texture => "Terraria/Images/Item_" + ItemID.DrumStick;
        private Point16 _topLeft = Point16.NegativeOne;

        private Point16 _bottomRight = Point16.NegativeOne;

        private bool _isSaving;

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DrumStick);
            Item.autoReuse = false;
            Item.useTime = 30;
            Item.useAnimation = 30;
        }

        public override bool? UseItem(Player player) {
            if (player.altFunctionUse == 0) {
                if (_topLeft == Point16.NegativeOne) {
                    _topLeft = new Point16((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));
                    Main.NewText("Top Left Set to: " + _topLeft.X + ", " + _topLeft.Y);
                }
                else if (_bottomRight == Point16.NegativeOne) {
                    _bottomRight = new Point16((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));
                    Main.NewText("Bottom Right Set to: " + _bottomRight.X + ", " + _bottomRight.Y);
                }
                else {
                    _topLeft = Point16.NegativeOne;
                    _bottomRight = Point16.NegativeOne;
                    Main.NewText("Points Reset");
                }
                return true;
            }
            else if (player.altFunctionUse == 2 && !_isSaving && _topLeft != Point16.NegativeOne && _bottomRight != Point16.NegativeOne) {
                Main.NewText("Saving Structure...");
                SaveStructure();
                return true;
            }

            return null;
        }

        public override bool AltFunctionUse(Player player) => true;

        private void SaveStructure() {
            _isSaving = true;
            List<List<TileData>> tileData = new List<List<TileData>>();

            for (int x = 0; x <= _bottomRight.X - _topLeft.X; x++) {
                tileData.Add(new List<TileData>());
                for (int y = 0; y <= _bottomRight.Y - _topLeft.Y; y++) {
                    Tile requestedTile = Framing.GetTileSafely(x + _topLeft.X, y + _topLeft.Y);
                    tileData[x].Add(new TileData(requestedTile));
                }
            }

            StructureData structData = new StructureData(tileData.Count, tileData[0].Count, tileData);

            string outputPath = IOUtils.GetLWMFilePath() + $"/StructureOutput_{DateTime.Now.ToShortTimeString().Replace(':', '_').Replace(' ', '_')}.struct";

            TagIO.ToFile(new TagCompound() { { "structureData", structData } }, outputPath);

            Main.NewText("Structure Copied to File!");
            _isSaving = false;
        }
    }
}