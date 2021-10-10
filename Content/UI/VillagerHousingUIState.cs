using LivingWorldMod.Content.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI {

    /// <summary>
    /// UIState that handles the visuals of the Housing UI.
    /// </summary>
    public class VillagerHousingUIState : UIState {

        /// <summary>
        /// Whether or not the menu that shows each of the villagers is visible (open).
        /// </summary>
        public bool isMenuVisible;

        /// <summary>
        /// The Villager type to currently be showing to the player.
        /// </summary>
        public VillagerType typeToShow;

        /// <summary>
        /// The button that closes/opens the menu showing each of the villagers.
        /// </summary>
        public UIBetterImageButton openMenuButton;

        /// <summary>
        /// The displacement of the menu icon based on the map and it's current mode/placement.
        /// </summary>
        private int mapDisplacement;

        /// <summary>
        /// Path to the sprites for this UI.
        /// </summary>
        private string HousingTexturePath => LivingWorldMod.LWMSpritePath + "/UI/VillagerHousingUI/";

        public override void OnInitialize() {
            typeToShow = VillagerType.Harpy;

            openMenuButton = new UIBetterImageButton(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off", AssetRequestMode.ImmediateLoad)) {
                isVisible = false
            };
            openMenuButton.SetHoverImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_On", AssetRequestMode.ImmediateLoad));
            openMenuButton.SetVisibility(1f, 1f);
            openMenuButton.WhileHovering += WhileHovering;

            Append(openMenuButton);
        }

        protected override void DrawChildren(SpriteBatch spriteBatch) {
            bool isMiniMapEnabled = !Main.mapFullscreen && Main.mapStyle == 1;

            //Adapted vanilla code since "Main.mH" is private, and I do not want to use reflection every frame
            mapDisplacement = 0;

            if (Main.mapEnabled) {
                if (!Main.mapFullscreen && Main.mapStyle == 1) {
                    mapDisplacement = 256;
                }

                if (mapDisplacement + Main.instance.RecommendedEquipmentAreaPushUp > Main.screenHeight) {
                    mapDisplacement = Main.screenHeight - Main.instance.RecommendedEquipmentAreaPushUp;
                }
            }

            openMenuButton.isVisible = Main.playerInventory;

            openMenuButton.Left.Set(Main.screenWidth - (isMiniMapEnabled ? 220f : 175f), 0f);
            openMenuButton.Top.Set((isMiniMapEnabled ? 145f : 116f) + mapDisplacement, 0f);

            base.DrawChildren(spriteBatch);
        }

        private void WhileHovering() {
            Main.instance.MouseText(LocalizationUtils.GetLWMTextValue("UI.VillagerHousing.ButtonHoverText"));
        }
    }
}