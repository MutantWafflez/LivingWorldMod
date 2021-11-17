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
        private Point16 _topLeft = Point16.NegativeOne;

        private Point16 _bottomRight = Point16.NegativeOne;

        private bool _isPlacingWalls;

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
            else if (player.altFunctionUse == 2 && !_isPlacingWalls && _topLeft != Point16.NegativeOne && _bottomRight != Point16.NegativeOne) {
                Main.NewText("Placing Skip Walls...");
                PlaceSkipWalls();
                return true;
            }

            return null;
        }

        public override bool AltFunctionUse(Player player) => true;

        private void PlaceSkipWalls() {
            _isPlacingWalls = true;

            for (int x = 0; x <= _bottomRight.X - _topLeft.X; x++) {
                for (int y = 0; y <= _bottomRight.Y - _topLeft.Y; y++) {
                    Tile requestedTile = Framing.GetTileSafely(x + _topLeft.X, y + _topLeft.Y);
                    if (requestedTile.wall == WallID.None) {
                        WorldGen.PlaceWall(x + _topLeft.X, y + _topLeft.Y, ModContent.WallType<SkipWall>());
                    }
                }
            }

            _isPlacingWalls = false;
            Main.NewText("Walls Placed!");
        }
    }
}