using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Trait that mimics the "loneliness" feature that, in vanilla, the Princess can feel when there aren't enough other Town NPCs nearby. This uses raw numbers for the Town NPC Revitalization
///     instead of an <see cref="AffectionLevel" /> object.
/// </summary>
public class LonelyTrait : IPersonalityTrait {
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        TownNPCMoodModule moodModule = info.NPC.GetGlobalNPC<TownNPCMoodModule>();
        if (info.NearbyHouseNPCCount < 2 && info.NearbyVillageNPCCount < 2) {
            moodModule.AddModifier("TownNPCMoodDescription.Lonely".Localized(), Language.GetText($"{TownNPCMoodModule.GetFlavorTextKeyPrefix(info.NPC)}.HateLonely"), -20);
        }
    }
}