using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.DataStructures.Records;

using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Personality trait that grants mood shifts based on the current state of the NPC's sleep.
/// </summary>
public class SleepTrait : IPersonalityTrait {
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        NPC npc = info.NPC;
        TownNPCSleepModule sleepModule = npc.GetGlobalNPC<TownNPCSleepModule>();

        SleepThresholds thresholds = TownNPCSleepModule.GetSleepThresholdsOrDefault(npc.type);
        float awakeValue = sleepModule.awakeTicks;

        string sleepQualityKey = "SleepDeprived";
        int moodOffset = -20;
        if (awakeValue <= thresholds.BestRestLimit) {
            sleepQualityKey = "VeryWellRested";
            moodOffset = 12;
        }
        else if (awakeValue <= thresholds.WellRestedLimit) {
            sleepQualityKey = "WellRested";
            moodOffset = 8;
        }
        else if (awakeValue <= thresholds.TiredLimit) {
            sleepQualityKey = "Tired";
            moodOffset = -8;
        }

        string npcTypeName = LWMUtils.GetNPCTypeNameOrIDName(info.NPC.type);
        string flavorTextKey = $"TownNPCMoodFlavorText.{npcTypeName}.{sleepQualityKey}";
        npc.GetGlobalNPC<TownNPCMoodModule>()
            .AddModifier(
                new DynamicLocalizedText($"TownNPCMoodDescription.{sleepQualityKey}".Localized()),
                new DynamicLocalizedText(flavorTextKey.Localized(), FallbackText: flavorTextKey.Replace(npcTypeName, "Default").Localized()),
                moodOffset
            );
    }
}