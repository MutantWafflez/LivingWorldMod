using System;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.Villages.DataStructures.Records;

/// <summary>
///     Data Structure that represents an item with a remaining "stock" and option for custom price. If <see cref="Price" /> is less than 0, the default price
///     of the item type is used.
/// </summary>
public record struct VillagerShopItem (int ItemType, int Stock, long Price = -1) : TagSerializable, IComparable<VillagerShopItem>, IComparable {
    private const string PriceSaveKey = "Price";
    private static readonly Item SaveItem = new ();
    public static readonly Func<TagCompound, VillagerShopItem> DESERIALIZER = Deserialize;

    public long Price {
        get;
    } = Price < 0 ? ContentSamples.ItemsByType[ItemType].value : Price;

    public static bool operator <(VillagerShopItem left, VillagerShopItem right) => left.CompareTo(right) < 0;

    public static bool operator >(VillagerShopItem left, VillagerShopItem right) => left.CompareTo(right) > 0;

    public static bool operator <=(VillagerShopItem left, VillagerShopItem right) => left.CompareTo(right) <= 0;

    public static bool operator >=(VillagerShopItem left, VillagerShopItem right) => left.CompareTo(right) >= 0;

    private static VillagerShopItem Deserialize(TagCompound tag) {
        ItemIO.Load(SaveItem, tag);

        long price = tag.GetLong(PriceSaveKey);
        return new VillagerShopItem(SaveItem.type, SaveItem.stack, price < 0 ? -1 : price);
    }

    public TagCompound SerializeData() {
        // Using item.stack as a save space for the Stock, to take full advantage of ItemIO.Save()
        SaveItem.SetDefaults(ItemType);
        SaveItem.stack = Stock;

        TagCompound tag = ItemIO.Save(SaveItem);
        tag[PriceSaveKey] = Price;

        return tag;
    }

    public int CompareTo(VillagerShopItem other) => ItemType.CompareTo(other.ItemType);

    public int CompareTo(object obj) {
        if (obj is null) {
            return 1;
        }

        return obj is VillagerShopItem other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(VillagerShopItem)}");
    }
}