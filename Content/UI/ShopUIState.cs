using LivingWorldMod.Content.NPCs.Villagers;
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

        public void ReloadUI(Villager villager) {
            shopType = villager.VillagerType;

            string shopUIPath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/{shopType}/{shopType}";
            DynamicSpriteFont deathFont = FontAssets.DeathText.Value;

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

            nameText = new UIText(villager.NPC.GivenName, large: true);
            //Hardcoded because getting dimensions in general just seems to be extremely weird and inconsistent
            nameText.Left.Set(111f - (deathFont.MeasureString(villager.NPC.GivenName).X / 2f), 0f);
            nameText.Top.Set(24f + (deathFont.MeasureString(villager.NPC.GivenName).Y / 8f), 0f);
            nameFrame.Append(nameText);

            dialogueFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "DialogueFrame"));
            dialogueFrame.Left.Set(portraitFrame.Left.Pixels - 40f, 0f);
            dialogueFrame.Top.Set(375f, 0f);
            backFrame.Append(dialogueFrame);

            dialogueText = villager.ShopDialogue;

            RecalculateChildren();
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
    }
}