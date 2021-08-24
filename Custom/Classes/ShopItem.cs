﻿using Terraria;

namespace LivingWorldMod.Custom.Classes {

    /// <summary>
    /// Class that does what it says on the tin. Has fields for an instance of a shop item for
    /// Villagers to sell as its primary use.
    /// </summary>
    public sealed class ShopItem {

        /// <summary>
        /// The item type that pertains to this shop index.
        /// </summary>
        public readonly int itemType;

        /// <summary>
        /// The remaining stock left for the item type pertaining to this shop index.
        /// </summary>
        public int remainingStock;

        /// <summary>
        /// The price of the item type pertaining to this shop index. If null, uses the default
        /// vanilla value of the item.
        /// </summary>
        private readonly long? internalPrice;

        /// <summary>
        /// The price of the item type pertaining to this shop index.
        /// </summary>
        public long ItemPrice {
            get {
                if (internalPrice.HasValue) {
                    return internalPrice.Value;
                }
                else {
                    Item item = new Item();
                    item.SetDefaults(itemType);
                    return item.value;
                }
            }
        }

        public ShopItem(int itemType, int remainingStock, long? internalPrice) {
            this.itemType = itemType;
            this.remainingStock = remainingStock;
            this.internalPrice = internalPrice;
        }

        public static bool operator ==(ShopItem shopItem1, ShopItem shopItem2) => shopItem1.itemType == shopItem2.itemType;

        public static bool operator !=(ShopItem shopItem1, ShopItem shopItem2) => shopItem1.itemType != shopItem2.itemType;

        //We only care about the item type when determining equality, as the other two factors are inconsequential
        public bool Equals(ShopItem other) {
            return itemType == other.itemType;
        }

        public override bool Equals(object obj) {
            return obj is ShopItem other && Equals(other);
        }

        public override int GetHashCode() {
            return itemType;
        }
    }
}