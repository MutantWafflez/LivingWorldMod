using Terraria.GameContent.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// UIElement class that holds different UIImages and UIElements that is the index in a given
    /// shop UI list. Holds data on the entire entry for the given t
    /// </summary>
    public class ShopItemUIElement : UIElement {
        public UIImage shopItemFrame;
        public UIImage itemImage;

        public string itemName;

        public int remainingStock;
        public int costPerItem;

        public VillagerType villagerType;

        public ShopItemUIElement(Asset<Texture2D> itemTexture, string itemName, int remainingStock, int costPerItem, VillagerType villagerType) {
            shopItemFrame = new UIImage(ModContent.Request<Texture2D>($"{IOUtilities.LWMSpritePath}/UI/ShopUI/{villagerType}/{villagerType}ShopItemFrame"));
            itemImage = new UIImage(itemTexture);
            this.itemName = itemName;
            this.remainingStock = remainingStock;
            this.costPerItem = costPerItem;
            this.villagerType = villagerType;
            Activate();
        }

        public override void OnInitialize() {
            Width.Set(424f, 0f);
            Height.Set(110f, 0f);
            Append(shopItemFrame);

            itemImage.SetPadding(48f);
            shopItemFrame.Append(itemImage);
        }
    }
}