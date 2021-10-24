using System;
using Extensions;
using LivingWorldMod.Content.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
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
        /// Button that enumerates to the right (up) when clicked when swapping through villager types.
        /// </summary>
        public UIBetterImageButton enumerateRightButton;

        /// <summary>
        /// Button that enumerates to the left (down) when clicked when swapping through villager types.
        /// </summary>
        public UIBetterImageButton enumerateLeftButton;

        /// <summary>
        /// Element that exists to center the villager type display text.
        /// </summary>
        public UIElement villagerTypeCenterElement;

        /// <summary>
        /// Text that displays what type of villager is currently selected for housing.
        /// </summary>
        public UIBetterText villagerTypeText;

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
            openMenuButton.SetHoverImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Hovered", AssetRequestMode.ImmediateLoad));
            openMenuButton.SetVisibility(1f, 1f);
            openMenuButton.WhileHovering += WhileHoveringButton;
            openMenuButton.OnClick += MenuButtonClicked;
            Append(openMenuButton);

            enumerateRightButton = new UIBetterImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Button_Forward", AssetRequestMode.ImmediateLoad)) {
                isVisible = false
            };
            enumerateRightButton.SetVisibility(1f, 0.7f);
            enumerateRightButton.OnClick += EnumerateTypeButtonClicked;
            Append(enumerateRightButton);

            enumerateLeftButton = new UIBetterImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Button_Back", AssetRequestMode.ImmediateLoad)) {
                isVisible = false
            };
            enumerateLeftButton.SetVisibility(1f, 0.7f);
            enumerateLeftButton.OnClick += EnumerateTypeButtonClicked;
            Append(enumerateLeftButton);

            villagerTypeCenterElement = new UIElement();
            villagerTypeCenterElement.Width.Set(82f, 0f);
            villagerTypeCenterElement.Height.Set(28f, 0f);
            Append(villagerTypeCenterElement);

            villagerTypeText = new UIBetterText(typeToShow.ToString(), 1.1f) {
                isVisible = false,
                horizontalTextConstraint = villagerTypeCenterElement.Width.Pixels,
                HAlign = 0.5f,
                VAlign = 0.5f
            };
            villagerTypeCenterElement.Append(villagerTypeText);
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

            //Update positions
            openMenuButton.Left.Set(Main.screenWidth - (isMiniMapEnabled ? 220f : 177f), 0f);
            openMenuButton.Top.Set((isMiniMapEnabled ? 143f : 114f) + mapDisplacement, 0f);

            enumerateLeftButton.Left.Set(Main.screenWidth - 190f, 0f);
            enumerateLeftButton.Top.Set(180f + mapDisplacement, 0f);

            villagerTypeCenterElement.Left.Set(Main.screenWidth - 157f, 0f);
            villagerTypeCenterElement.Top.Set(180f + mapDisplacement, 0f);

            enumerateRightButton.Left.Set(Main.screenWidth - 70f, 0f);
            enumerateRightButton.Top.Set(180f + mapDisplacement, 0f);

            //Disable Menu Visibility when any other equip page buttons are pressed
            if (isMenuVisible && Main.EquipPageSelected != -1) {
                isMenuVisible = false;
                openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off"));
            }

            enumerateRightButton.isVisible = enumerateLeftButton.isVisible = villagerTypeText.isVisible = isMenuVisible;

            base.DrawChildren(spriteBatch);
        }

        private void EnumerateTypeButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
            //Up = true, Down = false
            bool enumerateDirection = listeningElement == enumerateRightButton;

            //Make sure to wrap around properly when necessary
            if (enumerateDirection) {
                VillagerType nextValue = typeToShow.NextEnum();
                typeToShow = nextValue == VillagerType.TypeCount ? VillagerType.Harpy : nextValue;
            }
            else {
                VillagerType previousValue = typeToShow.PreviousEnum();
                typeToShow = previousValue == VillagerType.TypeCount ? VillagerType.TypeCount - 1 : previousValue;
            }

            //Change to proper villager type text
            villagerTypeText.SetText(LocalizationUtils.GetLWMTextValue($"VillagerType.{typeToShow}"));
        }

        private void MenuButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
            //Opening/closing the housing menu
            isMenuVisible = !isMenuVisible;

            if (isMenuVisible) {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_On", AssetRequestMode.ImmediateLoad));
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