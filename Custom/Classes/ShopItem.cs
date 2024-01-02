using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Classes;

/// <summary>
/// Class that does what it says on the tin. Has fields for an instance of a shop item for
/// Villagers to sell as its primary use.
/// </summary>
public sealed class ShopItem : TagSerializable {
    public static readonly Func<TagCompound, ShopItem> DESERIALIZER = Deserialize;

    /// <summary>
    /// The price of the item type pertaining to this shop index.
    /// </summary>
    public long ItemPrice {
        get {
            if (_internalPrice.HasValue) {
                return _internalPrice.Value;
            }
            Item item = new();
            item.SetDefaults(itemType);
            return item.value;
        }
    }

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
    private readonly long? _internalPrice;

    public ShopItem(int itemType, int remainingStock, long? internalPrice) {
        this.itemType = itemType;
        this.remainingStock = remainingStock;
        _internalPrice = internalPrice;
    }

    public static bool operator ==(ShopItem shopItem1, ShopItem shopItem2) => shopItem1.itemType == shopItem2.itemType;

    public static bool operator !=(ShopItem shopItem1, ShopItem shopItem2) => shopItem1.itemType != shopItem2.itemType;

    public TagCompound SerializeData() {
        TagCompound tag = new() {
            { "ItemType", itemType },
            { "Stock", remainingStock },
            { "ItemPrice", ItemPrice }
        };

        if (itemType >= ItemID.Count) {
            ModItem modItem = ModContent.GetModItem(itemType);

            tag["ItemModName"] = modItem.Mod.Name;
            tag["ItemName"] = modItem.Name;
        }

        return tag;
    }

    //We only care about the item type when determining equality, as the other two factors are inconsequential
    public bool Equals(ShopItem other) => itemType == other.itemType;

    private static ShopItem Deserialize(TagCompound tag) {
        if (tag.TryGet("ItemModName", out string modName) && tag.TryGet("ItemName", out string itemName) && ModContent.TryFind(modName, itemName, out ModItem modItem)) {
            return new ShopItem(modItem.Type, tag.GetInt("Stock"), tag.GetLong("ItemPrice"));
        }

        return new ShopItem(tag.GetInt("ItemType"), tag.GetInt("Stock"), tag.GetLong("ItemPrice"));
    }
}