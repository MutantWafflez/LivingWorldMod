using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.Villages.UI.VillagerHousing;

/// <summary>
///     UIState that handles the visuals of the Housing UI.
/// </summary>
public class VillagerHousingUIState : UIState {
    /// <summary>
    ///     Element that shows a specific villager in the housing menu. An instance of this is created
    ///     per villager that exists in the world of a given type when the player is in the housing menu.
    /// </summary>
    private class UIHousingVillagerDisplay : UIElement {
        // Very arbitrary number, I know, but this is about the smallest we can get the
        // width/height while keeping 3 objects in a row in the grid
        private const float SideLength = 50.284f;

        private const int LockIconSideLength = 22;

        /// <summary>
        ///     The villager instance that this element is displaying.
        /// </summary>
        public readonly Villager myVillager;

        /// <summary>
        ///     Whether or not this villager is currently selected.
        /// </summary>
        public bool IsSelected => myVillager.NPC.whoAmI == Main.instance.mouseNPCIndex;

        /// <summary>
        ///     Whether or not this NPC is "allowed" to be housed, which is to say whether or not the
        ///     village that the villager belongs to likes the player.
        /// </summary>
        //TODO: Swap back to commented expression when Reputation system is re-implemented
        public bool IsAllowed => true; //myVillager.RelationshipStatus >= VillagerRelationship.Like;

        public UIHousingVillagerDisplay(Villager villager) {
            myVillager = villager;

            Width = StyleDimension.FromPixels(SideLength);
            Height = StyleDimension.FromPixels(SideLength);
        }

        public override void LeftClick(UIMouseEvent evt) {
            //Prevent any interaction if the villagers do not like the player
            if (!IsAllowed) {
                return;
            }

            //Change the mouse type properly
            if (IsSelected) {
                Main.instance.SetMouseNPC(-1, -1);
            }
            else {
                //Our IL edit in NPCHousingPatches.cs handles the drawing here.
                Main.instance.SetMouseNPC(myVillager.NPC.whoAmI, myVillager.Type);
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }

        public override void Update(GameTime gameTime) {
            if (!ContainsPoint(Main.MouseScreen) || myVillager is null) {
                return;
            }

            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText(IsAllowed ? myVillager.NPC.GivenName : "UI.VillagerHousing.VillagerTypeLocked".Localized().FormatWith(myVillager.VillagerType.ToString()));
        }

        protected override void DrawChildren(SpriteBatch spriteBatch) {
            base.DrawChildren(spriteBatch);

            spriteBatch.Draw(
                TextureAssets.InventoryBack11.Value,
                GetDimensions().ToRectangle(),
                null,
                !IsAllowed ? Color.Gray : Color.White,
                0f,
                default(Vector2),
                SpriteEffects.None,
                0f
            );


            LayeredDrawObject drawObject = myVillager.drawObject;
            Rectangle textureDrawRegion = new(0, 0, drawObject.GetLayerFrameWidth(), drawObject.GetLayerFrameHeight(0, 0, Main.npcFrameCount[myVillager.Type]));

            drawObject.Draw(
                spriteBatch,
                new DrawData(
                    null,
                    GetDimensions().Center(),
                    textureDrawRegion,
                    IsSelected ? Color.Yellow : Color.White,
                    0f,
                    new Vector2(textureDrawRegion.Width / 2f, textureDrawRegion.Height / 2f * 1.15f),
                    0.75f,
                    SpriteEffects.None
                ),
                myVillager.DrawIndices
            );

            if (IsAllowed) {
                return;
            }

            //Draw lock icon if not "allowed" (player is not high enough rep)
            spriteBatch.Draw(
                TextureAssets.HbLock[0].Value,
                GetDimensions().Center(),
                new Rectangle(0, 0, LockIconSideLength, LockIconSideLength),
                Color.White,
                0f,
                new Vector2(LockIconSideLength / 2f, LockIconSideLength / 2f),
                1.25f,
                SpriteEffects.None,
                0f
            );
        }
    }

