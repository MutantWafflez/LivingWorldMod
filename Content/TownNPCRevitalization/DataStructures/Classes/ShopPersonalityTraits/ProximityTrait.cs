using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
///     Re-creation of the "crowded-ness" functionality of Town NPC Happiness from vanilla, but for usage with the raw number system of the Town NPC Revitalization.
/// </summary>
public class ProximityTrait : IShopPersonalityTrait {
    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        // The two out parameters of this method aren't stored in the HelperInfo, so we must call this method again - perhaps look into only having to call it once?
        shopHelperInstance.GetNearbyResidentNPCs(info.npc, out int npcsWithinHouse, out int npcsWithinVillage);

        TownNPCMoodModule moodModule = info.npc.GetGlobalNPC<TownGlobalNPC>().MoodModule;
        if (npcsWithinHouse > 3) { }
    }
}