using System;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.DataStructures.Records;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.Villages.Globals.Systems;
using LivingWorldMod.Content.Villages.Globals.Systems.UI;
using LivingWorldMod.Content.Villages.HarpyVillage.NPCs;
using LivingWorldMod.Globals.UIElements;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LivingWorldMod.Content.Villages.UI.VillagerShop;

/// <summary>
///     UIState that handles the entire UI portion of the shop system for all villager types.
/// </summary>

// Future Mutant: Cleaned up. For now?
public class ShopUIState : UIState {
    /// <summary>
    ///     UIImage class that holds different UITexts and UIElements that is the index in a given shop
    ///     UI list. Holds data on the entire entry for the given item.
    /// </summary>
    public class UIShopItem : UIImage {
        private const float ShopItemWidth = 448f;
        private const float ShopItemHeight = 106f;
        private const float ItemImageXPos = 38f;

        public readonly Item displayedItem;
        public readonly UIModifiedText itemNameText;

        public readonly long displayedCost;
        public readonly int shopIndex;

        public bool isSelected;

        private readonly UIBetterItemIcon _itemImage;
        private readonly UICoinDisplay _itemCostDisplay;

        private readonly Villager _villager;

        private float _flashShaderTime;

        public VillagerShopItem ShopItem => _villager.shopInventory[shopIndex];

        public UIShopItem(Villager villager, int shopIndex, long displayedCost) : base(
            ModContent.Request<Texture2D>($"{LWM.SpritePath}Villages/UI/ShopUI/{villager.VillagerType}/ShopItemBox")
        ) {
            _villager = villager;
            this.shopIndex = shopIndex;

            VillagerShopItem item = villager.shopInventory[shopIndex];
            displayedItem = new Item();
            displayedItem.SetDefaults(item.ItemType);
            displayedItem.stack = item.Stock;
            this.displayedCost = displayedCost;

            Width = StyleDimension.FromPixels(ShopItemWidth);
            Height = StyleDimension.FromPixels(ShopItemHeight);

            _itemImage = new UIBetterItemIcon(displayedItem, ItemImageSideLength, true) {
                Left = StyleDimension.FromPixels(ItemImageXPos),
                Width = StyleDimension.FromPixels(ItemImageSideLength),
                Height = StyleDimension.FromPixels(ItemImageSideLength),
                VAlign = 0.5f,
                IgnoresMouseInteraction = true
            };
            Append(_itemImage);

            itemNameText = new UIModifiedText(displayedItem.HoverName, 1.25f) { Left = StyleDimension.FromPixels(94f), VAlign = 0.5f, horizontalTextConstraint = 194f, IgnoresMouseInteraction = true };
            Append(itemNameText);

            _itemCostDisplay = new UICoinDisplay(displayedCost, UICoinDisplay.CoinDrawStyle.NoCoinsWithZeroValue, 1.34f) {
                Left = StyleDimension.FromPixels(-DefaultElementPadding), HAlign = 1f, VAlign = 0.5f, IgnoresMouseInteraction = true
            };
            Append(_itemCostDisplay);

            OnMouseOver += MousedOverElement;
            OnMouseOut += MouseExitedElement;
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (!isSelected && !IsMouseHovering) {
                _flashShaderTime = 0f;
                return;
            }

            _flashShaderTime += MathHelper.Pi / LWMUtils.RealLifeSecond;
            if (_flashShaderTime >= MathHelper.TwoPi) {
                _flashShaderTime = 0f;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            RasterizerState defaultRasterizerState = new() { CullMode = CullMode.None, ScissorTestEnable = true };

            if (ShopItem.Stock <= 0) {
                Effect shader = ShopUISystem.grayScaleShader.Value;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, defaultRasterizerState, shader, Main.UIScaleMatrix);

                base.DrawSelf(spriteBatch);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, defaultRasterizerState, null, Main.UIScaleMatrix);
                return;
            }

