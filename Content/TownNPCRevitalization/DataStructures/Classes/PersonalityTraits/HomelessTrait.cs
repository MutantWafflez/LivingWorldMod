using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

using Terraria.GameContent;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Mimics the happiness penalty from the vanilla happiness system due to the NPC being homeless, but uses the raw numeric system of the Town NPC Revitalization.
/// </summary>
public class HomelessTrait : IPersonalityTrait {
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        if (!info.NPC.homeless)  {
            return;
        }

        info.NPC.GetGlobalNPC<TownNPCMoodModule>().AddModifier("TownNPCMoodDescription.Homeless".Localized(), Language.GetText($"{TownNPCMoodModule.GetFlavorTextKeyPrefix(info.NPC)}.NoHome"), -100);
    }
}