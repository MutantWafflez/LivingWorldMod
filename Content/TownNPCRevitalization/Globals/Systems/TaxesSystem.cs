using System.Collections.Generic;
using System.IO;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.Content.TownNPCRevitalization.UI.TaxSheet;
using LivingWorldMod.Globals.BaseTypes.Systems;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     System that handles the Tax Collector's overhauled Taxes in tandem with the new "Tax Sheet" UI. See <see cref="TaxSheetUISystem" /> and <see cref="TaxSheetUIState" />.
/// </summary>
public class TaxesSystem : BaseModSystem<TaxesSystem>  {
    public const int MaxPropertyTax = Item.gold * 2 + Item.silver * 50;
    public const float MaxSalesTax = 0.4f;

    public const int TaxCap = Item.platinum;

    /// <summary>
    ///     LWM's new taxes are in-game day based, and as such, we need to change the tax rate variable to account for it.
    /// </summary>
    private const double LWMTaxRate = LWMUtils.InGameFullDay;

    /// <summary>
    ///     The tax rate that vanilla uses. <see cref="Player.taxRate" /> is the variable, and as of 1.4.4.9, is 3600.
    /// </summary>
    private const double VanillaTaxRate = 3600d;

    private const string ModSaveKey = "Mod";
    private const string NPCNameSaveKey = "NPCName";
    private const string TypeSaveKey = "Type";
    private const string InnerTaxValuesSaveKey = "Values";
    private const string TaxValuesListSaveKey = "TaxValues";

    // In vanilla, each NPC gets taxed 50 copper per in-game hour - since this new system is day based, we multiply that value by 24 for an in-game day
    // Also, sales tax does not exist in vanilla, so we're defaulting to 0%
    private static readonly NPCTaxValues DefaultTaxValues = new (Item.copper * 50 * 24, 0f);

    private readonly Dictionary<int, NPCTaxValues> _taxValues = [];

    /// <summary>
    ///     Returns whether or not the passed in tax values are valid; i.e. the values fit within their min-max range.
    /// </summary>
    public static bool AreValidTaxValues(NPCTaxValues values) => values is { SalesTax: >= 0 and <= MaxSalesTax, PropertyTax: >= 0 and <= MaxPropertyTax };

    /// <summary>
    ///     Returns whether a given NPC is valid to collect taxes from.
    /// </summary>
    public static bool IsNPCValidForTaxes(NPC npc, out int headIndex) {
        headIndex = -1;
        return TownGlobalNPC.EntityIsValidTownNPC(npc, true) && (headIndex = TownNPCProfiles.GetHeadIndexSafe(npc)) >= 0;
    }

    public override void Load() {
        Player.taxRate = LWMTaxRate;
    }

    public override void Unload() {
        Player.taxRate = VanillaTaxRate;
    }

    public override void SaveWorldData(TagCompound tag) {
        List<TagCompound> saveList = [];
        foreach ((int type, NPCTaxValues values) in _taxValues) {
            TagCompound taxTag = [];

            if (NPCLoader.GetNPC(type) is { } modNPC) {
                taxTag[ModSaveKey] = modNPC.Mod.Name;
                taxTag[NPCNameSaveKey] = modNPC.Name;
            }
            else {
                taxTag[TypeSaveKey] = type;
            }

            taxTag[InnerTaxValuesSaveKey] = values;

            saveList.Add(taxTag);
        }

        tag[TaxValuesListSaveKey] = saveList;
    }

    public override void LoadWorldData(TagCompound tag) {
        _taxValues.Clear();

        List<TagCompound> saveList = tag.Get<List<TagCompound>>(TaxValuesListSaveKey);
        foreach (TagCompound taxTag in saveList) {
            if (!taxTag.TryGet(TypeSaveKey, out int type)) {
                type = ModContent.Find<ModNPC>(taxTag.GetString(ModSaveKey), taxTag.GetString(NPCNameSaveKey)).Type;
            }

            _taxValues[type] = taxTag.Get<NPCTaxValues>(InnerTaxValuesSaveKey);
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

    public NPCTaxValues GetTaxValuesOrDefault(int type) => _taxValues.GetValueOrDefault(type, DefaultTaxValues);

    public void SubmitNewTaxValues(int npcType, NPCTaxValues newValues) {
        if (newValues == DefaultTaxValues) {
            _taxValues.Remove(npcType);

            return;
        }

        if (_taxValues.TryGetValue(npcType, out NPCTaxValues oldValues) && oldValues == newValues) {
            return;
        }

        _taxValues[npcType] = newValues;
    }
}