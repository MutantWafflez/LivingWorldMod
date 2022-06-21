using LivingWorldMod.Content.Items.Miscellaneous;
using LivingWorldMod.Content.UI.CommonElements;
using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.VillageShrine {
    /// <summary>
    /// UIState that handles the UI for the village shrine for each type of village.
    /// </summary>
    //TODO: Add functionality for other villages when they're added
    public class VillageShrineUIState : UIState {
        public VillagerType currentShrineType;

        public Vector2 centerShrinePosition;

        public UIPanel backPanel;

        public UIPanel itemPanel;

        public UIBetterItemIcon respawnItemDisplay;

        public override void OnInitialize() {
            currentShrineType = VillagerType.Harpy;

            Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
            Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}UI/Elements/GradientPanelBorder");
            Asset<Texture2D> shadowedPanelBorder = ModContent.Request<Texture2D>($"{LivingWorldMod.LWMSpritePath}UI/Elements/ShadowedPanelBorder");

            backPanel = new UIPanel(vanillaPanelBackground, gradientPanelBorder) {
                BackgroundColor = new Color(59, 97, 203),
                BorderColor = Color.White
            };
            backPanel.Width = backPanel.Height = new StyleDimension(194f, 0f);

            itemPanel = new UIPanel(vanillaPanelBackground, shadowedPanelBorder) {
                BackgroundColor = new Color(46, 46, 159),
                BorderColor = new Color(22, 29, 107)
            };
            itemPanel.Width = itemPanel.Height = new StyleDimension(48f, 0f);
            backPanel.Append(itemPanel);

            respawnItemDisplay = new UIBetterItemIcon(new Item(), 48f, true) {
                overrideDrawColor = Color.White * 0.45f
            };
            respawnItemDisplay.Width = respawnItemDisplay.Height = itemPanel.Width;
            itemPanel.Append(respawnItemDisplay);

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
            respawnItemDisplay.SetItem(ModContent.ItemType<HarpyEgg>());
        }
    }
}