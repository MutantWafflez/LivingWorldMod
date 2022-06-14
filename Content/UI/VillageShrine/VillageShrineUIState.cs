using LivingWorldMod.Custom.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.VillageShrine {
    /// <summary>
    /// UIState that handles the UI for the village shrine for each type of village.
    /// </summary>
    public class VillageShrineUIState : UIState {

        public VillagerType currentShrineType;

        public Vector2 topLeftShrineTile;

        public UIPanel backPanel;

        private float _oldUIScale;

        private const float DefaultSquareSize = 180f;

        public override void OnInitialize() {
            currentShrineType = VillagerType.Harpy;

            _oldUIScale = Main.UIScale;

            backPanel = new UIPanel();
            backPanel.Width = backPanel.Height = new StyleDimension(DefaultSquareSize, 0f);
            Append(backPanel);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            if (Main.UIScale != _oldUIScale) {
                backPanel.Width = backPanel.Height = new StyleDimension(DefaultSquareSize * Main.UIScale, 0f);

                _oldUIScale = Main.UIScale;
            }

            backPanel.Left.Set(topLeftShrineTile.X - 16f - Main.screenPosition.X, 0f);
            backPanel.Top.Set(topLeftShrineTile.Y - 16f - backPanel.GetDimensions().Height - Main.screenPosition.Y, 0f);
        }

        /// <summary>
        /// Regenerates this UI state with the new passed in shrine type and shrine position.
        /// </summary>
        /// <param name="newShrineType"> The new shrine type to swap this state to. </param>
        /// <param name="topLeftShrinePos"> The WORLD position of where to move this state to. </param>
        public void RegenState(VillagerType newShrineType, Vector2 topLeftShrinePos) {
            currentShrineType = newShrineType;
            topLeftShrineTile = topLeftShrinePos;
        }
    }
}