using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;

namespace BuilderEssentials.UI.Elements
{
    public class CustomUIElement : UIElement
    {
        /// <summary>
        /// Get the current drawing scale.
        /// </summary>
        public float Scale { get; private set; } = 1;

        /// <summary>
        /// Get the current opacity.
        /// </summary>
        public float Opacity { get; private set; } = 0f;

        /// <summary>
        /// Get the currect dimensions of the UI Element. (Width and Height)
        /// </summary>
        public Vector2 Size { get; private set; } = Vector2.Zero;

        /// <summary>
        /// Get the current offset dimensions of the UI Element. (Left and Top)
        /// </summary>
        public Vector2 Offset { get; private set; } = Vector2.Zero;
        /// <summary>
        /// Get the current visibility state.
        /// </summary>
        public bool Visible { get; private set; } = true;

        private UIElement _parent;
        private bool _initialized = false;
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //TODO: Fix Size and Scale issues
            if (!_initialized)
            {
                SetSize(Width.Pixels, Height.Pixels);
                _initialized = true;
            }
            SetSize(Size);

            if (IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;
        }

        //GET/SET METHODS

        /// <summary>
        /// Sets the UIElement drawing scale.
        /// </summary>
        /// <param name="scale">Negative values will mirror the UIElement drawing by the x and y vertices.</param>
        public virtual void SetScale(float scale)
        {
            Scale = scale;
            SetSize(Size);
        }

        /// <summary>
        /// Sets the UIElement opacity.
        /// </summary>
        /// <param name="opacity">The bigger the value, the more transparent it will seem.</param>
        public virtual void SetOpacity(float opacity)
        {
            Opacity = opacity;
        }

        /// <summary>
        /// Sets the UIElement width/height dimensions.
        /// </summary>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        public virtual void SetSize(float width, float height)
        {
            Size = new Vector2(width, height);
            Width.Set(width * Scale, 0);
            Height.Set(height * Scale, 0);
        }

        /// <summary>
        /// Sets the UIElement width/height dimensions.
        /// </summary>
        /// <param name="size">A Vector2 where x: width and y: height.</param>
        public virtual void SetSize(Vector2 size)
        {
            Size = size;
            Width.Set(size.X * Scale, 0);
            Height.Set(size.Y * Scale, 0);
        }

        /// <summary>
        /// Sets the UIElement left/top offset dimensions.
        /// </summary>
        /// <param name="left">Left offset in pixels.</param>
        /// <param name="top">Top offset in pixels.</param>
        public virtual void SetOffset(float left, float top)
        {
            Offset = new Vector2(left, top);
            Left.Set(left, 0);
            Top.Set(top, 0);
        }
        
        /// <summary>
        /// Sets the UIElement left/top offset dimensions
        /// </summary>
        /// <param name="offset">A Vector2 where x: left and y: top.</param>
        public virtual void SetOffset(Vector2 offset)
        {
            Offset = offset;
            Left.Set(offset.X, 0);
            Top.Set(offset.Y, 0);
        }

        /// <summary>
        /// Hides the element if visible or shows the element if not.
        /// </summary>
        public virtual void Toggle()
        {
            if (Visible)
                Hide();
            else
                Show();
        }

        #region Events

        public delegate void ElementEvent(CustomUIElement affectedElement);

        public event ElementEvent OnShow;
        public event ElementEvent OnHide;

        /// <summary>
        /// Makes the element render and re-enables all of its functionality.
        /// </summary>
        public virtual void Show()
        {
            if (Visible) return;
            
            Visible = true;
            _parent?.Append(this);

            OnShow?.Invoke(this);
        }

        /// <summary>
        /// Makes the element not render and disables all of its functionality.
        /// </summary>
        public virtual void Hide()
        {
            if (!Visible) return;
            
            Visible = false;
            _parent = Parent;
            Remove();

            OnHide?.Invoke(this);
        }

        #endregion
    }
}