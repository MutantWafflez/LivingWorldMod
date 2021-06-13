using System;
using System.Text;
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

            string PrintIntArray(int[,] array) {
                string fullLine = "";
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

                    fullLine = String.Concat(fullLine, line + "\n");
                }

                return fullLine;
            }

            string PrintBoolArray(bool[,] array) {
                string fullLine = "";
                for (int y = 0; y < yHeight; y++) {
                    string line = "{";
                    for (int x = 0; x < xWidth; x++) {
                        if (x == 0) {
                            line = String.Concat(line, array[x, y].ToString().ToLower());
                        }
                        else if (x != xWidth - 1) {
                            line = String.Concat(line, "," + array[x, y].ToString().ToLower());
                        }
                        else {
                            line = String.Concat(line, "," + array[x, y].ToString().ToLower() + "},");
                        }
                    }

                    fullLine = String.Concat(fullLine, line + "\n");
                }

                return fullLine;
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
            stringCreator.Append("public sealed class InsertCacheNameHere : StructureCache {" + Environment.NewLine);

            stringCreator.Append("public override int[,] TileTypes => new int[,] {\n" + PrintIntArray(tileTypes) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override bool[,] AreTilesHalfBlocks => new bool[,] {\n" + PrintBoolArray(areTilesHalfBlocks) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] TileFrameNumbers => new int[,] {\n" + PrintIntArray(tileFrameNumbers) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] TileFrameXs => new int[,] {\n" + PrintIntArray(tileFrameXs) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] TileFrameYs => new int[,] {\n" + PrintIntArray(tileFrameYs) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] TileSlopeTypes => new int[,] {\n" + PrintIntArray(tileSlopeTypes) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] TileColors => new int[,] {\n" + PrintIntArray(tileColors) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override bool[,] AreTilesActuated => new bool[,] {\n" + PrintBoolArray(areTilesActuated) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override bool[,] HaveActuators => new bool[,] {\n" + PrintBoolArray(haveActuators) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override bool[,] HaveRedWires => new bool[,] {\n" + PrintBoolArray(haveRedWires) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override bool[,] HaveBlueWires => new bool[,] {\n" + PrintBoolArray(haveBlueWires) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override bool[,] HaveGreenWires => new bool[,] {\n" + PrintBoolArray(haveGreenWires) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override bool[,] HaveYellowWires => new bool[,] {\n" + PrintBoolArray(haveYellowWires) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] LiquidTypes => new int[,] {\n" + PrintIntArray(liquidTypes) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] LiquidAmounts => new int[,] {\n" + PrintIntArray(liquidAmounts) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] WallTypes => new int[,] {\n" + PrintIntArray(wallTypes) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] WallColors => new int[,] {\n" + PrintIntArray(wallColors) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] WallFrameNumbers => new int[,] {\n" + PrintIntArray(wallFrameNumbers) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] WallFrameXs => new int[,] {\n" + PrintIntArray(wallFrameXs) + "\n};" + Environment.NewLine);
            stringCreator.Append("public override int[,] WallFrameYs => new int[,] {\n" + PrintIntArray(wallFrameYs) + "\n};" + Environment.NewLine);

            stringCreator.Append("}\n}");

            Main.NewText("Structure Copied to Clipboard!");
        }
    }
}