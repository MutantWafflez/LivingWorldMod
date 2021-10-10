using LivingWorldMod.Content.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
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
        /// The displacement of the menu icon based on the map and its current mode/placement.
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
            openMenuButton.SetHoverImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off_Hovered", AssetRequestMode.ImmediateLoad));
            openMenuButton.SetVisibility(1f, 1f);
            openMenuButton.WhileHovering += WhileHoveringButton;
            openMenuButton.OnClick += ClickedButton;

            Append(openMenuButton);
        }

        protected override void DrawChildren(SpriteBatch spriteBatch) {
            bool isMiniMapEnabled = !Main.mapFullscreen && Main.mapStyle == 1;

            //Adapted vanilla code since "Main.mH" is private, and I do not want to use reflection every frame
            mapDisplacement = 0;

            if (Main.mapEnabled) {
                if (isMiniMapEnabled) {
                    mapDisplacement = 256;
                }

                if (mapDisplacement + Main.instance.RecommendedEquipmentAreaPushUp > Main.screenHeight) {
                    mapDisplacement = Main.screenHeight - Main.instance.RecommendedEquipmentAreaPushUp;
                }
            }

            openMenuButton.isVisible = Main.playerInventory;

            openMenuButton.Left.Set(Main.screenWidth - (isMiniMapEnabled ? 220f : 177f), 0f);
            openMenuButton.Top.Set((isMiniMapEnabled ? 143f : 114f) + mapDisplacement, 0f);

            //Disable Menu Visibility when any other equip page buttons are pressed
            if (isMenuVisible && Main.EquipPageSelected != -1) {
                isMenuVisible = false;
                openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off"));
            }

            base.DrawChildren(spriteBatch);
        }

        private void ClickedButton(UIMouseEvent evt, UIElement listeningElement) {
            isMenuVisible = !isMenuVisible;

            if (isMenuVisible) {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Selected", AssetRequestMode.ImmediateLoad));
                Main.EquipPageSelected = -1;
                Main.EquipPage = -1;
            }
            else {
                SoundEngine.PlaySound(SoundID.MenuClose);
                openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off"));
                Main.EquipPageSelected = 0;
                Main.EquipPage = 0;
            }
        }

        private void WhileHoveringButton() {
            Main.instance.MouseText(LocalizationUtils.GetLWMTextValue("UI.VillagerHousing.ButtonHoverText"));
            Main.LocalPlayer.mouseInterface = true;
        }
    }
}