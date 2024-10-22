using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
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
        TownGlobalNPC globalNPC = info.NPC.GetGlobalNPC<TownGlobalNPC>();
        float awakeValue = globalNPC.SleepModule.awakeTicks;

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

        globalNPC.MoodModule.AddModifier(
            new SubstitutableLocalizedText($"TownNPCMoodDescription.{sleepQualityKey}".Localized()),
            new SubstitutableLocalizedText($"TownNPCMoodFlavorText.{LWMUtils.GetNPCTypeNameOrIDName(info.NPC.type)}.{sleepQualityKey}".Localized()),
            moodOffset
        );
    }
}