using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.Villages.UI.VillagerHousing;

/// <summary>
///     UIState that handles the visuals of the Housing UI.
/// </summary>
public class VillagerHousingUIState : UIState {
    /// <summary>
    ///     The Villager type to currently be showing to the player.
    /// </summary>
    public VillagerType typeToShow;

    public UIVisibilityElement villagerHousingZone;

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
        openMenuButton.OnLeftClick += MenuButtonClicked;
        Append(openMenuButton);

        villagerHousingZone = new UIVisibilityElement {
            Left = StyleDimension.FromPixelsAndPercent(-196f, 1f), Top = StyleDimension.FromPixels(180f), Width = StyleDimension.FromPixels(160f), Height = StyleDimension.FromPixels(418f)
        };
        Append(villagerHousingZone);

        enumerateRightButton = new UIBetterImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Button_Forward", AssetRequestMode.ImmediateLoad));
        enumerateRightButton.Left.Set(126f, 0f);
        enumerateRightButton.SetVisibility(1f, 0.7f);
        enumerateRightButton.OnLeftClick += EnumerateTypeButtonClicked;
        villagerHousingZone.Append(enumerateRightButton);

        enumerateLeftButton = new UIBetterImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Button_Back", AssetRequestMode.ImmediateLoad));
        enumerateLeftButton.Left.Set(6f, 0f);
        enumerateLeftButton.SetVisibility(1f, 0.7f);
        enumerateLeftButton.OnLeftClick += EnumerateTypeButtonClicked;
        villagerHousingZone.Append(enumerateLeftButton);

        villagerTypeCenterElement = new UIElement();
        villagerTypeCenterElement.Width.Set(82f, 0f);
        villagerTypeCenterElement.Height.Set(28f, 0f);
        villagerHousingZone.Append(villagerTypeCenterElement);

        villagerTypeText = new UIBetterText("VillagerType.Harpy".Localized(), 1.1f) { horizontalTextConstraint = villagerTypeCenterElement.Width.Pixels, HAlign = 0.5f, VAlign = 0.5f };
        villagerTypeCenterElement.Left.Set(39f, 0f);
        villagerTypeCenterElement.Append(villagerTypeText);

        gridScrollbar = new UIBetterScrollbar();
        gridScrollbar.Left.Set(170f, 0f);
        gridScrollbar.Top.Set(34f, 0f);
        gridScrollbar.Height.Set(390f, 0f);
        villagerHousingZone.Append(gridScrollbar);

        gridOfVillagers = new UIGrid { ListPadding = 4f };
        gridOfVillagers.Top.Set(34f, 0f);
        gridOfVillagers.Width.Set(0f, 1f);
        gridOfVillagers.Height.Set(gridScrollbar.Height.Pixels, 0f);
        gridOfVillagers.SetScrollbar(gridScrollbar);
        villagerHousingZone.Append(gridOfVillagers);
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

        villagerHousingZone.Top.Set(180f + _mapDisplacement, 0f);

        //Disable Menu Visibility when any other equip page buttons are pressed
        if (villagerHousingZone.IsVisible && Main.EquipPageSelected != -1) {
            CloseMenu();
        }

        base.DrawChildren(spriteBatch);
    }

    /// <summary>
    ///     Simple method that closes the menu. Public for the parent system to use.
    /// </summary>
    public void CloseMenu() {
        villagerHousingZone.SetVisibility(false);
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
        villagerTypeText.SetText($"VillagerType.{typeToShow}".Localized());

        DisplayAvailableVillagers();
    }

    private void MenuButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
        //Opening/closing the housing menu
        villagerHousingZone.SetVisibility(!villagerHousingZone.IsVisible);

        if (villagerHousingZone.IsVisible) {
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