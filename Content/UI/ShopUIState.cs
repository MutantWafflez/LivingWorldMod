using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Content.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI {

    public class ShopUIState : UIState {
        public VillagerType shopType;

        public UIImage backFrame;
        public UIImage shopFrame;
        public UIImage portraitFrame;

        public UIImage nameFrame;
        public UIText nameText;

        public UIImage dialogueFrame;
        public string dialogueText;

        public UIScrollbar shopScrollbar;
        public UIList shopList;

        public override void OnInitialize() {
            string shopUIPath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/Harpy/Harpy";

            backFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "BackFrame"));
            backFrame.Left.Set(475f, 0f);
            backFrame.Top.Set(175f, 0f);
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

            nameText = new UIText("null", large: true);
            nameFrame.Append(nameText);

            dialogueFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "DialogueFrame"));
            dialogueFrame.Left.Set(portraitFrame.Left.Pixels - 40f, 0f);
            dialogueFrame.Top.Set(375f, 0f);
            backFrame.Append(dialogueFrame);

            shopScrollbar = new UIScrollbar();
            shopScrollbar.Left.Set(472f, 0f);
            shopScrollbar.Top.Set(32f, 0f);
            shopScrollbar.Height.Set(448f, 0f);
            shopFrame.Append(shopScrollbar);

            shopList = new UIList();
            shopList.Width.Set(470f, 0f);
            shopList.Height.Set(490f, 0f);
            shopList.PaddingLeft = 26f;
            shopList.PaddingTop = 28f;
            shopList.ListPadding = 4f;
            shopList.SetScrollbar(shopScrollbar);
            shopFrame.Append(shopList);
        }

        public void ReloadUI(Villager villager) {
            shopType = villager.VillagerType;

            string shopUIPath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/{shopType}/{shopType}";
            DynamicSpriteFont deathFont = FontAssets.DeathText.Value;

            backFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "BackFrame"));

            shopFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "ShopFrame"));

            portraitFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "PortraitFrame"));

            nameFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "NameFrame"));

            nameText.SetText(villager.NPC.GivenName, 1f, true);
            //Hardcoded because getting dimensions in general just seems to be extremely weird and inconsistent
            nameText.Left.Set(111f - (deathFont.MeasureString(villager.NPC.GivenName).X / 2f), 0f);
            nameText.Top.Set(24f + (deathFont.MeasureString(villager.NPC.GivenName).Y / 8f), 0f);

            dialogueFrame.SetImage(ModContent.Request<Texture2D>(shopUIPath + "DialogueFrame"));

            dialogueText = villager.ShopDialogue;

            PopulateShopList(villager);
        }

        protected override void DrawChildren(SpriteBatch spriteBatch) {
            base.DrawChildren(spriteBatch);
            //Manually draw dialogue text cause UIText is funky with wrapping
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            //Hardcoded values here because getting InnerDimensions() and calculating position with it for some reason changes values when closing then re-opening the UI?? Terraria's UI code perplexes me
            Vector2 stringPos = new Vector2(1134f, 574f);

            string visibleText = font.CreateWrappedText(dialogueText, 260f);

            Utils.DrawBorderString(spriteBatch, visibleText, stringPos, Color.White);
        }

        private void PopulateShopList(Villager villager) {
            shopList.Clear();

            for (int i = 0; i < 6; i++) {
                shopList.Add(new ShopItemUIElement(Main.Assets.Request<Texture2D>("Item_1"), "Test", 5, 5, VillagerType.Harpy));
            }
            shopScrollbar.Activate();
        }
    }
}