using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Personality trait that grants mood shifts based on the current state of the NPC's sleep.
/// </summary>
/// <param name="tiredLimit">Max amount of awake ticks to gain the "tired" mood loss. Defaults to 17 of in-game time.</param>
/// <param name="wellRestedLimit">Max amount of awake ticks to gain the "well rested" mood bonus. Defaults to 13 hours of in-game time.</param>
/// <param name="bestRestLimit">Max amount of awake ticks to gain the "very well rested" mood bonus. Defaults to 5 hours of in-game time.</param>
public class SleepTrait(int tiredLimit = LWMUtils.InGameHour * 17, int wellRestedLimit = LWMUtils.InGameHour * 13, int bestRestLimit = LWMUtils.InGameHour * 5) : IPersonalityTrait {
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        NPC npc = info.NPC;
        float awakeValue = npc.GetGlobalNPC<TownNPCSleepModule>().awakeTicks;

        string sleepQualityKey = "SleepDeprived";
        int moodOffset = -20;
        if (awakeValue <= bestRestLimit) {
            sleepQualityKey = "VeryWellRested";
            moodOffset = 12;
        }
        else if (awakeValue <= wellRestedLimit) {
            sleepQualityKey = "WellRested";
            moodOffset = 8;
        }
        else if (awakeValue <= tiredLimit) {
            sleepQualityKey = "Tired";
            moodOffset = -8;
        }

        string npcTypeName = LWMUtils.GetNPCTypeNameOrIDName(info.NPC.type);
        string flavorTextKey = $"TownNPCMoodFlavorText.{npcTypeName}.{sleepQualityKey}";
        npc.GetGlobalNPC<TownNPCMoodModule>()
            .AddModifier(
                new DynamicLocalizedText($"TownNPCMoodDescription.{sleepQualityKey}".Localized()),
                new DynamicLocalizedText(flavorTextKey.Localized(), fallbackText: flavorTextKey.Replace(npcTypeName, "Default").Localized()),
                moodOffset
            );
    }
}