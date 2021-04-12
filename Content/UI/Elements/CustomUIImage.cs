using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace LivingWorldMod.Content.UI.Elements
{
    public class CustomUIImage : CustomUIElement
    {
        /// <summary>
        /// Get the current image texture.
        /// </summary>
        public Texture2D ImageTexture { get; private set; }

        /// <summary>
        /// Get the currect image rotation.
        /// </summary>
        public float ImageRotation { get; private set; }

        /// <summary>
        /// Get the current scale to fit state.
        /// ----------------Scales the image to fit the current UIElement dimensions.
        /// </summary>
        public bool ScaleToFit { get; private set; }

        /// <summary>
        /// Get the current color rendering above the image.
        /// </summary>
        public Color OverlayColor { get; private set; }

        //--------------------------------------------------------------------------------------------------
        //--- Two ctors to make user aware that scale won't have any effect if scaleToFit is set to true ---
        //--------------------------------------------------------------------------------------------------

        /// <summary>
        /// </summary>
        /// <param name="texture"> UIImage's image texture. </param>
        /// <param name="scale"> UIImage's drawing scale. </param>
        /// <param name="opacity"> UIImage's opacity. (higher value, higher opacity) </param>
        /// <param name="imageRotation"> UIImage's image rotation. </param>
        /// <param name="overlayColor">
        /// UIImage's color rendered above the image texture. (default is transparent)
        /// </param>
        public CustomUIImage(Texture2D texture, float scale = 1f, float opacity = 1f, float imageRotation = 0f, Color overlayColor = default)
        {
            Initialize(texture, scale, opacity, imageRotation, false, overlayColor);
        }

        /// <summary>
        /// </summary>
        /// <param name="texture"> UIImage's image texture. </param>
        /// <param name="scaleToFit">
        /// Whether the UIImage image is scaled to fit the current UIElement dimensions or not.
        /// </param>
        /// <param name="opacity"> UIImage's opacity. (higher value, higher opacity) </param>
        /// <param name="imageRotation"> UIImage's image rotation. </param>
        /// <param name="overlayColor">
        /// UIImage's color rendered above the image texture. (default is transparent)
        /// </param>
        public CustomUIImage(Texture2D texture, bool scaleToFit = false, float opacity = 1f, float imageRotation = 0f, Color overlayColor = default)
        {
            Initialize(texture, 1f, opacity, imageRotation, scaleToFit, overlayColor);
        }

        public override void SetScale(float scale)
        {
            base.SetScale(scale);

            if (ImageTexture != null)
            {
                Width.Set(ImageTexture.Width * Scale, 0);
                Height.Set(ImageTexture.Height * Scale, 0);
            }
        }

        /// <summary>
        /// Set the CustomUIImage texture.
        /// </summary>
        /// <param name="texture"> A XNA Framework Texture2D object. </param>
        public void SetImage(Texture2D texture)
        {
            ImageTexture = texture ?? Main.inventoryBack9Texture;

            Height.Set(ImageTexture.Height * Scale, 0);
            Width.Set(ImageTexture.Width * Scale, 0);
        }

        //GET/SET METHODS
        /// <summary>
        /// Set the CustomUIImage rotation.
        /// </summary>
        /// <param name="rotation"> Rotation value (0f is the default). </param>
        public void SetImageRotation(float rotation)
        {
            ImageRotation = rotation;
        }

        /// <summary>
        /// Set the image to scale to fit the current UIElement dimensions.
        /// </summary>
        /// <param name="scaleToFit"> True makes the image scale to the UIElement Height/Width. </param>
        public void SetScaleToFit(bool scaleToFit)
        {
            ScaleToFit = scaleToFit;
        }

        /// <summary>
        /// Set the color rendered above the current texture.
        /// </summary>
        /// <param name="color"> A XNA Framework Color object. </param>
        public void SetOverlayColor(Color color)
        {
            OverlayColor = color == default ? Color.White : color;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (ScaleToFit)
            {
                spriteBatch.Draw(ImageTexture, GetDimensions().ToRectangle(), OverlayColor * Opacity);
                return;
            }
            Vector2 size = ImageTexture.Size();
            Vector2 topLeft = GetDimensions().Position() + size * (1f - Scale) / 2f;

            spriteBatch.Draw(ImageTexture, topLeft, null, OverlayColor * Opacity, ImageRotation, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }

        private void Initialize(Texture2D texture, float scale = 1f, float opacity = 1f, float imageRotation = 0f, bool scaleToFit = false, Color overlayColor = default)
        {
            SetImage(texture);
            SetScale(scale);
            SetOpacity(opacity);

            ImageRotation = imageRotation;
            ScaleToFit = scaleToFit;
            OverlayColor = overlayColor == default ? Color.White : overlayColor;
        }
    }
}