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

// Future Mutant: This is wack. All of it is wack. I don't like it. As a matter of fact, I hate it. Needs a complete re-write, eventually. 
public class ShopUIState : UIState {
    /// <summary>
    ///     UIImage class that holds different UITexts and UIElements that is the index in a given shop
    ///     UI list. Holds data on the entire entry for the given item.
    /// </summary>
    public class UIShopItem : UIImage {
        private const float ItemImageSize = 32f;

        public readonly Item displayedItem;
        public readonly UIModifiedText itemNameText;

        public readonly long displayedCost;
        public readonly int shopIndex;

        public bool isSelected;

        private readonly UIBetterItemIcon _itemImage;
        private readonly UICoinDisplay _itemCostDisplay;

        private readonly Villager _villager;

        private float _manualUpdateTime;

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

            Width = StyleDimension.FromPixels(448f);
            Height = StyleDimension.FromPixels(106f);

            _itemImage = new UIBetterItemIcon(displayedItem, ItemImageSize, true) { VAlign = 0.5f, IgnoresMouseInteraction = true };
            _itemImage.Left.Set(38f, 0f);
            _itemImage.Width.Set(ItemImageSize, 0f);
            _itemImage.Height.Set(ItemImageSize, 0f);
            Append(_itemImage);

            itemNameText = new UIModifiedText(displayedItem.HoverName, 1.25f) { VAlign = 0.5f, horizontalTextConstraint = 194f, IgnoresMouseInteraction = true };
            itemNameText.Left.Set(94f, 0f);
            Append(itemNameText);

            _itemCostDisplay = new UICoinDisplay(displayedCost, UICoinDisplay.CoinDrawStyle.NoCoinsWithZeroValue, 1.34f) { VAlign = 0.5f, IgnoresMouseInteraction = true };
            _itemCostDisplay.Left.Set(-_itemCostDisplay.Width.Pixels - 12f, 1f);
            Append(_itemCostDisplay);

            OnMouseOver += MousedOverElement;
            OnMouseOut += MouseExitedElement;
        }

