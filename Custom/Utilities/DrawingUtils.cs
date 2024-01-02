using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace LivingWorldMod.Custom.Utilities {
    /// <summary>
    /// Utilities class that has helper methods for drawing any assortment of things.
    /// </summary>
    public static class DrawingUtils {
        /// <summary>
        /// Draws a given texture with the specified armor shader id.
        /// </summary>
        /// <remarks>
        /// Note: Make sure the given sprite-batch is already started when this method is called; this method end and restarts it.
        /// </remarks>
        /// <param name="spriteBatch">
        /// The sprite-batch that will be used to draw with. Make sure it is already before this method
        /// is called; this method ends and restarts it.
        /// </param>
        /// <param name="texture"> The texture that will be drawn. </param>
        /// <param name="shaderID"> The given id for the armor shader that will be applied to the texture. </param>
        /// <param name="drawColor"> The underlying color that the texture will be drawn with. </param>
        /// <param name="destinationRectangle"> The rectangle where the texture to be drawn to. </param>
        /// <param name="sourceRectangle"> The region of the texture/s sprite that will actually be drawn. </param>
        /// <param name="origin"> The offset from the draw position. </param>
        /// <param name="rotation"> The rotation that the texture will be drawn with. </param>
        /// <param name="drawMatrix">
        /// The matrix that will be used to draw with; make sure to use the right one for UI and world
        /// drawing, where applicable.
        /// </param>
        public static void DrawTextureWithArmorShader(SpriteBatch spriteBatch, Texture2D texture, int shaderID, Color drawColor, Rectangle destinationRectangle, Rectangle sourceRectangle, Vector2 origin, float rotation, Matrix drawMatrix) {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, drawMatrix);

            DrawData itemDrawData = new(texture, destinationRectangle, sourceRectangle, drawColor, rotation, origin, SpriteEffects.None);
            GameShaders.Armor.Apply(shaderID, null, itemDrawData);

            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, drawColor, rotation, origin, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, drawMatrix);
        }
    }
}