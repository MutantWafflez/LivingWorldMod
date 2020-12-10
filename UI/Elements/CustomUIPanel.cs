using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;

namespace BuilderEssentials.UI.Elements
{
    public class CustomUIPanel : CustomUIElement
    {
        private const int CornerSize = 12;
        private const int BarSize = 4;

        /// <summary>
        /// Get the current background texture.
        /// </summary>
        public Texture2D BackgroundTexture { get; private set; }

        /// <summary>
        /// Get the current border texture.
        /// </summary>
        public Texture2D BorderTexture { get; private set; }

        /// <summary>
        /// Get/Set the current background color.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Get/Set the current border color.
        /// </summary>
        public Color BorderColor { get; set; }

        /// <summary></summary>
        /// <param name="scale">UIPanel's drawing scale</param>
        /// <param name="opacity">UIPanel's opacity. (higher value, higher opacity)</param>
        public CustomUIPanel(float scale = 1f, float opacity = 1f)
        {
            SetScale(scale);
            SetOpacity(opacity);

            SetBackgroundTexture(null);
            SetBorderTexture(null);
            BackgroundColor = new Color(63, 82, 151);
            BorderColor = Color.Black;
            SetPadding(12);
        }

        private void DrawPanel(SpriteBatch spriteBatch, Texture2D texture, Color color)
        {
            //Vanilla code
            CalculatedStyle dimensions = GetDimensions();
            Point point = new Point((int)dimensions.X, (int)dimensions.Y);
            Point point2 = new Point(point.X + (int)dimensions.Width - CornerSize, point.Y + (int)dimensions.Height - CornerSize);
            int width = point2.X - point.X - CornerSize;
            int height = point2.Y - point.Y - CornerSize;
            spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, CornerSize, CornerSize), new Rectangle?(new Rectangle(0, 0, CornerSize, CornerSize)), color);
            spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, CornerSize, CornerSize), new Rectangle?(new Rectangle(CornerSize + BarSize, 0, CornerSize, CornerSize)), color);
            spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, CornerSize, CornerSize), new Rectangle?(new Rectangle(0, CornerSize + BarSize, CornerSize, CornerSize)), color);
            spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, CornerSize, CornerSize), new Rectangle?(new Rectangle(CornerSize + BarSize, CornerSize + BarSize, CornerSize, CornerSize)), color);
            spriteBatch.Draw(texture, new Rectangle(point.X + CornerSize, point.Y, width, CornerSize), new Rectangle?(new Rectangle(CornerSize, 0, BarSize, CornerSize)), color);
            spriteBatch.Draw(texture, new Rectangle(point.X + CornerSize, point2.Y, width, CornerSize), new Rectangle?(new Rectangle(CornerSize, CornerSize + BarSize, BarSize, CornerSize)), color);
            spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + CornerSize, CornerSize, height), new Rectangle?(new Rectangle(0, CornerSize, CornerSize, BarSize)), color);
            spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + CornerSize, CornerSize, height), new Rectangle?(new Rectangle(CornerSize + BarSize, CornerSize, CornerSize, BarSize)), color);
            spriteBatch.Draw(texture, new Rectangle(point.X + CornerSize, point.Y + CornerSize, width, height), new Rectangle?(new Rectangle(CornerSize, CornerSize, BarSize, BarSize)), color);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (BackgroundTexture != null)
                DrawPanel(spriteBatch, BackgroundTexture, BackgroundColor * Opacity);

            if (BorderTexture != null)
                DrawPanel(spriteBatch, BorderTexture, BorderColor * Opacity);
        }

        //GET/SET METHODS

        /// <summary>
        /// Sets the UIPanel background texture.
        /// </summary>
        /// <param name="texture">A XNA Framework Texture2D object.</param>
        public void SetBackgroundTexture(Texture2D texture)
        {
            BackgroundTexture = texture ?? ModContent.GetTexture("Terraria/UI/PanelBackground");
        }

        /// <summary>
        /// Sets the UIPanel border texture.
        /// </summary>
        /// <param name="texture">A XNA Framework Texture2D object.</param>
        public void SetBorderTexture(Texture2D texture)
        {
            BorderTexture = texture ?? ModContent.GetTexture("Terraria/UI/PanelBorder");
        }
    }
}