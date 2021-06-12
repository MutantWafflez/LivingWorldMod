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
            else if (player.altFunctionUse == 2 && topLeft != Point16.NegativeOne && bottomRight != Point16.NegativeOne) {
                Main.NewText("Saving Structure...");
                SaveStructure();
                return true;
            }

            return null;
        }

        public override bool AltFunctionUse(Player player) => true;

        private void SaveStructure() {
            /* Tile Fields/Properties:
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
            tile.CheckingLiquid;
            tile.SkipLiquid;
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
            bool[,] areCheckingLiquids = new bool[xWidth, yHeight];
            bool[,] areSkippingLiquids = new bool[xWidth, yHeight];
            int[,] liquidTypes = new int[xWidth, yHeight];
            int[,] liquidAmounts = new int[xWidth, yHeight];
            int[,] wallTypes = new int[xWidth, yHeight];
            int[,] wallColors = new int[xWidth, yHeight];
            int[,] wallFrameNumbers = new int[xWidth, yHeight];
            int[,] wallFrameXs = new int[xWidth, yHeight];
            int[,] wallFrameYs = new int[xWidth, yHeight];

            void PrintIntToConsole(string title, int[,] array) {
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

            void PrintBoolToConsole(string title, bool[,] array) {
                LivingWorldMod.Instance.Logger.Info(title);
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
                    LivingWorldMod.Instance.Logger.Info(line);
                }
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
                    areCheckingLiquids[x, y] = requestedTile.CheckingLiquid;
                    areSkippingLiquids[x, y] = requestedTile.SkipLiquid;
                    liquidTypes[x, y] = requestedTile.LiquidType;
                    liquidAmounts[x, y] = requestedTile.LiquidAmount;
                    wallTypes[x, y] = requestedTile.wall;
                    wallColors[x, y] = requestedTile.WallColor;
                    wallFrameNumbers[x, y] = requestedTile.FrameNumber;
                    wallFrameXs[x, y] = requestedTile.WallFrameX;
                    wallFrameYs[x, y] = requestedTile.WallFrameY;
                }
            }

            PrintIntToConsole("Tile Types", tileTypes);
            PrintBoolToConsole("Half Blocks", areTilesHalfBlocks);
            PrintIntToConsole("Tile Frame Numbers", tileFrameNumbers);
            PrintIntToConsole("Tile Frame X's", tileFrameXs);
            PrintIntToConsole("Tile Frame Y's", tileFrameYs);
            PrintIntToConsole("Tile Slope Types", tileSlopeTypes);
            PrintIntToConsole("Tile Colors", tileColors);
            PrintBoolToConsole("Tile Is Actuated", areTilesActuated);
            PrintBoolToConsole("Tile Have Actuators", haveActuators);
            PrintBoolToConsole("Tile Have Red Wire", haveRedWires);
            PrintBoolToConsole("Tile Have Blue Wire", haveBlueWires);
            PrintBoolToConsole("Tile Have Green Wire", haveGreenWires);
            PrintBoolToConsole("Tile Have Yellow Wire", haveYellowWires);
            PrintBoolToConsole("Are Checking Liquids", areCheckingLiquids);
            PrintBoolToConsole("Are Skipping Liquids", areSkippingLiquids);
            PrintIntToConsole("Liquid Types", liquidTypes);
            PrintIntToConsole("Liquid Amounts", liquidAmounts);
            PrintIntToConsole("Wall Types", wallTypes);
            PrintIntToConsole("Wall Colors", wallColors);
            PrintIntToConsole("Wall Frame Numbers", wallFrameNumbers);
            PrintIntToConsole("Wall Frame Xs", wallFrameXs);
            PrintIntToConsole("Wall Frame Ys", wallFrameYs);

            Main.NewText("Structure Saved!");
        }
    }
}