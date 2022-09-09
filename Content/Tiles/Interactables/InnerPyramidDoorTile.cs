using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Tiles.Interactables {
    /// <summary>
    /// Door that functions as the travel method between rooms WITHIN the Revamped Pyramid.
    /// </summary>
    public class InnerPyramidDoorTile : PyramidDoorTile {
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
            if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0) {
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
            }
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
            // Gets offscreen vector for different lighting modes
            Vector2 offscreenVector = new Vector2(Main.offScreenRange);
            if (Main.drawToScreen) {
                offscreenVector = Vector2.Zero;
            }

            // Double check that the tile exists
            Point point = new Point(i, j);
            Tile tile = Main.tile[point.X, point.Y];
            if (tile == null || !tile.HasTile) {
                return;
            }

            Asset<Texture2D> arrowAsset = TextureAssets.GolfBallArrow;
            Rectangle arrowFrame = arrowAsset.Frame(2, 1, 0, 0);
            Rectangle arrowOutlineFrame = arrowAsset.Frame(2, 1, 1, 0);
        }

        public override bool RightClick(int i, int j) => false;
    }
}