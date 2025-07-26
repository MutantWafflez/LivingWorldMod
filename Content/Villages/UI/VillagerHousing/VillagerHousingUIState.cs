using System.Runtime.InteropServices.Marshalling;
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
        public Villager myVillager;

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
    public UIModifiedText villagerTypeText;

    /// <summary>
    ///     The scroll bar for the grid of villagers.
    /// </summary>
    public UIBetterScrollbar gridScrollbar;

    /// <summary>
    ///     The grid of villagers of the specified type that are currently being displayed.
    /// </summary>
    public UIGrid gridOfVillagers;

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

        villagerTypeText = new UIModifiedText("VillagerType.Harpy".Localized(), 1.1f) { horizontalTextConstraint = villagerTypeCenterElement.Width.Pixels, HAlign = 0.5f, VAlign = 0.5f };
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

    public override void Update(GameTime gameTime) {
        bool isMiniMapEnabled = !Main.mapFullscreen && Main.mapStyle == 1;

        //Update positions
        openMenuButton.Left.Set(Main.screenWidth - (isMiniMapEnabled ? 220f : 177f), 0f);
        openMenuButton.Top.Set((isMiniMapEnabled ? 143f : 114f) + Main.mH, 0f);

        villagerHousingZone.Top.Set(180f + Main.mH, 0f);

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

        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.ModNPC is not Villager villager || villager.VillagerType != typeToShow) {
                continue;
            }

            UIHousingVillagerDisplay element = new(villager);

            gridOfVillagers.Add(element);
        }

        gridScrollbar.ViewPosition = 0f;
    }

    private static void WhileHoveringButton() {
        Main.instance.MouseText("UI.VillagerHousing.ButtonHoverText".Localized().Value);
    }
}