        private static void MousedOverElement(UIMouseEvent evt, UIElement listeningElement) {
            SoundEngine.PlaySound(SoundID.MenuTick);
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

            if (ContainsPoint(Main.MouseScreen) || isSelected) {
                Effect shader = ShopUISystem.hoverFlashShader.Value;

                _manualUpdateTime += 1f / 45f;
                if (_manualUpdateTime >= MathHelper.TwoPi) {
                    _manualUpdateTime = 0f;
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, defaultRasterizerState, shader, Main.UIScaleMatrix);

                shader.Parameters["manualUTime"].SetValue(_manualUpdateTime);
                base.DrawSelf(spriteBatch);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, defaultRasterizerState, null, Main.UIScaleMatrix);
            }
            else {
                base.DrawSelf(spriteBatch);
            }
        }

        private void MouseExitedElement(UIMouseEvent evt, UIElement listeningElement) {
            if (!isSelected) {
                _manualUpdateTime = 0f;
            }
        }
    }

    private const float MaxBuyDelay = 60f;

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
        shopZone = new UIElement();
        shopZone.Width.Set(504f, 0f);
        shopZone.Height.Set(504f, 0f);
        shopZone.Left.Set(56f, 0f);
        shopZone.Top.Set(50f, 0f);
        backImage.Append(shopZone);

        //Portrait Zone
        portraitZone = new UIElement();
        portraitZone.Width.Set(196f, 0f);
        portraitZone.Height.Set(196f, 0f);
        portraitZone.Left.Set(732f, 0f);
        portraitZone.Top.Set(120f, 0f);
        backImage.Append(portraitZone);

        portrait = new UIPortrait(new HarpyVillager());
        portrait.Top.Set(4f, 0f);
        portraitZone.Append(portrait);

        //Name Zone
        nameZone = new UIElement();
        nameZone.Width.Set(196f, 0f);
        nameZone.Height.Set(60f, 0f);
        nameZone.Left.Set(734f, 0f);
        nameZone.Top.Set(322f, 0f);
        backImage.Append(nameZone);

        nameText = new UIModifiedText(large: true) { HAlign = 0.5f, VAlign = 0.5f, horizontalTextConstraint = 184 };
        nameZone.Append(nameText);

        //Dialogue Zone
        dialogueZone = new UIElement();
        dialogueZone.Width.Set(410f, 0f);
        dialogueZone.Height.Set(166f, 0f);
        dialogueZone.Left.Set(584f, 0f);
        dialogueZone.Top.Set(388f, 0f);
        backImage.Append(dialogueZone);

        dialogueText = new UIModifiedText { IsWrapped = true, horizontalWrapConstraint = 388f };
        dialogueText.SetPadding(12f);
        dialogueZone.Append(dialogueText);

        //Buy Item Zone
        buyItemZone = new UIVisibilityElement();
        buyItemZone.Width.Set(158f, 0f);
        buyItemZone.Height.Set(136f, 0f);
        buyItemZone.Left.Set(562f, 0f);
        buyItemZone.Top.Set(90f, 0f);
        backImage.Append(buyItemZone);

        buyItemHeader = new UIModifiedText("UI.VillagerShop.Buying".Localized(), 1.25f) {  HAlign = 0.5f };
        buyItemHeader.Top.Set(4f, 0f);
        buyItemZone.Append(buyItemHeader);

        buyItemIcon = new UIBetterItemIcon(new Item(ItemID.Acorn), 32f, true) {  HAlign = 0.5f };
        buyItemIcon.Width.Set(32f, 0f);
        buyItemIcon.Height.Set(32f, 0f);
        buyItemIcon.Top.Set(26f, 0f);
        buyItemZone.Append(buyItemIcon);

        buyItemStockHeader = new UIModifiedText("UI.VillagerShop.Stock".Localized(), 1.25f) {  HAlign = 0.5f };
        buyItemStockHeader.Top.Set(56f, 0f);
        buyItemZone.Append(buyItemStockHeader);

        buyItemStock = new UIModifiedText("1000", 1.25f) {  horizontalTextConstraint = 150, HAlign = 0.5f };
        buyItemStock.Top.Set(80f, 0f);
        buyItemZone.Append(buyItemStock);

        buyItemButton = new UIPanelButton(vanillaPanelBackground, gradientPanelBorder, text: "UI.VillagerShop.Buy".Localized()) {
            BackgroundColor = new Color(59, 97, 203),
            BorderColor = Color.White,
            Width = StyleDimension.FromPixels(70f),
            Height = StyleDimension.FromPixels(30f),
            HAlign = 0.5f
        };
        buyItemButton.Top.Set(102f, 0f);
        buyItemZone.Append(buyItemButton);

        //Savings Zone
        savingsZone = new UIElement();
        savingsZone.Width.Set(126f, 0f);
        savingsZone.Height.Set(84f, 0f);
        savingsZone.Left.Set(576f, 0f);
        savingsZone.Top.Set(260f, 0f);
        backImage.Append(savingsZone);

        savingsText = new UIModifiedText("UI.VillagerShop.Savings".Localized()) { HAlign = 0.5f };
        savingsText.Top.Set(-26f, 0f);
        savingsZone.Append(savingsText);

        savingsDisplay = new UICoinDisplay(Main.LocalPlayer.CalculateTotalSavings()) { HAlign = 0.5f, VAlign = 0.5f };
        savingsZone.Append(savingsDisplay);

        //List Zone
        shopScrollbar = new UIScrollbar();
        shopScrollbar.Left.Set(466f, 0f);
        shopScrollbar.Top.Set(16f, 0f);
        shopScrollbar.Height.Set(464f, 0f);
        shopZone.Append(shopScrollbar);

        shopList = [];
        shopList.Width.Set(470f, 0f);
        shopList.Height.Set(512f, 0f);
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
        bool selectedElementOutOfStock = newSelectedItem?.ShopItem.Stock <= 0;
        if (newSelectedItem is null || _selectedItem == newSelectedItem || selectedElementOutOfStock) {
            _selectedItem = null;

            buyItemZone.SetVisibility(false);

            if (playSound) {
                SoundEngine.PlaySound(!selectedElementOutOfStock ? SoundID.MenuClose : SoundID.Tink);
            }

            return;
        }

        if (_selectedItem is not null) {
            _selectedItem.isSelected = false;
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