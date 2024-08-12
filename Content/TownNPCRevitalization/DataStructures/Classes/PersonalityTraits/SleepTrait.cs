using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <param name="badSleepThreshold">Minimum ticks required for the second lowest mood loss from sleep. Defaults to 3 hours of in-game time.</param>
/// <param name="decentSleepThreshold">Minimum ticks required for the second highest mood boost from sleep. Defaults to 6 hours of in-game time.</param>
/// <param name="bestSleepThreshold">Minimum ticks required for highest mood boost from sleeping. Defaults to 8 hours of in-game time.</param>
public class SleepTrait(int badSleepThreshold = LWMUtils.InGameHour * 3, int decentSleepThreshold = LWMUtils.InGameHour * 6, int bestSleepThreshold = LWMUtils.InGameHour * 8) : IPersonalityTrait {
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        TownGlobalNPC globalNPC = info.NPC.GetGlobalNPC<TownGlobalNPC>();
        float sleepValue = globalNPC.SleepModule.sleepValue;

        string sleepQualityKey = "SleepDeprived";
        int moodOffset = -20;
        if (sleepValue >= bestSleepThreshold) {
            sleepQualityKey = "VeryWellRested";
            moodOffset = 12;
        }
        else if (sleepValue >= decentSleepThreshold) {
            sleepQualityKey = "WellRested";
            moodOffset = 8;
        }
        else if (sleepValue >= badSleepThreshold) {
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