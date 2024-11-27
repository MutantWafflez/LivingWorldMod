using System;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.Villages.DataStructures.Records;

/// <summary>
///     Class that does what it says on the tin. Has fields for an instance of a shop item for
///     Villagers to sell as its primary use.
/// </summary>
public record struct ShopItem (int ItemType, int RemainingStock, long ItemPrice = -1) : TagSerializable {
    public static readonly Func<TagCompound, ShopItem> DESERIALIZER = Deserialize;

    public long ItemPrice {
        get;
    } = ItemPrice < 0 ? ContentSamples.ItemsByType[ItemType].value : ItemPrice;

    private static ShopItem Deserialize(TagCompound tag) {
        int remainingStock = tag.GetInt("Stock");
        long itemPrice = tag.GetLong("ItemPrice");
        if (tag.TryGet("ItemModName", out string modName) && tag.TryGet("ItemName", out string itemName) && ModContent.TryFind(modName, itemName, out ModItem modItem)) {
            return new ShopItem(modItem.Type, remainingStock, itemPrice);
        }

        return new ShopItem(tag.GetInt("ItemType"), remainingStock, itemPrice);
    }

    public override int GetHashCode() => ItemType.GetHashCode();

    public readonly bool Equals(ShopItem other) => ItemType == other.ItemType;

    public TagCompound SerializeData() {
        TagCompound tag = new() { { "Stock", RemainingStock }, { "ItemPrice", ItemPrice } };

        if (ItemType >= ItemID.Count) {
            ModItem modItem = ModContent.GetModItem(ItemType);

            tag["ItemModName"] = modItem.Mod.Name;
            tag["ItemName"] = modItem.Name;

            return tag;
        }

        tag["ItemType"] = ItemType;
        return tag;
    }
}