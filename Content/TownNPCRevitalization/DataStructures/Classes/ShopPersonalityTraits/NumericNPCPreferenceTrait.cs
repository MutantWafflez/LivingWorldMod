using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

public class NumericNPCPreferenceTrait(int moodOffset, int npcType) : IShopPersonalityTrait {
    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        if (!info.nearbyNPCsByType[npcType]) {
            return;
        }

        string otherNPCTypeName = LWMUtils.GetTypeNameOrIDName(npcType);
        info.npc.GetGlobalNPC<TownGlobalNPC>()
            .MoodModule.AddModifier(
                new SubstitutableLocalizedText("TownNPCMoodDescription.NeighborNPC".Localized(), new { NPCTypeName = otherNPCTypeName }),
                new SubstitutableLocalizedText($"TownNPCMood.{LWMUtils.GetTypeNameOrIDName(info.npc.type)}.NPC_{otherNPCTypeName}".Localized(), new { NPCName = NPC.GetFirstNPCNameOrNull(npcType) }),
                moodOffset,
                0
            );
    }
}