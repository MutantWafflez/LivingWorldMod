using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI {

    public class ShopUIState : UIState {
        public VillagerType shopType;

        public UIImage shopFrame;
        public UIImage portraitFrame;

        public void ReloadUI(Villager villager) {
            string shopUIPath = IOUtilities.LWMSpritePath + "/UI/ShopUI/";

            shopType = villager.VillagerType;

            shopFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + shopType + "ShopFrame"));
            shopFrame.Left.Set(475f, 0f);
            shopFrame.Top.Set(200f, 0f);
            Append(shopFrame);

            portraitFrame = new UIImage(ModContent.Request<Texture2D>(shopUIPath + shopType + "PortraitFrame"));
            portraitFrame.Left.Set(625f, 0f);
            portraitFrame.Top.Set(50f, 0f);
            shopFrame.Append(portraitFrame);
        }
    }
}