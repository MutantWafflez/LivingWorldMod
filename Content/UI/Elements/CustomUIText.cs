using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements
{
    public class CustomUIText : UIText
    {
        private UIElement _parent;

        /// <summary>
        /// Get the current visibility state.
        /// </summary>
        public bool Visible { get; private set; }

        public CustomUIText(string text, float textScale = 1, bool large = false) : base(text, textScale, large)
        {
        }

        public CustomUIText(LocalizedText text, float textScale = 1, bool large = false) : base(text, textScale, large)
        {
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
        /// Makes the element render and re-enables all of its functionality
        /// </summary>
        public virtual void Show()
        {
            Visible = true;
            _parent?.Append(this);

            OnShow?.Invoke(null);
        }

        /// <summary>
        /// Makes the element not render and disables all of its functionality. (still takes up space)
        /// </summary>
        public virtual void Hide()
        {
            Visible = false;
            _parent = Parent ?? _parent;
            Remove();

            OnHide?.Invoke(null);
        }

        #endregion Events
    }
}