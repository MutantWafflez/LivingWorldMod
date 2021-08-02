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

        public UIImage backImage;
        public UIImage shopOverlay;

        public UIElement shopZone;

        public UIElement portraitZone;
        public UIPortrait portrait;

        public UIElement nameZone;
        public UIBetterText nameText;

        public UIElement dialogueZone;
        public UIBetterText dialogueText;

        public UIElement savingsZone;
        public UICoinDisplay savingsDisplay;

        public UIScrollbar shopScrollbar;
        public UIList shopList;

        public override void OnInitialize() {
            string shopUIPath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/Harpy/";

            backImage = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "BackImage"));
            backImage.HAlign = backImage.VAlign = 0.5f;
            Append(backImage);

            shopOverlay = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "Overlay"));
            backImage.Append(shopOverlay);

            shopZone = new UIElement();
            shopZone.Width.Set(504f, 0f);
            shopZone.Height.Set(504f, 0f);
            shopZone.Left.Set(56f, 0f);
            shopZone.Top.Set(50f, 0f);
            backImage.Append(shopZone);

            portraitZone = new UIElement();
            portraitZone.Width.Set(196f, 0f);
            portraitZone.Height.Set(196f, 0f);
            portraitZone.Left.Set(732f, 0f);
            portraitZone.Top.Set(120f, 0f);
            backImage.Append(portraitZone);

            portrait = new UIPortrait(VillagerType.Harpy);
            portrait.Top.Set(4f, 0f);
            portraitZone.Append(portrait);

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

            shopScrollbar = new UIScrollbar();
            shopScrollbar.Left.Set(472f, 0f);
            shopScrollbar.Top.Set(16f, 0f);
            shopScrollbar.Height.Set(464f, 0f);
            shopZone.Append(shopScrollbar);

            savingsZone = new UIElement();
            savingsZone.Width.Set(126f, 0f);
            savingsZone.Height.Set(84f, 0f);
            savingsZone.Left.Set(576f, 0f);
            savingsZone.Top.Set(260f, 0f);
            backImage.Append(savingsZone);

            savingsDisplay = new UICoinDisplay();
            savingsDisplay.HAlign = savingsDisplay.VAlign = 0.5f;
            savingsZone.Append(savingsDisplay);

            shopList = new UIList();
            shopList.Width.Set(470f, 0f);
            shopList.Height.Set(512f, 0f);
            shopList.SetPadding(6f);
            shopList.SetScrollbar(shopScrollbar);
            shopZone.Append(shopList);

            DummyPopulateShopList();
        }

        public void ReloadUI(Villager villager) {
            shopType = villager.VillagerType;

            string shopUIPath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/{shopType}/";

            backImage.SetImage(ModContent.Request<Texture2D>(shopUIPath + "BackImage"));

            shopOverlay.SetImage(ModContent.Request<Texture2D>(shopUIPath + "Overlay"));

            portrait.ChangePortraitType(shopType);

            nameText.SetText(villager.NPC.GivenName, large: true);

            dialogueText.SetText(villager.ShopDialogue);

            PopulateShopList(villager);

            RecalculateChildren();
        }

        public override void Update(GameTime gameTime) {
            if (backImage.ContainsPoint(Main.MouseScreen)) {
                Main.LocalPlayer.mouseInterface = true;
            }

            base.Update(gameTime);
        }

        private void PopulateShopList(Villager villager) {
            DummyPopulateShopList();
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
                UIShopItem element = new UIShopItem(Main.rand.Next(ItemID.DirtBlock, ItemID.Count),
                    Main.rand.Next(1000),
                    (ulong)Main.rand.Next(0, 10000000),
                    VillagerType.Harpy);

                element.Activate();

                shopList.Add(element);
            }

            shopScrollbar.Activate();
        }
    }
}