using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Numeric version of <see cref="NPCPreferenceTrait" /> that uses raw numbers instead of <see cref="AffectionLevel" /> values.
/// </summary>
public class NumericNPCPreferenceTrait(int moodOffset, int npcType) : IPersonalityTrait {
    public override string ToString() => $"NPC: {LWMUtils.GetNPCTypeNameOrIDName(npcType)}, Offset: {moodOffset}";

    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        if (!info.NearbyNPCsByType[npcType]) {
            return;
        }

        info.NPC.GetGlobalNPC<TownGlobalNPC>()
            .MoodModule.AddModifier(
                new SubstitutableLocalizedText(
                    "TownNPCMoodDescription.NeighborNPC".Localized(),
                    new { NPCTypeName = LWMUtils.GetFirstNPC(npc => npc.type == npcType)?.TypeName ?? "Error"  }
                ),
                new SubstitutableLocalizedText(
                    TownNPCDataSystem.GetAutoloadedFlavorTextOrDefault($"{LWMUtils.GetNPCTypeNameOrIDName(info.NPC.type)}.NPC_{LWMUtils.GetNPCTypeNameOrIDName(npcType)}"),
                    new { NPCName = NPC.GetFirstNPCNameOrNull(npcType) }
                ),
                moodOffset
            );
    }
}