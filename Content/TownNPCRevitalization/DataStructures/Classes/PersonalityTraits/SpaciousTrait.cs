using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Imitates the "spacious" living from vanilla happiness, where fewer NPCs near gives a boost to mood. Uses raw numeric system of the Town NPC Revitalization instead of a
///     <see cref="AffectionLevel" /> object.
/// </summary>
public class SpaciousTrait : IPersonalityTrait {
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        TownNPCMoodModule moodModule = info.NPC.GetGlobalNPC<TownNPCMoodModule>();
        string flavorTextKeyPrefix = TownNPCMoodModule.GetFlavorTextKeyPrefix(info.NPC);

        if (info.NearbyHouseNPCCount <= 2 && info.NearbyVillageNPCCount < 4) {
            moodModule.AddModifier("TownNPCMoodDescription.NotCrowded".Localized(), Language.GetText($"{flavorTextKeyPrefix}.LoveSpace"), 15);
        }
    }
}