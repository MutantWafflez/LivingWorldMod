using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Content.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI {

    public class ShopUIState : UIState {
        public VillagerType shopType;

        public UIImage backFrame;
        public UIImage shopFrame;
        public UIImage portraitFrame;

        public UIImage nameFrame;
        public UIBetterText nameText;

        public UIImage dialogueFrame;
        public UIBetterText dialogueText;

        public UICoinDisplay savingsDisplay;

        public UIScrollbar shopScrollbar;
        public UIList shopList;

        public UIBetterText itemHeader;
        public UIBetterText stockHeader;
        public UIBetterText costHeader;

        public override void OnInitialize() {
            string shopUIPath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/Harpy/Harpy";

            backFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "BackFrame"));
            backFrame.HAlign = backFrame.VAlign = 0.5f;
            Append(backFrame);

            shopFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "ShopFrame"));
            shopFrame.Left.Set(50f, 0f);
            shopFrame.Top.Set(42.5f, 0f);
            backFrame.Append(shopFrame);

            portraitFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "PortraitFrame"));
            portraitFrame.Left.Set(675f, 0f);
            portraitFrame.Top.Set(50f, 0f);
            backFrame.Append(portraitFrame);

            nameFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "NameFrame"));
            nameFrame.Left.Set(portraitFrame.Left.Pixels, 0f);
            nameFrame.Top.Set(275f, 0f);
            backFrame.Append(nameFrame);

            nameText = new UIBetterText(large: true) {
                HAlign = 0.5f,
                VAlign = 0.5f,
                horizontalTextConstraint = 170f
            };
            nameFrame.Append(nameText);

            dialogueFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "DialogueFrame"));
            dialogueFrame.Left.Set(portraitFrame.Left.Pixels - 40f, 0f);
            dialogueFrame.Top.Set(375f, 0f);
            backFrame.Append(dialogueFrame);

            dialogueText = new UIBetterText() {
                IsWrapped = true,
                horizontalWrapConstraint = 240f
            };
            dialogueText.SetPadding(28f);
            dialogueFrame.Append(dialogueText);

            shopScrollbar = new UIScrollbar();
            shopScrollbar.Left.Set(472f, 0f);
            shopScrollbar.Top.Set(32f, 0f);
            shopScrollbar.Height.Set(448f, 0f);
            shopFrame.Append(shopScrollbar);

            savingsDisplay = new UICoinDisplay();
            savingsDisplay.Left.Set(580f, 0f);
            savingsDisplay.Top.Set(326f, 0f);
            backFrame.Append(savingsDisplay);

            shopList = new UIList();
            shopList.Width.Set(470f, 0f);
            shopList.Height.Set(490f, 0f);
            shopList.PaddingLeft = 26f;
            shopList.PaddingTop = 20f;
            shopList.ListPadding = 4f;
            shopList.SetScrollbar(shopScrollbar);
            shopFrame.Append(shopList);

            itemHeader = new UIBetterText("Item", 1.15f);
            itemHeader.Left.Set(156f, 0f);
            itemHeader.Top.Set(24f, 0f);
            backFrame.Append(itemHeader);

            stockHeader = new UIBetterText("Stock", 1.15f);
            stockHeader.Left.Set(276f, 0f);
            stockHeader.Top.Set(itemHeader.Top.Pixels, 0f);
            backFrame.Append(stockHeader);

            costHeader = new UIBetterText("Price", 1.15f);
            costHeader.Left.Set(382f, 0f);
            costHeader.Top.Set(itemHeader.Top.Pixels, 0f);
            backFrame.Append(costHeader);

            DummyPopulateShopList();
        }

        public void ReloadUI(Villager villager) {
            shopType = villager.VillagerType;

            string shopUIPath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/{shopType}/{shopType}";

            backFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "BackFrame"));

            shopFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "ShopFrame"));

            portraitFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "PortraitFrame"));

            nameFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "NameFrame"));

            nameText.SetText(villager.NPC.GivenName, large: true);

            dialogueFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "DialogueFrame"));

            dialogueText.SetText(villager.ShopDialogue);

            PopulateShopList(villager);

            RecalculateChildren();
        }

        public override void Update(GameTime gameTime) {
            if (backFrame.ContainsPoint(Main.MouseScreen)) {
                Main.LocalPlayer.mouseInterface = true;
            }

            base.Update(gameTime);
        }

        private void PopulateShopList(Villager villager) {
            shopList.Clear();

            for (int i = 0; i < 6; i++) {
                UIShopItem element = new UIShopItem(ItemID.TerraBlade,
                    15,
                    (ulong)Item.buyPrice(gold: 35, silver: 68, copper: 99),
                    villager.VillagerType);

                element.Activate();

                shopList.Add(element);
            }

            shopScrollbar.Activate();
        }

        /// <summary>
        /// Almost a carbon copy of PopulateShopList that purely exists for the purposes of UI
        /// initialization. The elements in the list are created upon mod load so when the player
        /// actually opens the UI, the list is properly initialized and the elements within them
        /// will be properly scaled in PopulateShopList(). Also used for debug purposes.
        /// </summary>
        private void DummyPopulateShopList() {
            shopList.Clear();

            for (int i = 0; i < 6; i++) {
                UIShopItem element = new UIShopItem(ItemID.TerraBlade,
                    15,
                    (ulong)Item.buyPrice(gold: 35, silver: 68, copper: 99),
                    VillagerType.Harpy);

                element.Activate();

                shopList.Add(element);
            }

            shopScrollbar.Activate();
        }
    }
}