using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.VillageShrine {
    /// <summary>
    /// UIState that handles the UI for the village shrine for each type of village.
    /// </summary>
    public class VillageShrineUIState : UIState {
        public VillagerType currentShrineType;

        public Vector2 centerShrinePosition;

        public UIImage backPanel;

        public static readonly string SpritePath = $"{LivingWorldMod.LWMSpritePath}UI/ShrineUI/";

        public override void OnInitialize() {
            currentShrineType = VillagerType.Harpy;

            backPanel = new UIImage(ModContent.Request<Texture2D>($"{SpritePath}BackPanel"));
            backPanel.Width = backPanel.Height = new StyleDimension(160f, 0f);
            Append(backPanel);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            CalculatedStyle panelDimensions = backPanel.GetDimensions();

            backPanel.Left.Set(centerShrinePosition.X - panelDimensions.Width / 2f - Main.screenPosition.X, 0f);
            backPanel.Top.Set(centerShrinePosition.Y - 40f - panelDimensions.Height - Main.screenPosition.Y, 0f);
        }

        /// <summary>
        /// Regenerates this UI state with the new passed in shrine type and shrine position.
        /// </summary>
        /// <param name="newShrineType"> The new shrine type to swap this state to. </param>
        /// <param name="centerShrinePos"> The WORLD position of where to move this state to, which should be the center of the shrine tile. </param>
        public void RegenState(VillagerType newShrineType, Vector2 centerShrinePos) {
            currentShrineType = newShrineType;
            centerShrinePosition = centerShrinePos;
        }
    }
}