            if (_flashShaderTime > 0f) {
                Effect shader = ShopUISystem.hoverFlashShader.Value;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, defaultRasterizerState, shader, Main.UIScaleMatrix);

                shader.Parameters["uTime"].SetValue(_flashShaderTime);
                base.DrawSelf(spriteBatch);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, defaultRasterizerState, null, Main.UIScaleMatrix);
            }
            else {
                base.DrawSelf(spriteBatch);
            }
        }

        private void MousedOverElement(UIMouseEvent evt, UIElement listeningElement) {
            if (ShopItem.Stock <= 0) {
                return;
            }

            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        private void MouseExitedElement(UIMouseEvent evt, UIElement listeningElement) {
            if (!isSelected) {
                _flashShaderTime = 0f;
            }
        }
    }

    private const float MaxBuyDelay = 60f;

    private const float DefaultElementPadding = 12f;

    private const float ShopZoneSideLength = 504f;

    private const float PortraitZoneSideLength = 196f;
    private const float PortraitZoneXPos = 732f;
    private const float PortraitZoneYPos = 120f;

    private const float ShopZoneXPos = 56f;
    private const float ShopZoneYPos = 50f;

    private const float NameZoneHeight = 60f;
    private const float NameZoneXPos = ShopZoneXPos + 678f;
    private const float NameZoneYPos = ShopZoneYPos + 272f;

    private const float DialogueZoneWidth = 410f;
    private const float DialogueZoneHeight = 166f;
    private const float DialogueZoneXPos = ShopZoneXPos + 528f;
    private const float DialogueZoneYPos = NameZoneYPos + 66f;

    private const float DialogueTextWrapConstraint = DialogueZoneWidth - DefaultElementPadding * 2f;

    private const float BuyItemZoneWidth = 158f;
    private const float BuyItemZoneHeight = 136f;
    private const float BuyItemZoneXPos = ShopZoneXPos + 506f;
    private const float BuyItemZoneYPos = ShopZoneYPos + 40f;

    private const float ItemImageSideLength = 32f;

    private const float BuyItemIconYPos = 26f;

    private const float BuyItemStockHeaderYPos = BuyItemIconYPos + 30f;

    private const float BuyItemStockYPos = BuyItemStockHeaderYPos + 24f;

    private const float BuyItemButtonWidth = 70f;
    private const float BuyItemButtonHeight = 30f;
    private const float BuyItemButtonYPos = BuyItemStockYPos + 22f;

    private const float SavingsZoneWidth = 126f;
    private const float SavingsZoneHeight = 84f;
    private const float SavingsZoneXPos = BuyItemZoneXPos + 14f;
    private const float SavingsZoneYPos = BuyItemZoneYPos + 170f;

    private const float SavingsTextYPos = -26f;

    private const float ShopScrollbarXOffset = -38f;
    private const float ShopScrollbarYPos = 16f;
    private const float ShopScrollbarHeight = ShopZoneSideLength - 40f;

    private const float ShopListWidth = ShopZoneSideLength - 34f;
    private const float ShopListHeight = ShopZoneSideLength + 8f;

    public Villager currentVillager;

    public UIImage backImage;
    public UIImage shopOverlay;

    public UIElement shopZone;

    public UIElement portraitZone;
    public UIPortrait portrait;

    public UIElement nameZone;
    public UIModifiedText nameText;

    public UIElement dialogueZone;
    public UIModifiedText dialogueText;

    public UIVisibilityElement buyItemZone;
    public UIModifiedText buyItemHeader;
    public UIBetterItemIcon buyItemIcon;
    public UIModifiedText buyItemStockHeader;
    public UIModifiedText buyItemStock;
    public UIPanelButton buyItemButton;

    public UIElement savingsZone;
    public UIModifiedText savingsText;
    public UICoinDisplay savingsDisplay;

    public UIScrollbar shopScrollbar;
    public UIList shopList;
    private float _buySpeed;
    private float _buyDelay;

    private UIShopItem _selectedItem;

    public override void OnInitialize() {
        Asset<Texture2D> vanillaPanelBackground = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        Asset<Texture2D> gradientPanelBorder = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/Elements/GradientPanelBorder");

        string shopUIPath = $"{LWM.SpritePath}Villages/UI/ShopUI/Harpy/";

        //Background Zone
        backImage = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "BackImage")) { HAlign = 0.5f, VAlign = 0.5f };
        Append(backImage);

        shopOverlay = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "Overlay")) { HAlign = 0.5f, VAlign = 0.5f, IgnoresMouseInteraction = true };
        Append(shopOverlay);

        //Shop Zone
        shopZone = new UIElement {
            Width = StyleDimension.FromPixels(ShopZoneSideLength),
            Height = StyleDimension.FromPixels(ShopZoneSideLength),
            Left = StyleDimension.FromPixels(ShopZoneXPos),
            Top = StyleDimension.FromPixels(ShopZoneYPos)
        };
        backImage.Append(shopZone);

        //Portrait Zone
        portraitZone = new UIElement {
            Width = StyleDimension.FromPixels(PortraitZoneSideLength),
            Height = StyleDimension.FromPixels(PortraitZoneSideLength),
            Left = StyleDimension.FromPixels(PortraitZoneXPos),
            Top = StyleDimension.FromPixels(PortraitZoneYPos)
        };
        backImage.Append(portraitZone);

        portrait = new UIPortrait(new HarpyVillager()) { Top = StyleDimension.FromPixels(4f) };
        portraitZone.Append(portrait);

        //Name Zone
        nameZone = new UIElement {
            Width = StyleDimension.FromPixels(PortraitZoneSideLength),
            Height = StyleDimension.FromPixels(NameZoneHeight),
            Left = StyleDimension.FromPixels(NameZoneXPos),
            Top = StyleDimension.FromPixels(NameZoneYPos)
        };
        backImage.Append(nameZone);

        nameText = new UIModifiedText(large: true) { HAlign = 0.5f, VAlign = 0.5f, horizontalTextConstraint = PortraitZoneSideLength };
        nameZone.Append(nameText);

        //Dialogue Zone
        dialogueZone = new UIElement {
            Width = StyleDimension.FromPixels(DialogueZoneWidth),
            Height = StyleDimension.FromPixels(DialogueZoneHeight),
            Left = StyleDimension.FromPixels(DialogueZoneXPos),
            Top = StyleDimension.FromPixels(DialogueZoneYPos)
        };
        backImage.Append(dialogueZone);

        dialogueText = new UIModifiedText { IsWrapped = true, horizontalWrapConstraint = DialogueTextWrapConstraint };
        dialogueText.SetPadding(DefaultElementPadding);
        dialogueZone.Append(dialogueText);

        //Buy Item Zone
        buyItemZone = new UIVisibilityElement {
            Width = StyleDimension.FromPixels(BuyItemZoneWidth),
            Height = StyleDimension.FromPixels(BuyItemZoneHeight),
            Left = StyleDimension.FromPixels(BuyItemZoneXPos),
            Top = StyleDimension.FromPixels(BuyItemZoneYPos)
        };
        backImage.Append(buyItemZone);

        buyItemHeader = new UIModifiedText("UI.VillagerShop.Buying".Localized(), 1.25f) { HAlign = 0.5f, Top = StyleDimension.FromPixels(4f) };
        buyItemZone.Append(buyItemHeader);

        buyItemIcon = new UIBetterItemIcon(new Item(ItemID.Acorn), ItemImageSideLength, true) {
            Width = StyleDimension.FromPixels(ItemImageSideLength), Height = StyleDimension.FromPixels(ItemImageSideLength), Top = StyleDimension.FromPixels(BuyItemIconYPos), HAlign = 0.5f
        };
        buyItemZone.Append(buyItemIcon);

        buyItemStockHeader = new UIModifiedText("UI.VillagerShop.Stock".Localized(), 1.25f) { Top = StyleDimension.FromPixels(BuyItemStockHeaderYPos), HAlign = 0.5f };
        buyItemZone.Append(buyItemStockHeader);

        buyItemStock = new UIModifiedText("1000", 1.25f) { Top = StyleDimension.FromPixels(BuyItemStockYPos), horizontalTextConstraint = BuyItemZoneWidth, HAlign = 0.5f };
        buyItemZone.Append(buyItemStock);

        buyItemButton = new UIPanelButton(vanillaPanelBackground, gradientPanelBorder, text: "UI.VillagerShop.Buy".Localized()) {
            BackgroundColor = new Color(59, 97, 203),
            BorderColor = Color.White,
            Width = StyleDimension.FromPixels(BuyItemButtonWidth),
            Height = StyleDimension.FromPixels(BuyItemButtonHeight),
            Top = StyleDimension.FromPixels(BuyItemButtonYPos),
            HAlign = 0.5f
        };
        buyItemZone.Append(buyItemButton);

        //Savings Zone
        savingsZone = new UIElement {
            Width = StyleDimension.FromPixels(SavingsZoneWidth),
            Height = StyleDimension.FromPixels(SavingsZoneHeight),
            Left = StyleDimension.FromPixels(SavingsZoneXPos),
            Top = StyleDimension.FromPixels(SavingsZoneYPos)
        };
        backImage.Append(savingsZone);

        savingsText = new UIModifiedText("UI.VillagerShop.Savings".Localized()) { Top = StyleDimension.FromPixels(SavingsTextYPos), HAlign = 0.5f };
        savingsZone.Append(savingsText);

        savingsDisplay = new UICoinDisplay(Main.LocalPlayer.CalculateTotalSavings()) { HAlign = 0.5f, VAlign = 0.5f };
        savingsZone.Append(savingsDisplay);

        //List Zone
        shopScrollbar = new UIScrollbar {
            Left = StyleDimension.FromPixelsAndPercent(ShopScrollbarXOffset, 1f), Top = StyleDimension.FromPixels(ShopScrollbarYPos), Height = StyleDimension.FromPixels(ShopScrollbarHeight)
        };
        shopZone.Append(shopScrollbar);

        shopList = new UIList { Width = StyleDimension.FromPixels(ShopListWidth), Height = StyleDimension.FromPixels(ShopListHeight) };
        shopList.SetPadding(6f);
        shopList.SetScrollbar(shopScrollbar);
        shopZone.Append(shopList);
    }

    public override void Update(GameTime gameTime) {
        if (backImage.ContainsPoint(Main.MouseScreen)) {
            Main.LocalPlayer.mouseInterface = true;
        }

        if (_selectedItem != null && buyItemButton.ContainsPoint(Main.MouseScreen) && Main.mouseLeft) {
            Player player = Main.LocalPlayer;
            VillagerShopItem villagerShopItem = _selectedItem.ShopItem;

            if (--_buyDelay < 0f) {
                _buyDelay = 0f;
            }

            if ((_buySpeed -= 1f / 60f) < 0f) {
                _buySpeed = 0f;
            }

            _buyDelay *= _buySpeed;

            if (villagerShopItem.Stock > 0) {
                if (player.CanAfford((int)_selectedItem.displayedCost) && player.CanAcceptItemIntoInventory(_selectedItem.displayedItem) && _buyDelay <= 0f) {
                    _buyDelay = MaxBuyDelay;

                    villagerShopItem.Stock--;
                    currentVillager.shopInventory[_selectedItem.shopIndex] = villagerShopItem;

                    player.BuyItem((int)_selectedItem.displayedCost);
                    player.QuickSpawnItem(new EntitySource_DropAsItem(player), _selectedItem.displayedItem);

                    _selectedItem.displayedItem.stack--;
                    _selectedItem.itemNameText.SetText(_selectedItem.displayedItem.HoverName);
                    buyItemStock.SetText(villagerShopItem.Stock.ToString());

                    savingsDisplay.moneyToDisplay = player.CalculateTotalSavings();

                    if (currentVillager.RelationshipStatus >= VillagerRelationship.Dislike) {
                        portrait.temporaryExpression = UIPortrait.VillagerPortraitExpression.Happy;
                        portrait.temporaryExpressionTimer = 120f;
                    }

                    dialogueText.SetText(DialogueSystem.Instance.GetDialogue(currentVillager.VillagerType, currentVillager.RelationshipStatus, DialogueType.ShopBuy));

                    SoundEngine.PlaySound(SoundID.Coins);
                }
            }
            else {
                SetSelectedItem(null);
            }
        }
        else {
            _buySpeed = 1f;
            //Set to zero so on the very first initial buy the purchase goes through instantly
            _buyDelay = 0f;
        }

        base.Update(gameTime);
    }

    public void ReloadUI(Villager newVillager) {
        currentVillager = newVillager;

        string shopUIPath = $"{LWM.SpritePath}Villages/UI/ShopUI/{currentVillager.VillagerType}/";

        backImage.SetImage(ModContent.Request<Texture2D>(shopUIPath + "BackImage"));

        shopOverlay.SetImage(ModContent.Request<Texture2D>(shopUIPath + "Overlay"));

        portrait.ReloadPortrait(currentVillager);

        nameText.SetText(currentVillager.NPC.GivenName, large: true);

        dialogueText.SetText(DialogueSystem.Instance.GetDialogue(currentVillager.VillagerType, currentVillager.RelationshipStatus, DialogueType.ShopInitial));

        savingsDisplay.moneyToDisplay = Main.LocalPlayer.CalculateTotalSavings();

        PopulateShopList();

        shopScrollbar.ViewPosition = 0f;

        RecalculateChildren();
    }

    /// <summary>
    ///     Changes the currently selected shop item index in order to be potentially bought by the
    ///     player. Passing in null will unselect all of the indices.
    /// </summary>
    /// <param name="newSelectedItem"> The newly selected shop item. </param>
    /// <param name="playSound"> Whether or not to play the sound of opening/closing the menu. </param>
    public void SetSelectedItem(UIShopItem newSelectedItem, bool playSound = true) {
        if (_selectedItem is not null) {
            _selectedItem.isSelected = false;
        }

        bool selectedElementOutOfStock = newSelectedItem?.ShopItem.Stock <= 0;
        if (newSelectedItem is null || _selectedItem == newSelectedItem || selectedElementOutOfStock) {
            _selectedItem = null;

            buyItemZone.SetVisibility(false);

            if (playSound) {
                SoundEngine.PlaySound(!selectedElementOutOfStock ? SoundID.MenuClose : SoundID.Tink);
            }

            return;
        }

        newSelectedItem.isSelected = true;

        buyItemIcon.SetItem(newSelectedItem.displayedItem);
        buyItemStock.SetText(currentVillager.shopInventory[newSelectedItem.shopIndex].Stock.ToString());

        buyItemZone.SetVisibility(true);

        if (playSound) {
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        _selectedItem = newSelectedItem;
    }

    /// <summary>
    ///     Populates the shop list full of entries of whatever the given villager being spoken to
    ///     is selling at the given moment.
    /// </summary>
    private void PopulateShopList() {
        shopList.Clear();

        float priceMult = LWMUtils.GetPriceMultiplierFromRep(currentVillager);

        for (int i = 0; i < currentVillager.shopInventory.Count; i++) {
            VillagerShopItem item = currentVillager.shopInventory[i];
            UIShopItem element = new(
                currentVillager,
                i,
                (long)Math.Round(item.Price * priceMult)
            );

            element.OnLeftClick += ClickedOnShopItem;

            shopList.Add(element);
        }
    }

    private void ClickedOnShopItem(UIMouseEvent evt, UIElement listeningElement) {
        SetSelectedItem((UIShopItem)listeningElement);
    }
}