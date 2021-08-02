using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// UIElement class extension that handles and creates portraits for villagers in the shop UI, primarily.
    /// </summary>
    public class UIPortrait : UIElement {
        public UIImage portraitBase;
        public UIImage portraitClothing;
        public UIImage portraitHair;
        public UIImage portraitFace;

        private VillagerType villagerType;

        public UIPortrait(VillagerType villagerType) {
            this.villagerType = villagerType;
            Width.Set(190f, 0f);
            Height.Set(190f, 0f);
        }

        public override void OnInitialize() {
            string portraitSpritePath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/{villagerType}/Portraits/";

            portraitBase = new UIImage(ModContent.Request<Texture2D>(portraitSpritePath + "Base"));
            Append(portraitBase);

            portraitClothing = new UIImage(ModContent.Request<Texture2D>(portraitSpritePath + "Body1"));
            Append(portraitClothing);

            portraitHair = new UIImage(ModContent.Request<Texture2D>(portraitSpritePath + "Hair1"));
            Append(portraitHair);

            portraitFace = new UIImage(ModContent.Request<Texture2D>(portraitSpritePath + "FaceNeutral"));
            Append(portraitFace);
        }

        public void ChangePortraitType(VillagerType newType) {
            villagerType = newType;
            ReloadPortrait();
        }

        public void ReloadPortrait() {
            string portraitSpritePath = $"{IOUtilities.LWMSpritePath}/UI/ShopUI/{villagerType}/Portraits/";

            portraitBase.SetImage(ModContent.Request<Texture2D>(portraitSpritePath + "Base"));

            portraitClothing.SetImage(ModContent.Request<Texture2D>(portraitSpritePath + "Body1"));

            portraitHair.SetImage(ModContent.Request<Texture2D>(portraitSpritePath + "Hair1"));

            portraitFace.SetImage(ModContent.Request<Texture2D>(portraitSpritePath + "FaceNeutral"));
        }
    }
}