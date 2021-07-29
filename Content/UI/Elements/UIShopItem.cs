using Terraria.GameContent.UI.Elements;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.UI.Elements {

    /// <summary>
    /// UIImage class that holds different UITexts and UIElements that is the index in a given shop
    /// UI list. Holds data on the entire entry for the given item.
    /// </summary>
    public class UIShopItem : UIImage {
        public UIBetterItemIcon itemImage;
        public UIBetterText itemNameText;
        public UIText stockText;
        public UICoinDisplay itemCostDisplay;

        public Item displayedItem;

        public int remainingStock;
        public ulong costPerItem;

        public VillagerType villagerType;

        public UIShopItem(int itemType, int remainingStock, ulong costPerItem, VillagerType villagerType) : base(ModContent.Request<Texture2D>($"{IOUtilities.LWMSpritePath}/UI/ShopUI/{villagerType}/{villagerType}ShopItemFrame")) {
            displayedItem = new Item();
            displayedItem.SetDefaults(itemType);
            this.remainingStock = remainingStock;
            this.costPerItem = costPerItem;
            this.villagerType = villagerType;
        }

        public override void OnInitialize() {
            itemImage = new UIBetterItemIcon(displayedItem, 32f) {
                VAlign = 0.5f
            };
            itemImage.Left.Set(28f, 0f);
            itemImage.Width.Set(32f, 0f);
            itemImage.Height.Set(32f, 0f);
            Append(itemImage);

            itemNameText = new UIBetterText(displayedItem.HoverName, 1.25f) {
                horizontalTextConstraint = 140f,
                VAlign = 0.5f
            };
            itemNameText.Left.Set(66f, 0f);
            Append(itemNameText);

            stockText = new UIText(remainingStock.ToString(), 1.25f) {
                VAlign = 0.5f
            };
            stockText.Left.Set(224f, 0f);
            Append(stockText);

            itemCostDisplay = new UICoinDisplay(costPerItem, 1.34f, true) {
                VAlign = 0.5f
            };
            itemCostDisplay.Left.Set(260f, 0f);
            Append(itemCostDisplay);
        }
    }
}