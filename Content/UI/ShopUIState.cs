using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
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
        public UIText dialogueText;

        public void ReloadUI(Villager villager) {
            shopType = villager.VillagerType;

            string shopUIPath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/{shopType}/{shopType}";

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

            nameText = new UIText(villager.NPC.GivenName);
            nameFrame.Append(nameText);

            dialogueFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + "DialogueFrame"));
            dialogueFrame.Left.Set(portraitFrame.Left.Pixels - 40f, 0f);
            dialogueFrame.Top.Set(375f, 0f);
            backFrame.Append(dialogueFrame);

            float dialogueTextPadding = 24f;
            dialogueText = new UIText(villager.GetChat());
            dialogueText.SetPadding(dialogueTextPadding);
            dialogueText.Width.Set(0, 1f);
            dialogueText.Height.Set(0, 1f);
            dialogueText.IsWrapped = true;
            dialogueText.DynamicallyScaleDownToWidth = true;
            dialogueFrame.Append(dialogueText);
        }
    }
}