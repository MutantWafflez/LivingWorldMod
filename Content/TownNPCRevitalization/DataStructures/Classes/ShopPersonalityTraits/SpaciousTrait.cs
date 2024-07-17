using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
///     Imitates the "spacious" living from vanilla happiness, where fewer NPCs near gives a boost to mood. Uses raw numeric system of the Town NPC Revitalization instead of a
///     <see cref="AffectionLevel" /> object.
/// </summary>
public class SpaciousTrait : IShopPersonalityTrait {
    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        TownNPCMoodModule moodModule = info.npc.GetGlobalNPC<TownGlobalNPC>().MoodModule;
        string flavorTextKeyPrefix = TownNPCMoodModule.GetFlavorTextKeyPrefix(info.npc);

        if (HappinessPatches.NPCCountWithinHouse <= 2 && HappinessPatches.NPCCountWithinVillage < 4) {
            moodModule.AddModifier("TownNPCMoodDescription.NotCrowded".Localized(), Language.GetText($"{flavorTextKeyPrefix}.LoveSpace"), 15, 0);
        }
    }
}