    private const float VanillaArrowButtonsSideLength = 28f;
    private const float OpenMenuButtonSideLength = 26f;

    private const float VillagerHousingZoneXOffset = -196f;
    private const float DefaultVillagerHousingZoneYPos = 180f;
    private const float VillagerHousingZoneWidth = 160f;
    private const float VillagerHousingZoneHeight = 418f;

    private const float EnumerateRightButtonXPos = 126f;
    private const float EnumerateLeftButtonXPos = 6f;

    private const float VillagerTypeZoneWidth = 82f;
    private const float VillagerTypeZoneHeight = 28f;
    private const float VillagerTypeTextXPos = 39f;

    private const float VillagerGridYPos = 34f;
    private const float GridScrollbarXPos = 170f;
    private const float VillagerGridHeight = 390f;

    private const float OpenMenuButtonXPosWithMinimap = -220f;
    private const float OpenMenuButtonXPosWithoutMinimap = -177f;
    private const float OpenMenuButtonYPosWithMinimap = 143f;
    private const float OpenMenuButtonYPosWithoutMinimap = 114f;

    /// <summary>
    ///     The Villager type to currently be showing to the player.
    /// </summary>
    public VillagerType typeToShow;

    /// <summary>
    ///     Backing element that holds the open button.
    /// </summary>
    public UIVisibilityElement openMenuButtonZone;

    /// <summary>
    ///     The button that closes/opens the menu showing each of the villagers.
    /// </summary>
    public UIBetterImageButton openMenuButton;

    /// <summary>
    ///     Backing element that holds all elements, except for the open button.
    /// </summary>
    public UIVisibilityElement villagerHousingZone;

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
    public UIElement villagerTypeZone;

    /// <summary>
    ///     Text that displays what type of villager is currently selected for housing.
    /// </summary>
    public UIModifiedText villagerTypeText;

    /// <summary>
    ///     The grid of villagers of the specified type that are currently being displayed.
    /// </summary>
    public UIGrid gridOfVillagers;

    /// <summary>
    ///     The scroll bar for the grid of villagers.
    /// </summary>
    public UIBetterScrollbar gridScrollbar;

    /// <summary>
    ///     Path to the sprites for this UI.
    /// </summary>
    private string HousingTexturePath => $"{LWM.SpritePath}Villages/UI/VillagerHousingUI/";

    private static void WhileHoveringButton() {
        Main.instance.MouseText("UI.VillagerHousing.ButtonHoverText".Localized().Value);
    }

    public override void OnInitialize() {
        typeToShow = VillagerType.Harpy;

        openMenuButtonZone = new UIVisibilityElement { Width = StyleDimension.FromPixels(OpenMenuButtonSideLength), Height = StyleDimension.FromPixels(OpenMenuButtonSideLength) };
        Append(openMenuButtonZone);

        openMenuButton = new UIBetterImageButton(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off")) { Width = StyleDimension.Fill, Height = StyleDimension.Fill };
        openMenuButton.SetHoverImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Hovered"));
        openMenuButton.SetVisibility(1f, 1f);
        openMenuButton.WhileHovering += WhileHoveringButton;
        openMenuButton.OnLeftClick += MenuButtonClicked;
        openMenuButtonZone.Append(openMenuButton);

        villagerHousingZone = new UIVisibilityElement {
            Left = StyleDimension.FromPixelsAndPercent(VillagerHousingZoneXOffset, 1f),
            Top = StyleDimension.FromPixels(DefaultVillagerHousingZoneYPos),
            Width = StyleDimension.FromPixels(VillagerHousingZoneWidth),
            Height = StyleDimension.FromPixels(VillagerHousingZoneHeight)
        };
        Append(villagerHousingZone);

        enumerateRightButton = new UIBetterImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Button_Forward")) {
            Width = StyleDimension.FromPixels(VanillaArrowButtonsSideLength),
            Height = StyleDimension.FromPixels(VanillaArrowButtonsSideLength),
            Left = StyleDimension.FromPixels(EnumerateRightButtonXPos)
        };
        enumerateRightButton.SetVisibility(1f, 0.7f);
        enumerateRightButton.OnLeftClick += EnumerateTypeButtonClicked;
        villagerHousingZone.Append(enumerateRightButton);

        enumerateLeftButton = new UIBetterImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Button_Back")) {
            Width = StyleDimension.FromPixels(VanillaArrowButtonsSideLength),
            Height = StyleDimension.FromPixels(VanillaArrowButtonsSideLength),
            Left = StyleDimension.FromPixels(EnumerateLeftButtonXPos)
        };
        enumerateLeftButton.SetVisibility(1f, 0.7f);
        enumerateLeftButton.OnLeftClick += EnumerateTypeButtonClicked;
        villagerHousingZone.Append(enumerateLeftButton);

