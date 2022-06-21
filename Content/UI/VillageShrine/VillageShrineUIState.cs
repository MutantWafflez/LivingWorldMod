using LivingWorldMod.Content.TileEntities.Interactables;
using LivingWorldMod.Content.UI.CommonElements;
using LivingWorldMod.Custom.Utilities;
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
    public class VillageShrineUIState : UIState {
        public UIPanel backPanel;

        public UIPanel itemPanel;

        public UIBetterItemIcon respawnItemDisplay;

        public UIBetterText respawnItemCount;

        public VillageShrineEntity CurrentEntity {
            get;
            private set;
        }

        public override void OnInitialize() {
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

            respawnItemCount = new UIBetterText("0") {
                horizontalTextConstraint = itemPanel.Width.Pixels,
                HAlign = 0.5f,
                VAlign = 0.5f
            };
            itemPanel.Append(respawnItemCount);

            Append(backPanel);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            CalculatedStyle panelDimensions = backPanel.GetDimensions();
            Vector2 centerOfEntity = CurrentEntity.Position.ToWorldCoordinates(32f, 40f);

            backPanel.Left.Set(centerOfEntity.X - panelDimensions.Width / 2f - Main.screenPosition.X, 0f);
            backPanel.Top.Set(centerOfEntity.Y - 40f - panelDimensions.Height - Main.screenPosition.Y, 0f);
        }

        /// <summary>
        /// Regenerates this UI state with the new passed in shrine entity.
        /// </summary>
        /// <param name="entity"> The new entity that this state will bind to. </param>
        public void RegenState(VillageShrineEntity entity) {
            CurrentEntity = entity;

            respawnItemDisplay.SetItem(NPCUtils.VillagerTypeToRespawnItemType(entity.shrineType));
            respawnItemCount.SetText(entity.remainingRespawnItems.ToString());
        }
    }
}