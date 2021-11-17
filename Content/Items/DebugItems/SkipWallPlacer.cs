using LivingWorldMod.Content.Walls.DebugWalls;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Items.DebugItems {
    /// <summary>
    /// Debug tool that can place debug walls
    /// </summary>
    public class SkipWallPlacer : DebugItem {
        public override string Texture => "Terraria/Images/Item_" + ItemID.ActuationRod;
        private Point16 topLeft = Point16.NegativeOne;

        private Point16 bottomRight = Point16.NegativeOne;

        private bool isPlacingWalls;

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
            else if (player.altFunctionUse == 2 && !isPlacingWalls && topLeft != Point16.NegativeOne && bottomRight != Point16.NegativeOne) {
                Main.NewText("Placing Skip Walls...");
                PlaceSkipWalls();
                return true;
            }

            return null;
        }

        public override bool AltFunctionUse(Player player) => true;

        private void PlaceSkipWalls() {
            isPlacingWalls = true;

            for (int x = 0; x <= bottomRight.X - topLeft.X; x++) {
                for (int y = 0; y <= bottomRight.Y - topLeft.Y; y++) {
                    Tile requestedTile = Framing.GetTileSafely(x + topLeft.X, y + topLeft.Y);
                    if (requestedTile.wall == WallID.None) {
                        WorldGen.PlaceWall(x + topLeft.X, y + topLeft.Y, ModContent.WallType<SkipWall>());
                    }
                }
            }

            isPlacingWalls = false;
            Main.NewText("Walls Placed!");
        }
    }
}