        villagerTypeZone = new UIElement { Width = StyleDimension.FromPixels(VillagerTypeZoneWidth), Height = StyleDimension.FromPixels(VillagerTypeZoneHeight) };
        villagerHousingZone.Append(villagerTypeZone);

        villagerTypeText = new UIModifiedText("VillagerType.Harpy".Localized(), 1.1f) {
            Left = StyleDimension.FromPixels(VillagerTypeTextXPos), horizontalTextConstraint = VillagerTypeZoneWidth, HAlign = 0.5f, VAlign = 0.5f
        };
        villagerTypeZone.Append(villagerTypeText);

        gridOfVillagers = new UIGrid {
            Top = StyleDimension.FromPixels(VillagerGridYPos), Width = StyleDimension.FromPercent(1f), Height = StyleDimension.FromPixels(VillagerGridHeight), ListPadding = 4f
        };
        villagerHousingZone.Append(gridOfVillagers);

        gridScrollbar = new UIBetterScrollbar {
            Left = StyleDimension.FromPixels(GridScrollbarXPos), Top = StyleDimension.FromPixels(VillagerGridYPos), Height = StyleDimension.FromPixels(VillagerGridHeight)
        };
        gridOfVillagers.SetScrollbar(gridScrollbar);
        villagerHousingZone.Append(gridScrollbar);
    }

    public override void Update(GameTime gameTime) {
        bool isMiniMapEnabled = !Main.mapFullscreen && Main.mapStyle == 1;

        //Update positions
        openMenuButtonZone.Left.Set(isMiniMapEnabled ? OpenMenuButtonXPosWithMinimap : OpenMenuButtonXPosWithoutMinimap, 1f);
        openMenuButtonZone.Top.Set((isMiniMapEnabled ? OpenMenuButtonYPosWithMinimap : OpenMenuButtonYPosWithoutMinimap) + Main.mH, 0f);

        villagerHousingZone.Top.Set(DefaultVillagerHousingZoneYPos + Main.mH, 0f);

        //Disable Menu Visibility when any other equip page buttons are pressed
        if (villagerHousingZone.IsVisible && Main.EquipPageSelected != -1) {
            CloseMenu();
        }

        base.Update(gameTime);
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
            Main.EquipPageSelected = -1;
            Main.EquipPage = -1;

            DisplayAvailableVillagers();
            openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_On", AssetRequestMode.ImmediateLoad));

            SoundEngine.PlaySound(SoundID.MenuOpen);
        }
        else {
            Main.EquipPageSelected = 0;
            Main.EquipPage = 0;

            openMenuButton.SetImage(ModContent.Request<Texture2D>(HousingTexturePath + "VillagerHousing_Off"));
            gridOfVillagers.Clear();

            SoundEngine.PlaySound(SoundID.MenuClose);
        }
    }

    /// <summary>
    ///     Finds and displays the current villagers in the world of the current type.
    /// </summary>
    private void DisplayAvailableVillagers() {
        //Clear list for re-displaying
        gridOfVillagers.Clear();

        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.ModNPC is not Villager villager || villager.VillagerType != typeToShow) {
                continue;
            }

            UIHousingVillagerDisplay element = new(villager);

            gridOfVillagers.Add(element);
        }

        gridScrollbar.ViewPosition = 0f;
    }
}