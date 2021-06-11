using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.DebugItems {

    public class StructureStick : ModItem {
        private Point16 topLeft = Point16.NegativeOne;

        private Point16 bottomRight = Point16.NegativeOne;

        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DrumStick);
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
            else if (player.altFunctionUse == 2) {
                if (topLeft != Point16.NegativeOne && bottomRight != Point16.NegativeOne) {
                    Main.NewText("Saving Structure...");
                    SaveStructure();
                }
                else {
                    Tile selectedTile = Framing.GetTileSafely((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));
                }

                return true;
            }

            return null;
        }

        public override bool AltFunctionUse(Player player) => true;

        private void SaveStructure() {
            int xWidth = bottomRight.X - topLeft.X;
            int yHeight = bottomRight.Y - topLeft.Y;
            int[,] savedTileStructure = new int[xWidth, yHeight];
            int[,] savedSlopeStructure = new int[xWidth, yHeight];
            int[,] savedWallStructure = new int[xWidth, yHeight];
            int[,] savedLiquidStructure = new int[xWidth, yHeight];
            int[,] savedXFrameStructure = new int[xWidth, yHeight];
            int[,] savedYFrameStructure = new int[xWidth, yHeight];

            void PrintToConsole(string title, int[,] array) {
                LivingWorldMod.Instance.Logger.Info(title);
                for (int y = 0; y < yHeight; y++) {
                    string line = "{";
                    for (int x = 0; x < xWidth; x++) {
                        if (x == 0) {
                            line = String.Concat(line, array[x, y]);
                        }
                        else if (x != xWidth - 1) {
                            line = String.Concat(line, "," + array[x, y]);
                        }
                        else {
                            line = String.Concat(line, "," + array[x, y] + "},");
                        }
                    }
                    LivingWorldMod.Instance.Logger.Info(line);
                }
            }

            for (int x = 0; x < xWidth; x++) {
                for (int y = 0; y < yHeight; y++) {
                    Tile requestedTile = Framing.GetTileSafely(x + topLeft.X, y + topLeft.Y);
                    savedTileStructure[x, y] = requestedTile.IsActive ? requestedTile.type : -1;
                    savedSlopeStructure[x, y] = (int)requestedTile.Slope;
                    savedWallStructure[x, y] = requestedTile.wall;
                    savedLiquidStructure[x, y] = requestedTile.LiquidAmount;
                    savedXFrameStructure[x, y] = requestedTile.frameX;
                    savedYFrameStructure[x, y] = requestedTile.frameY;
                }
            }

            PrintToConsole("Tiles:", savedTileStructure);
            PrintToConsole("Slopes:", savedSlopeStructure);
            PrintToConsole("Walls:", savedWallStructure);
            PrintToConsole("Liquids:", savedLiquidStructure);
            PrintToConsole("XFrames:", savedXFrameStructure);
            PrintToConsole("YFrames:", savedYFrameStructure);

            Main.NewText("Structure Saved!");
        }
    }
}