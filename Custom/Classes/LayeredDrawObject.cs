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
        public int[] currentTextureIndices;

        public LayeredDrawObject(Asset<Texture2D>[][] allLayerTextures, int[] textureIndices) {
            if (allLayerTextures.Length != textureIndices.Length) {
                throw new ArithmeticException("Draw Object arrays are not all the same length.");
            }

            this.allLayerTextures = allLayerTextures;
            currentTextureIndices = textureIndices;
        }

        /// <summary>
        /// Draws this object given the specified parameters.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Rectangle destinationRect, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, SpriteEffects spriteEffect, float layerDepth) {
            for (int i = 0; i < currentTextureIndices.Length; i++) {
                spriteBatch.Draw(allLayerTextures[i][currentTextureIndices[i]].Value, destinationRect, sourceRect, color, rotation, origin, spriteEffect, layerDepth);
            }
        }

        /// <summary>
        /// Draws this object given the specified parameters.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 drawPos, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects spriteEffect, float layerDepth) {
            for (int i = 0; i < currentTextureIndices.Length; i++) {
                spriteBatch.Draw(allLayerTextures[i][currentTextureIndices[i]].Value, drawPos, sourceRect, color, rotation, origin, scale, spriteEffect, layerDepth);
            }
        }

        /// <summary>
        /// Returns the frame width for all layers (since, logically, they should all be the same).
        /// </summary>
        public int GetFrameWidth() => allLayerTextures[0][0].Width();

        /// <summary>
        /// Calculates and returns the frame width for all layers (since, logically, they must all be the
        /// same).
        /// </summary>
        /// <param name="frameCount"> How many frames the texture is comprised of. </param>
        public int GetFrameHeight(int frameCount) => allLayerTextures[0][0].Height() / frameCount;
    }
}