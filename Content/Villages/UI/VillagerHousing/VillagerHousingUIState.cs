using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.Villages.UI.VillagerHousing;

/// <summary>
///     UIState that handles the visuals of the Housing UI.
/// </summary>
public class VillagerHousingUIState : UIState {
    /// <summary>
    ///     Whether or not the menu that shows each of the villagers is visible (open).
    /// </summary>
    public bool isMenuVisible;

    /// <summary>
    ///     The Villager type to currently be showing to the player.
    /// </summary>
    public VillagerType typeToShow;

    /// <summary>
    ///     The button that closes/opens the menu showing each of the villagers.
    /// </summary>
    public UIBetterImageButton openMenuButton;

    /// <summary>
    ///     Button that enumerates to the right (up) when clicked when swapping through villager types.
    /// </summary>
    public UIBetterImageButton enumerateRightButton;

    /// <summary>
    ///     Button that enumerates to the left (down) when clicked when swapping through villager types.
    /// </summary>
    public UIBetterImageButton enumerateLeftButton;

    /// <summary>
    ///     Element that exists to center the villager type display text.
    /// </summary>
    public UIElement villagerTypeCenterElement;

    /// <summary>
    ///     Text that displays what type of villager is currently selected for housing.
    /// </summary>
    public UIBetterText villagerTypeText;

    /// <summary>
    ///     The scroll bar for the grid of villagers.
    /// </summary>
    public UIBetterScrollbar gridScrollbar;

    /// <summary>
    ///     The grid of villagers of the specified type that are currently being displayed.
    /// </summary>
    public UIGrid gridOfVillagers;

    /// <summary>
    ///     The displacement of the menu icon based on the map and its current mode/placement.
    /// </summary>
    private int _mapDisplacement;

    /// <summary>
    ///     Path to the sprites for this UI.
    /// </summary>
    private string HousingTexturePath => $"{LWM.SpritePath}Villages/UI/VillagerHousingUI/";

