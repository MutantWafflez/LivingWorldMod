using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Version of <see cref="BiomePreferenceListTrait" /> that is single-instanced per biome as well as numeric - i.e. uses the Town NPC Revitalization mood offset instead of
///     <see cref="AffectionLevel" /> objects.
/// </summary>
public class NumericBiomePreferenceTrait(int moodOffset, IShoppingBiome biome) : IPersonalityTrait {
    public override string ToString() => $"Biome: {biome.NameKey}, Offset: {moodOffset}";

    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        if (!biome.IsInBiome(info.Player))  {
            return;
        }

        info.NPC.GetGlobalNPC<TownNPCMoodModule>()
            .AddModifier(
                new DynamicLocalizedText("TownNPCMoodDescription.InBiome".Localized(), new { Biome = ShopHelper.BiomeNameByKey(biome.NameKey) }),
                TownNPCDataSystem.GetAutoloadedFlavorTextOrDefault($"{LWMUtils.GetNPCTypeNameOrIDName(info.NPC.type)}.Biome_{biome.NameKey}"),
                moodOffset
            );
    }
}