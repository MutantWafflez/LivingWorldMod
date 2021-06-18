using System;
using System.IO;
using System.Text;
using LivingWorldMod.Custom.Utilities;
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
            else if (player.altFunctionUse == 2 && topLeft != Point16.NegativeOne && bottomRight != Point16.NegativeOne) {
                Main.NewText("Saving Structure...");
                SaveStructure();
                return true;
            }

            return null;
        }

        public override bool AltFunctionUse(Player player) => true;

        private void SaveStructure() {
            /* Relevant Tile Fields/Properties:
            tile.type;
            IsActivated
            IsHalfBlock;
            FrameNumber;
            frameX;
            frameY;
            tile.Slope;
            tile.Color;
            tile.IsActuated;
            tile.HasActuator;
            tile.RedWire;
            tile.BlueWire;
            tile.GreenWire;
            tile.YellowWire;
            tile.LiquidType;
            tile.LiquidAmount;
            tile.wall;
            tile.WallColor;
            tile.WallFrameNumber;
            ile.WallFrameX;
            tile.WallFrameY;
            */

            int xWidth = bottomRight.X - topLeft.X;
            int yHeight = bottomRight.Y - topLeft.Y;
            int[,] tileTypes = new int[xWidth, yHeight];
            bool[,] areTilesHalfBlocks = new bool[xWidth, yHeight];
            int[,] tileFrameNumbers = new int[xWidth, yHeight];
            int[,] tileFrameXs = new int[xWidth, yHeight];
            int[,] tileFrameYs = new int[xWidth, yHeight];
            int[,] tileSlopeTypes = new int[xWidth, yHeight];
            int[,] tileColors = new int[xWidth, yHeight];
            bool[,] areTilesActuated = new bool[xWidth, yHeight];
            bool[,] haveActuators = new bool[xWidth, yHeight];
            bool[,] haveRedWires = new bool[xWidth, yHeight];
            bool[,] haveBlueWires = new bool[xWidth, yHeight];
            bool[,] haveGreenWires = new bool[xWidth, yHeight];
            bool[,] haveYellowWires = new bool[xWidth, yHeight];
            int[,] liquidTypes = new int[xWidth, yHeight];
            int[,] liquidAmounts = new int[xWidth, yHeight];
            int[,] wallTypes = new int[xWidth, yHeight];
            int[,] wallColors = new int[xWidth, yHeight];
            int[,] wallFrameNumbers = new int[xWidth, yHeight];
            int[,] wallFrameXs = new int[xWidth, yHeight];
            int[,] wallFrameYs = new int[xWidth, yHeight];

            string PrintIntArray(int[,] array, string fieldName) {
                string fullLine = "        public override int[,] " + fieldName + " => new int[,] {\n";
                bool hasNonDefaultValue = false;

                for (int y = 0; y < yHeight; y++) {
                    string line = "            {";
                    for (int x = 0; x < xWidth; x++) {
                        if (array[x, y] != default) {
                            hasNonDefaultValue = true;
                        }
                        if (x == 0) {
                            line = String.Concat(line, array[x, y]);
                        }
                        else if (x != xWidth - 1) {
                            line = String.Concat(line, ", " + array[x, y]);
                        }
                        else {
                            line = String.Concat(line, ", " + array[x, y] + "},");
                        }
                    }

                    fullLine = String.Concat(fullLine, line + "\n");
                }

                fullLine += "        };\n\n";

                return hasNonDefaultValue ? fullLine : $"        public override int[,] {fieldName} => new int[TileTypes.GetLength(0),TileTypes.GetLength(1)]; //All default values\n\n";
            }

            string PrintBoolArray(bool[,] array, string fieldName) {
                string fullLine = "        public override bool[,] " + fieldName + " => new bool[,] {\n";
                bool hasNonDefaultValue = false;

                for (int y = 0; y < yHeight; y++) {
                    string line = "            {";
                    for (int x = 0; x < xWidth; x++) {
                        if (array[x, y] != default) {
                            hasNonDefaultValue = true;
                        }
                        if (x == 0) {
                            line = String.Concat(line, array[x, y].ToString().ToLower());
                        }
                        else if (x != xWidth - 1) {
                            line = String.Concat(line, ", " + array[x, y].ToString().ToLower());
                        }
                        else {
                            line = String.Concat(line, ", " + array[x, y].ToString().ToLower() + "},");
                        }
                    }

                    fullLine = String.Concat(fullLine, line + "\n");
                }

                fullLine += "        };\n\n";

                return hasNonDefaultValue ? fullLine : $"        public override bool[,] {fieldName} => new bool[TileTypes.GetLength(0),TileTypes.GetLength(1)]; //All default values\n\n";
            }

            for (int x = 0; x < xWidth; x++) {
                for (int y = 0; y < yHeight; y++) {
                    Tile requestedTile = Framing.GetTileSafely(x + topLeft.X, y + topLeft.Y);
                    tileTypes[x, y] = requestedTile.IsActive ? requestedTile.type : -1;
                    areTilesHalfBlocks[x, y] = requestedTile.IsHalfBlock;
                    tileFrameNumbers[x, y] = requestedTile.FrameNumber;
                    tileFrameXs[x, y] = requestedTile.frameX;
                    tileFrameYs[x, y] = requestedTile.frameY;
                    tileSlopeTypes[x, y] = (int)requestedTile.Slope;
                    tileColors[x, y] = requestedTile.Color;
                    areTilesActuated[x, y] = requestedTile.IsActuated;
                    haveActuators[x, y] = requestedTile.HasActuator;
                    haveRedWires[x, y] = requestedTile.RedWire;
                    haveBlueWires[x, y] = requestedTile.BlueWire;
                    haveGreenWires[x, y] = requestedTile.GreenWire;
                    haveYellowWires[x, y] = requestedTile.YellowWire;
                    liquidTypes[x, y] = requestedTile.LiquidType;
                    liquidAmounts[x, y] = requestedTile.LiquidAmount;
                    wallTypes[x, y] = requestedTile.wall;
                    wallColors[x, y] = requestedTile.WallColor;
                    wallFrameNumbers[x, y] = requestedTile.FrameNumber;
                    wallFrameXs[x, y] = requestedTile.WallFrameX;
                    wallFrameYs[x, y] = requestedTile.WallFrameY;
                }
            }

            StringBuilder stringCreator = new StringBuilder();

            stringCreator.Append("namespace LivingWorldMod.Custom.Classes.StructureCaches {" + Environment.NewLine);
            stringCreator.Append("    public sealed class InsertCacheNameHere : StructureCache {" + Environment.NewLine);

            stringCreator.Append(PrintIntArray(tileTypes, "TileTypes"));
            stringCreator.Append(PrintBoolArray(areTilesHalfBlocks, "AreTilesHalfBlocks"));
            stringCreator.Append(PrintIntArray(tileFrameNumbers, "TileFrameNumbers"));
            stringCreator.Append(PrintIntArray(tileFrameXs, "TileFrameXs"));
            stringCreator.Append(PrintIntArray(tileFrameYs, "TileFrameYs"));
            stringCreator.Append(PrintIntArray(tileSlopeTypes, "TileSlopeTypes"));
            stringCreator.Append(PrintIntArray(tileColors, "TileColors"));
            stringCreator.Append(PrintBoolArray(areTilesActuated, "AreTilesActuated"));
            stringCreator.Append(PrintBoolArray(haveActuators, "HaveActuators"));
            stringCreator.Append(PrintBoolArray(haveRedWires, "HaveRedWires"));
            stringCreator.Append(PrintBoolArray(haveBlueWires, "HaveBlueWires"));
            stringCreator.Append(PrintBoolArray(haveGreenWires, "HaveGreenWires"));
            stringCreator.Append(PrintBoolArray(haveYellowWires, "HaveYellowWires"));
            stringCreator.Append(PrintIntArray(liquidTypes, "LiquidTypes"));
            stringCreator.Append(PrintIntArray(liquidAmounts, "LiquidAmounts"));
            stringCreator.Append(PrintIntArray(wallTypes, "WallTypes"));
            stringCreator.Append(PrintIntArray(wallColors, "WallColors"));
            stringCreator.Append(PrintIntArray(wallFrameNumbers, "WallFrameNumbers"));
            stringCreator.Append(PrintIntArray(wallFrameXs, "WallFrameXs"));
            stringCreator.Append(PrintIntArray(wallFrameYs, "WallFrameYs"));

            stringCreator.Append("    }\n}");

            string outputPath = IOUtilities.GetLWMFilePath() + "/StructureOutput.txt";

            if (File.Exists(outputPath)) {
                File.Delete(outputPath);
            }

            File.AppendAllText(outputPath, stringCreator.ToString());

            Main.NewText("Structure Copied to File!");
        }
    }
}