    public override void OnInitialize() {
        typeToShow = VillagerType.Harpy;

        openMenuButton = new UIBetterImageButton(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off", AssetRequestMode.ImmediateLoad));
        openMenuButton.SetHoverImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Hovered", AssetRequestMode.ImmediateLoad));
        openMenuButton.SetVisibility(1f, 1f);
        openMenuButton.WhileHovering += WhileHoveringButton;
        openMenuButton.ProperOnClick += MenuButtonClicked;
        Append(openMenuButton);

        enumerateRightButton = new UIBetterImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Button_Forward", AssetRequestMode.ImmediateLoad)) { isVisible = false };
        enumerateRightButton.Left.Set(Main.screenWidth - 70f, 0f);
        enumerateRightButton.SetVisibility(1f, 0.7f);
        enumerateRightButton.ProperOnClick += EnumerateTypeButtonClicked;
        Append(enumerateRightButton);

        enumerateLeftButton = new UIBetterImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Button_Back", AssetRequestMode.ImmediateLoad)) { isVisible = false };
        enumerateLeftButton.Left.Set(Main.screenWidth - 190f, 0f);
        enumerateLeftButton.SetVisibility(1f, 0.7f);
        enumerateLeftButton.ProperOnClick += EnumerateTypeButtonClicked;
        Append(enumerateLeftButton);

        villagerTypeCenterElement = new UIElement();
        villagerTypeCenterElement.Width.Set(82f, 0f);
        villagerTypeCenterElement.Height.Set(28f, 0f);
        Append(villagerTypeCenterElement);

        villagerTypeText = new UIBetterText(Language.GetOrRegister("Mods.LivingWorldMod.VillagerType.Harpy"), 1.1f) {
            isVisible = false, horizontalTextConstraint = villagerTypeCenterElement.Width.Pixels, HAlign = 0.5f, VAlign = 0.5f
        };
        villagerTypeCenterElement.Left.Set(Main.screenWidth - 157f, 0f);
        villagerTypeCenterElement.Append(villagerTypeText);

        gridScrollbar = new UIBetterScrollbar { isVisible = false };
        gridScrollbar.Left.Set(Main.screenWidth - 26f, 0f);
        gridScrollbar.Height.Set(390f, 0f);
        Append(gridScrollbar);

        gridOfVillagers = new UIGrid { ListPadding = 4f };
        gridOfVillagers.Left.Set(Main.screenWidth - 196f, 0f);
        gridOfVillagers.Width.Set(160f, 0f);
        gridOfVillagers.Height.Set(gridScrollbar.Height.Pixels, 0f);
        gridOfVillagers.SetScrollbar(gridScrollbar);
        Append(gridOfVillagers);
    }

    protected override void DrawChildren(SpriteBatch spriteBatch) {
        bool isMiniMapEnabled = !Main.mapFullscreen && Main.mapStyle == 1;

        //Adapted vanilla code since "Main.mH" is private, and I do not want to use reflection every frame
        _mapDisplacement = 0;

        if (Main.mapEnabled) {
            if (isMiniMapEnabled) {
                _mapDisplacement = 256;
            }

            if (_mapDisplacement + Main.instance.RecommendedEquipmentAreaPushUp > Main.screenHeight) {
                _mapDisplacement = Main.screenHeight - Main.instance.RecommendedEquipmentAreaPushUp;
            }
        }

        //Update positions
        openMenuButton.Left.Set(Main.screenWidth - (isMiniMapEnabled ? 220f : 177f), 0f);
        openMenuButton.Top.Set((isMiniMapEnabled ? 143f : 114f) + _mapDisplacement, 0f);

        enumerateLeftButton.Left.Set(Main.screenWidth - 190f, 0f);
        enumerateLeftButton.Top.Set(180f + _mapDisplacement, 0f);

        villagerTypeCenterElement.Left.Set(Main.screenWidth - 157f, 0f);
        villagerTypeCenterElement.Top.Set(180f + _mapDisplacement, 0f);

        enumerateRightButton.Left.Set(Main.screenWidth - 70f, 0f);
        enumerateRightButton.Top.Set(180f + _mapDisplacement, 0f);

        gridScrollbar.Left.Set(Main.screenWidth - 26f, 0f);
        gridScrollbar.Top.Set(214f + _mapDisplacement, 0f);
        gridScrollbar.Height.Set(390f - _mapDisplacement, 0f);

        gridOfVillagers.Left.Set(Main.screenWidth - 196f, 0f);
        gridOfVillagers.Top.Set(214f + _mapDisplacement, 0f);
        gridOfVillagers.Height.Set(gridScrollbar.Height.Pixels, 0f);

        //Disable Menu Visibility when any other equip page buttons are pressed
        if (isMenuVisible && Main.EquipPageSelected != -1) {
            CloseMenu();
        }

        enumerateRightButton.isVisible = enumerateLeftButton.isVisible = villagerTypeText.isVisible = gridScrollbar.isVisible = isMenuVisible;

        base.DrawChildren(spriteBatch);
    }

    /// <summary>
    ///     Simple method that closes the menu. Public for the parent system to use.
    /// </summary>
    public void CloseMenu() {
        isMenuVisible = false;
        openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off"));
        gridOfVillagers.Clear();
    }

    private void EnumerateTypeButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
        //Up = true, Down = false
        bool enumerateDirection = listeningElement == enumerateRightButton;

        //Make sure to wrap around properly when necessary
        if (enumerateDirection) {
            VillagerType nextValue = typeToShow.NextEnum();
            typeToShow = nextValue;
        }
        else {
            VillagerType previousValue = typeToShow.PreviousEnum();
            typeToShow = previousValue;
        }

        //Change to proper villager type text
        villagerTypeText.SetText(Language.GetOrRegister($"Mods.LivingWorldMod.VillagerType.{typeToShow}"));

        DisplayAvailableVillagers();
    }

    private void MenuButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
        //Opening/closing the housing menu
        isMenuVisible = !isMenuVisible;

        if (isMenuVisible) {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_On", AssetRequestMode.ImmediateLoad));
            Main.EquipPageSelected = -1;
            Main.EquipPage = -1;
            DisplayAvailableVillagers();
        }
        else {
            SoundEngine.PlaySound(SoundID.MenuClose);
            openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off"));
            Main.EquipPageSelected = 0;
            Main.EquipPage = 0;
            gridOfVillagers.Clear();
        }
    }

    /// <summary>
    ///     Finds and displays the current villagers in the world of the current type.
    /// </summary>
    private void DisplayAvailableVillagers() {
        //Clear list for re-displaying
        gridOfVillagers.Clear();

        for (int i = 0; i < Main.maxNPCs; i++) {
            if (Main.npc[i].active && Main.npc[i].ModNPC is Villager villager && villager.VillagerType == typeToShow) {
                UIHousingVillagerDisplay element = new(villager);

                element.Activate();

                gridOfVillagers.Add(element);
            }
        }

        gridScrollbar.Activate();
    }

    private void WhileHoveringButton() {
        Main.instance.MouseText("UI.VillagerHousing.ButtonHoverText".Localized().Value);
    }
}