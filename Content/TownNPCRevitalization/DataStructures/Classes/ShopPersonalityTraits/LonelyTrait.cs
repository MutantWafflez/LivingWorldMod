using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
///     Trait that mimics the "loneliness" feature that, in vanilla, the Princess can feel when there aren't enough other Town NPCs nearby. This uses raw numbers for the Town NPC Revitalization
///     instead of an <see cref="AffectionLevel" /> object.
/// </summary>
public class LonelyTrait : IShopPersonalityTrait {
    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        TownNPCMoodModule moodModule = info.npc.GetGlobalNPC<TownGlobalNPC>().MoodModule;
        string flavorTextKeyPrefix = info.npc.ModNPC is not null ? info.npc.ModNPC.GetLocalizationKey("TownNPCMood") : $"TownNPCMood_{NPCID.Search.GetName(info.npc.type)}";

        if (HappinessPatches.NPCCountWithinHouse < 2 && HappinessPatches.NPCCountWithinVillage < 2) {
            moodModule.AddModifier("TownNPCMoodDescription.Lonely".Localized(), Language.GetText($"{flavorTextKeyPrefix}.HateLonely"), -20, 0);
        }
    }
}