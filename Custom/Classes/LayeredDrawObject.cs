using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;

namespace LivingWorldMod.Custom.Classes {
    /// <summary>
    /// Class that assists with drawing an object that consists of multiple layers. All of the arrays are
    /// in the same order of layers; for example, index 1 of each array relates to layer 1.
    /// </summary>
    public class LayeredDrawObject {
        /// <summary>
        /// 2D jagged array of all textures for each layer.
        /// </summary>
        public readonly Asset<Texture2D>[][] allLayerTextures;

        /// <summary>
        /// What texture index is used for each layer during drawing.
        /// </summary>
        public int[] drawIndices;

        public LayeredDrawObject(Asset<Texture2D>[][] allLayerTextures, int[] textureIndices) {
            if (allLayerTextures.Length != textureIndices.Length) {
                throw new ArgumentOutOfRangeException(nameof(allLayerTextures), "Draw Object arrays are not all the same length.");
            }

            this.allLayerTextures = allLayerTextures;
            drawIndices = textureIndices;
        }

        /// <summary>
        /// Draws this object given the specified parameters.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Rectangle destinationRect, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, SpriteEffects spriteEffect, float layerDepth) {
            for (int i = 0; i < drawIndices.Length; i++) {
                spriteBatch.Draw(allLayerTextures[i][drawIndices[i]].Value, destinationRect, sourceRect, color, rotation, origin, spriteEffect, layerDepth);
            }
        }

        /// <summary>
        /// Draws this object given the specified parameters.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 drawPos, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects spriteEffect, float layerDepth) {
            for (int i = 0; i < drawIndices.Length; i++) {
                spriteBatch.Draw(allLayerTextures[i][drawIndices[i]].Value, drawPos, sourceRect, color, rotation, origin, scale, spriteEffect, layerDepth);
            }
        }

        /// <summary>
        /// Draws this object given the specified parameters. This override allows customization of framing for each
        /// layer.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Rectangle destinationRect, Rectangle?[] sourceRects, Color color, float rotation, Vector2 origin, SpriteEffects spriteEffect, float layerDepth) {
            for (int i = 0; i < drawIndices.Length; i++) {
                spriteBatch.Draw(allLayerTextures[i][drawIndices[i]].Value, destinationRect, sourceRects[i], color, rotation, origin, spriteEffect, layerDepth);
            }
        }

        /// <summary>
        /// Returns the frame width for all layers (since, in most cases, they are the same).
        /// </summary>
        public int GetFrameWidth() => allLayerTextures[0][0].Width();

        /// <summary>
        /// Calculates and returns the frame width for all layers (since, in most cases, they are the same).
        /// </summary>
        /// <param name="frameCount"> How many frames the texture is comprised of. </param>
        public int GetFrameHeight(int frameCount = 1) => allLayerTextures[0][0].Height() / frameCount;
    }
}