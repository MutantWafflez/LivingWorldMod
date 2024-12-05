using System;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.Villages.DataStructures.Records;

/// <summary>
///     Class that does what it says on the tin. Has fields for an instance of a shop item for
///     Villagers to sell as its primary use.
/// </summary>
public class ShopItem (int itemType, int remainingStock, long itemPrice = -1) : TagSerializable, IComparable<ShopItem> {
    public static readonly Func<TagCompound, ShopItem> DESERIALIZER = Deserialize;

    public readonly int itemType = itemType;

    public int remainingStock = remainingStock;

    public long ItemPrice {
        get;
    } = itemPrice < 0 ? ContentSamples.ItemsByType[itemType].value : itemPrice;

    private static ShopItem Deserialize(TagCompound tag) {
        int remainingStock = tag.GetInt("Stock");
        long itemPrice = tag.GetLong("ItemPrice");
        if (tag.TryGet("ItemModName", out string modName) && tag.TryGet("ItemName", out string itemName) && ModContent.TryFind(modName, itemName, out ModItem modItem)) {
            return new ShopItem(modItem.Type, remainingStock, itemPrice);
        }

        return new ShopItem(tag.GetInt("ItemType"), remainingStock, itemPrice);
    }

    public override int GetHashCode() => itemType.GetHashCode();

    public bool Equals(ShopItem other) => itemType == other.itemType;

    public TagCompound SerializeData() {
        TagCompound tag = new() { { "Stock", remainingStock }, { "ItemPrice", ItemPrice } };

        if (itemType >= ItemID.Count) {
            ModItem modItem = ModContent.GetModItem(itemType);

            tag["ItemModName"] = modItem.Mod.Name;
            tag["ItemName"] = modItem.Name;

            return tag;
        }

        tag["ItemType"] = itemType;
        return tag;
    }

    public int CompareTo(ShopItem other) => itemType.CompareTo(other.itemType);
}