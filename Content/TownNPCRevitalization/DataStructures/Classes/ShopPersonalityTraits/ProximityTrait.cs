using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
///     Re-creation of the "crowded-ness" functionality of Town NPC Happiness from vanilla, but for usage with the raw number system of the Town NPC Revitalization.
/// </summary>
public class ProximityTrait : IShopPersonalityTrait {
    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        // The two out parameters of this method aren't stored in the HelperInfo, so we must call this method again - perhaps look into only having to call it once?
        shopHelperInstance.GetNearbyResidentNPCs(info.npc, out int npcsWithinHouse, out int npcsWithinVillage);

        TownNPCMoodModule moodModule = info.npc.GetGlobalNPC<TownGlobalNPC>().MoodModule;
        string flavorTextKeyPrefix = info.npc.ModNPC is not null ? info.npc.ModNPC.GetLocalizationKey("TownNPCMood") : $"TownNPCMood_{NPCID.Search.GetName(info.npc.type)}";
        switch (npcsWithinHouse) {
            case > 3 and > 6 :
                moodModule.AddModifier("TownNPCMoodDescription.VeryCrowded".Localized(), Language.GetText($"{flavorTextKeyPrefix}.HateCrowded"), -30, 0);
                break;
            case > 3:
                moodModule.AddModifier("TownNPCMoodDescription.Crowded".Localized(), Language.GetText($"{flavorTextKeyPrefix}.DislikeCrowded"), -15, 0);
                break;
            case <= 2 when npcsWithinVillage < 4:
                moodModule.AddModifier("TownNPCMoodDescription.NotCrowded".Localized(), Language.GetText($"{flavorTextKeyPrefix}.LoveSpace"), 15, 0);
                break;
        }
    }
}