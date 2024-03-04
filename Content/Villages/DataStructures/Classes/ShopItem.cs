using System;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.Villages.DataStructures.Classes;

/// <summary>
/// Class that does what it says on the tin. Has fields for an instance of a shop item for
/// Villagers to sell as its primary use.
/// </summary>
public sealed class ShopItem : TagSerializable, IEquatable<ShopItem> {
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

    public override bool Equals(object obj) => Equals(obj as ShopItem);

    public override int GetHashCode() => itemType;

    public bool Equals(ShopItem other) {
        if (other is null) {
            return false;
        }
        if (ReferenceEquals(this, other)) {
            return true;
        }
        return itemType == other.itemType;
    }

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

    public static bool operator ==(ShopItem left, ShopItem right) => Equals(left, right);

    public static bool operator !=(ShopItem left, ShopItem right) => !Equals(left, right);

    private static ShopItem Deserialize(TagCompound tag) {
        if (tag.TryGet("ItemModName", out string modName) && tag.TryGet("ItemName", out string itemName) && ModContent.TryFind(modName, itemName, out ModItem modItem)) {
            return new ShopItem(modItem.Type, tag.GetInt("Stock"), tag.GetLong("ItemPrice"));
        }

        return new ShopItem(tag.GetInt("ItemType"), tag.GetInt("Stock"), tag.GetLong("ItemPrice"));
    }
}