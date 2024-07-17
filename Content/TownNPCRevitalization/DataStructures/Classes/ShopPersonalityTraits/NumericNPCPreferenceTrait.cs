using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

public class NumericNPCPreferenceTrait(int moodOffset, int npcType) : IShopPersonalityTrait {
    public override string ToString() => $"NPC: {LWMUtils.GetNPCTypeNameOrIDName(npcType)}, Offset: {moodOffset}";

    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        if (!info.nearbyNPCsByType[npcType]) {
            return;
        }

        info.npc.GetGlobalNPC<TownGlobalNPC>()
            .MoodModule.AddModifier(
                new SubstitutableLocalizedText(
                    "TownNPCMoodDescription.NeighborNPC".Localized(),
                    new { NPCTypeName = LWMUtils.GetFirstNPC(npc => npc.type == npcType)?.TypeName ?? "Error"  }
                ),
                new SubstitutableLocalizedText(
                    $"TownNPCMood.{LWMUtils.GetNPCTypeNameOrIDName(info.npc.type)}.NPC_{LWMUtils.GetNPCTypeNameOrIDName(npcType)}".Localized(),
                    new { NPCName = NPC.GetFirstNPCNameOrNull(npcType) }
                ),
                moodOffset,
                0
            );
    }
}