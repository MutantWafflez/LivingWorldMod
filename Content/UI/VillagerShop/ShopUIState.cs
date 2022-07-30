using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Content.UI.CommonElements;
using LivingWorldMod.Content.UI.Elements;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.VillagerShop {
    /// <summary>
    /// UIState that handles the entire UI portion of the shop system for all villager types.
    /// </summary>
    public class ShopUIState : UIState {
        public Villager currentVillager;

        public UIImage backImage;
        public UIImage shopOverlay;

        public UIElement shopZone;

        public UIElement portraitZone;
        public UIPortrait portrait;

        public UIElement nameZone;
        public UIBetterText nameText;

        public UIElement dialogueZone;
        public UIBetterText dialogueText;

        public UIElement buyItemZone;
        public UIBetterText buyItemHeader;
        public UIBetterItemIcon buyItemIcon;
        public UIBetterText buyItemStockHeader;
        public UIBetterText buyItemStock;
        public UIBetterImageButton buyItemButton;

        public UIElement savingsZone;
        public UIBetterText savingsText;
        public UICoinDisplay savingsDisplay;

        public UIScrollbar shopScrollbar;
        public UIList shopList;

        private readonly float _maxBuyDelay = 60f;
        private float _buySpeed;
        private float _buyDelay;

        private UIShopItem _selectedItem;

        public override void OnInitialize() {
            string shopUIPath = $"{LivingWorldMod.LWMSpritePath}UI/ShopUI/Harpy/";

            //Background Zone
            backImage = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "BackImage", AssetRequestMode.ImmediateLoad)) {
                HAlign = 0.5f,
                VAlign = 0.5f
            };
            Append(backImage);

            shopOverlay = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "Overlay", AssetRequestMode.ImmediateLoad)) {
                HAlign = 0.5f,
                VAlign = 0.5f,
                IgnoresMouseInteraction = true
            };
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

            nameText = new UIBetterText(large: true) {
                HAlign = 0.5f,
                VAlign = 0.5f,
                horizontalTextConstraint = 184
            };
            nameZone.Append(nameText);

            //Dialogue Zone
            dialogueZone = new UIElement();
            dialogueZone.Width.Set(410f, 0f);
            dialogueZone.Height.Set(166f, 0f);
            dialogueZone.Left.Set(584f, 0f);
            dialogueZone.Top.Set(388f, 0f);
            backImage.Append(dialogueZone);

            dialogueText = new UIBetterText() {
                IsWrapped = true,
                horizontalWrapConstraint = 388f
            };
            dialogueText.SetPadding(12f);
            dialogueZone.Append(dialogueText);

            //Buy Item Zone
            buyItemZone = new UIElement();
            buyItemZone.Width.Set(158f, 0f);
            buyItemZone.Height.Set(136f, 0f);
            buyItemZone.Left.Set(562f, 0f);
            buyItemZone.Top.Set(90f, 0f);
            backImage.Append(buyItemZone);

            buyItemHeader = new UIBetterText("Buying:", 1.25f) {
                isVisible = false,
                HAlign = 0.5f
            };
            buyItemHeader.Top.Set(4f, 0f);
            buyItemZone.Append(buyItemHeader);

            buyItemIcon = new UIBetterItemIcon(new Item(ItemID.Acorn), 32f, true) {
                isVisible = false,
                HAlign = 0.5f
            };
            buyItemIcon.Width.Set(32f, 0f);
            buyItemIcon.Height.Set(32f, 0f);
            buyItemIcon.Top.Set(26f, 0f);
            buyItemZone.Append(buyItemIcon);

            buyItemStockHeader = new UIBetterText("Stock:", 1.25f) {
                isVisible = false,
                HAlign = 0.5f
            };
            buyItemStockHeader.Top.Set(56f, 0f);
            buyItemZone.Append(buyItemStockHeader);

            buyItemStock = new UIBetterText("1000", 1.25f) {
                isVisible = false,
                horizontalTextConstraint = 150,
                HAlign = 0.5f
            };
            buyItemStock.Top.Set(80f, 0f);
            buyItemZone.Append(buyItemStock);

            buyItemButton = new UIBetterImageButton(ModContent.Request<Texture2D>(shopUIPath + "BuyButton", AssetRequestMode.ImmediateLoad), "Buy") {
                isVisible = false,
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

            savingsText = new UIBetterText("Savings") {
                HAlign = 0.5f
            };
            savingsText.Top.Set(-26f, 0f);
            savingsZone.Append(savingsText);

            savingsDisplay = new UICoinDisplay(Main.LocalPlayer.CalculateTotalSavings()) {
                HAlign = 0.5f,
                VAlign = 0.5f
            };
            savingsZone.Append(savingsDisplay);

            //List Zone
            shopScrollbar = new UIScrollbar();
            shopScrollbar.Left.Set(466f, 0f);
            shopScrollbar.Top.Set(16f, 0f);
            shopScrollbar.Height.Set(464f, 0f);
            shopZone.Append(shopScrollbar);

            shopList = new UIList();
            shopList.Width.Set(470f, 0f);
            shopList.Height.Set(512f, 0f);
            shopList.SetPadding(6f);
            shopList.SetScrollbar(shopScrollbar);
            shopZone.Append(shopList);

            DummyPopulateShopList();
        }

        public override void Update(GameTime gameTime) {
            if (backImage.ContainsPoint(Main.MouseScreen)) {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (_selectedItem != null && buyItemButton.ContainsPoint(Main.MouseScreen) && Main.mouseLeft) {
                Player player = Main.LocalPlayer;
                ShopItem shopItem = _selectedItem.pertainedInventoryItem;

                if (--_buyDelay < 0f) {
                    _buyDelay = 0f;
                }

                if ((_buySpeed -= 1f / 60f) < 0f) {
                    _buySpeed = 0f;
                }

                _buyDelay *= _buySpeed;

                if (shopItem.remainingStock > 0) {
                    if (player.CanBuyItem((int)_selectedItem.displayedCost) && player.CanAcceptItemIntoInventory(_selectedItem.displayedItem) && _buyDelay <= 0f) {
                        _buyDelay = _maxBuyDelay;

                        shopItem.remainingStock--;

                        player.BuyItem((int)_selectedItem.displayedCost);
                        player.QuickSpawnItem(new EntitySource_DropAsItem(player), _selectedItem.displayedItem);

                        _selectedItem.displayedItem.stack--;
                        _selectedItem.itemNameText.SetText(_selectedItem.displayedItem.HoverName);
                        buyItemStock.SetText(shopItem.remainingStock.ToString());

                        savingsDisplay.moneyToDisplay = player.CalculateTotalSavings();

                        if (currentVillager.RelationshipStatus >= VillagerRelationship.Dislike) {
                            portrait.temporaryExpression = VillagerPortraitExpression.Happy;
                            portrait.temporaryExpressionTimer = 120f;
                        }

                        dialogueText.SetText(currentVillager.BuyShopChat);

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

            string shopUIPath = $"{LivingWorldMod.LWMSpritePath}UI/ShopUI/{currentVillager.VillagerType}/";

            backImage.SetImage(ModContent.Request<Texture2D>(shopUIPath + "BackImage", AssetRequestMode.ImmediateLoad));

            shopOverlay.SetImage(ModContent.Request<Texture2D>(shopUIPath + "Overlay", AssetRequestMode.ImmediateLoad));

            portrait.ChangePortraitType(currentVillager);

            nameText.SetText(currentVillager.NPC.GivenName, large: true);

            dialogueText.SetText(currentVillager.InitialShopChat);

            buyItemButton.SetImage(ModContent.Request<Texture2D>(shopUIPath + "BuyButton", AssetRequestMode.ImmediateLoad));

            savingsDisplay.moneyToDisplay = Main.LocalPlayer.CalculateTotalSavings();

            PopulateShopList();

            RecalculateChildren();
        }

        /// <summary>
        /// Changes the currently selected shop item index in order to be potentially bought by the
        /// player. Passing in null will unselect all of the indices.
        /// </summary>
        /// <param name="newSelectedItem"> The newly selected shop item. </param>
        /// <param name="playSound"> Whether or not to play the sound of opening/closing the menu. </param>
        public void SetSelectedItem(UIShopItem newSelectedItem, bool playSound = true) {
            _selectedItem = newSelectedItem;

            foreach (UIElement element in shopList) {
                if (element is UIShopItem shopItem) {
                    shopItem.isSelected = false;
                }
            }

            if (_selectedItem != null) {
                _selectedItem.isSelected = true;

                buyItemIcon.SetItem(_selectedItem.displayedItem);
                buyItemStock.SetText(_selectedItem.pertainedInventoryItem.remainingStock.ToString());

                buyItemHeader.isVisible = true;
                buyItemIcon.isVisible = true;
                buyItemStockHeader.isVisible = true;
                buyItemStock.isVisible = true;
                buyItemButton.isVisible = true;

                if (playSound) {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
            }
            else {
                buyItemHeader.isVisible = false;
                buyItemIcon.isVisible = false;
                buyItemStockHeader.isVisible = false;
                buyItemStock.isVisible = false;
                buyItemButton.isVisible = false;

                if (playSound) {
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
            }
        }

        /// <summary>
        /// Populates the shop list full of entries of whatever the given villager being spoken to
        /// is selling at the given moment.
        /// </summary>
        private void PopulateShopList() {
            shopList.Clear();

            float priceMult = NPCUtils.GetPriceMultiplierFromRep(currentVillager);

            foreach (ShopItem item in currentVillager.shopInventory) {
                UIShopItem element = new UIShopItem(item,
                    (long)Math.Round(item.ItemPrice * priceMult),
                    currentVillager.VillagerType);

                element.Activate();

                shopList.Add(element);
            }
        }

        /// <summary>
        /// Almost a carbon copy of PopulateShopList that purely exists for the purposes of UI
        /// initialization. The elements in the list are created upon mod load so when the player
        /// actually opens the UI, the list is properly initialized and the elements within them
        /// will be properly scaled in PopulateShopList(). Also used for debug purposes.
        /// </summary>
        private void DummyPopulateShopList() {
            shopList.Clear();

            for (int i = 0; i < Main.rand.Next(10, 51); i++) {
                UIShopItem element = new UIShopItem(new ShopItem(Main.rand.Next(ItemID.Count), 1000, 0),
                    Main.rand.Next(0, 10000000),
                    VillagerType.Harpy);

                element.Activate();

                shopList.Add(element);
            }

            shopScrollbar.Activate();
        }
    }
}