using System;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

/// <summary>
///     Representation of a given NPC's daily property tax and their by-sale sales tax.
/// </summary>
/// <param name="PropertyTax"> How much money the Tax Collector "collects" from this NPC every day. </param>
/// <param name="SalesTax"> What percentage of every sale an NPC with a shop will have to "give" to the Tax Collector. </param>
public readonly record struct NPCTaxValues(int PropertyTax, float SalesTax) : TagSerializable {
    public static readonly Func<TagCompound, NPCTaxValues> DESERIALIZER = Deserialize;

    private static NPCTaxValues Deserialize(TagCompound tag) => new (tag.GetInt(nameof(PropertyTax)), tag.GetInt(nameof(SalesTax)));

    public TagCompound SerializeData() => new() { { nameof(PropertyTax), PropertyTax }, { nameof(SalesTax), SalesTax } };
}