using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.UI.Elements
{
    public class CustomUIImageButton : CustomUIImage
    {
        /// <summary>
        /// Whether the UIElement will behave like a toggleable or not.
        /// </summary>
        public bool Toggleable { get; private set; }

        /// <summary>
        /// Whether the UIElement was toggled or not.
        /// </summary>
        public bool Toggled { get; private set; }

        public CustomUIImageButton(Texture2D texture, float scale = 1, float opacity = 1, float imageRotation = 0,
            Color overlayColor = default) : base(texture, scale, opacity, imageRotation, overlayColor)
        {
            Init();
        }

        public CustomUIImageButton(Texture2D texture, bool scaleToFit = false, float opacity = 1,
            float imageRotation = 0, Color overlayColor = default) : base(texture, scaleToFit, opacity, imageRotation,
            overlayColor)
        {
            Init();
        }

        /// <summary>
        /// Sets the Toggleable state of the UIElement.
        /// </summary>
        /// <param name="isToggleable"> If true, UIElement is toggleable. </param>
        public void SetToggleable(bool isToggleable)
        {
            Toggleable = isToggleable;
        }

        /// <summary>
        /// Used to manually modify the Toggled value. (Should not be used unless necessary)
        /// </summary>
        /// <param name="toggleValue"> If true, UIelement is toggled on. </param>
        public void SetToggled(bool toggleValue)
        {
            Toggled = toggleValue;
        }

        private void Init()
        {
            OnMouseOver += (__, _) =>
            {
                if (Main.LocalPlayer.whoAmI != Main.myPlayer) return;
                Main.PlaySound(SoundID.MenuTick);
            };

            SetOpacity(.45f);

            OnMouseDown += (__, _) =>
            {
                if (Toggleable) Toggled = !Toggled;
                SetOpacity(Toggled ? 1f : .75f);
            };
            OnMouseOver += (__, _) => { if (!Toggled) SetOpacity(.75f); };
            OnMouseOut += (__, _) => { if (!Toggled) SetOpacity(.45f); };
        }
    }
}