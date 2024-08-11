using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Re-creation of the "crowded-ness" functionality of Town NPC Happiness from vanilla, but for usage with the raw number system of the Town NPC Revitalization.
/// </summary>
public class CrowdingTrait : IPersonalityTrait {
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        TownNPCMoodModule moodModule = info.NPC.GetGlobalNPC<TownGlobalNPC>().MoodModule;
        string flavorTextKeyPrefix = TownNPCMoodModule.GetFlavorTextKeyPrefix(info.NPC);

        switch (info.NearbyHouseNPCCount) {
            case > 3 and > 6 :
                moodModule.AddModifier("TownNPCMoodDescription.VeryCrowded".Localized(), Language.GetText($"{flavorTextKeyPrefix}.HateCrowded"), -30);
                break;
            case > 3:
                moodModule.AddModifier("TownNPCMoodDescription.Crowded".Localized(), Language.GetText($"{flavorTextKeyPrefix}.DislikeCrowded"), -15);
                break;
        }
    }
}