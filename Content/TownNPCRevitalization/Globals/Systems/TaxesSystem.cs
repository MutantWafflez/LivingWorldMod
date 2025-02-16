using System;
using System.Collections.Generic;
using System.IO;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.Content.TownNPCRevitalization.UI.TaxSheet;
using LivingWorldMod.Globals.BaseTypes.Systems;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     System that handles the Tax Collector's overhauled Taxes in tandem with the new "Tax Sheet" UI. See <see cref="TaxSheetUISystem" /> and <see cref="TaxSheetUIState" />.
/// </summary>
public class TaxesSystem : BaseModSystem<TaxesSystem>  {
    private readonly record struct NPCTaxValues (int PropertyTax, float SalesTax) : TagSerializable {
        
        public static readonly Func<TagCompound, NPCTaxValues> DESERIALIZER = Deserialize;
        
        public TagCompound SerializeData() => new() {
            {nameof(PropertyTax), PropertyTax},
            {nameof(SalesTax), SalesTax}
        };

        private static NPCTaxValues Deserialize(TagCompound tag) {
            return new NPCTaxValues(tag.GetInt(nameof(PropertyTax)), tag.GetInt(nameof(SalesTax)));
        }
    };

    public const int MaxPropertyTax = Item.gold * 2 + Item.silver * 50;

    public const float MaxSalesTax = 0.4f;

    // In vanilla, each NPC gets taxed 50 copper per in-game hour - since this new system is day based, we multiply that value by 24 for an in-game day
    private static readonly NPCTaxValues DefaultTaxValues = new (Item.copper * 50 * 24, 0f);

    private readonly Dictionary<int, NPCTaxValues> _taxValues = [];

    public override void SaveWorldData(TagCompound tag) {
        List<TagCompound> saveList = [];
        foreach ((int type, NPCTaxValues values) in _taxValues) {
            TagCompound data = [];

            if (NPCLoader.GetNPC(type) is { } modNPC) {
                data["mod"] = modNPC.Mod.Name;
                data["npcName"] = modNPC.Name;
            }
            else {
                data["type"] = type;
            }

            data["values"] = values;
            
            saveList.Add(data);
        }

        tag["taxValues"] = saveList;
    }

    public override void LoadWorldData(TagCompound tag) {
        _taxValues.Clear();
        
        List<TagCompound> saveList = tag.Get<List<TagCompound>>("taxValues");
        foreach (TagCompound taxTag in saveList) {
            if (!tag.TryGet("type", out int type)) {
                type = ModContent.Find<ModNPC>(tag.GetString("mod"), tag.GetString("npcName")).Type;
            }

            _taxValues[type] = tag.Get<NPCTaxValues>("values");
        }
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(_taxValues.Count);
        
        foreach ((int type, NPCTaxValues values) in _taxValues) {
            writer.Write(type);
            writer.Write(values.PropertyTax);
            writer.Write(values.SalesTax);
        }
    }

    public override void NetReceive(BinaryReader reader) {
        _taxValues.Clear();
        
        int taxValueCount = reader.ReadInt32();
        for (int i = 0; i < taxValueCount; i++) {
            _taxValues[reader.ReadInt32()] = new NPCTaxValues(reader.ReadInt32(), reader.ReadSingle());
        }
    }
}