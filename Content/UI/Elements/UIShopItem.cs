using Terraria.GameContent.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// UIImage class that holds different UITexts and UIElements that is the index in a given shop
    /// UI list. Holds data on the entire entry for the given item.
    /// </summary>
    public class UIShopItem : UIImage {
        public UIBetterItemIcon itemImage;
        public UIText stockText;
        public UIText itemCostText;

        public VillagerType villagerType;

        private Item displayedItem;
        private int costPerItem;

        public UIShopItem(int itemType, int remainingStock, int costPerItem, VillagerType villagerType) : base(ModContent.Request<Texture2D>($"{IOUtilities.LWMSpritePath}/UI/ShopUI/{villagerType}/{villagerType}ShopItemFrame")) {
            displayedItem = new Item();
            displayedItem.SetDefaults(itemType);
            itemImage = new UIBetterItemIcon(displayedItem, 32f);
            stockText = new UIText(remainingStock.ToString(), large: true);
            this.costPerItem = costPerItem;
            this.villagerType = villagerType;
        }

        public override void OnInitialize() {
            itemImage.VAlign = 0.5f;
            itemImage.Left.Set(36f, 0f);
            itemImage.Width.Set(32f, 0f);
            itemImage.Height.Set(32f, 0f);
            Append(itemImage);

            stockText.VAlign = 0.5f;
            stockText.Left.Set(325f, 0f);
            Append(stockText);
        }
    }
}