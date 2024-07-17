using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
///     Re-creation of the "crowded-ness" functionality of Town NPC Happiness from vanilla, but for usage with the raw number system of the Town NPC Revitalization.
/// </summary>
public class CrowdingTrait : IShopPersonalityTrait {
    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        TownNPCMoodModule moodModule = info.npc.GetGlobalNPC<TownGlobalNPC>().MoodModule;
        string flavorTextKeyPrefix = TownNPCMoodModule.GetFlavorTextKeyPrefix(info.npc);

        switch (HappinessPatches.NPCCountWithinHouse) {
            case > 3 and > 6 :
                moodModule.AddModifier("TownNPCMoodDescription.VeryCrowded".Localized(), Language.GetText($"{flavorTextKeyPrefix}.HateCrowded"), -30, 0);
                break;
            case > 3:
                moodModule.AddModifier("TownNPCMoodDescription.Crowded".Localized(), Language.GetText($"{flavorTextKeyPrefix}.DislikeCrowded"), -15, 0);
                break;
        }
    }
}