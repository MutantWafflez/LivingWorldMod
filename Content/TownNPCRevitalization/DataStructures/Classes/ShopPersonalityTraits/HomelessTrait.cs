using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
///     Mimics the happiness penalty from the vanilla happiness system due to the NPC being homeless, but uses the raw numeric system of the Town NPC Revitalization.
/// </summary>
public class HomelessTrait : IShopPersonalityTrait {
    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        if (!info.npc.homeless)  {
            return;
        }

        info.npc.GetGlobalNPC<TownGlobalNPC>()
            .MoodModule.AddModifier("TownNPCMoodDescription.Homeless".Localized(), Language.GetText($"{TownNPCMoodModule.GetFlavorTextKeyPrefix(info.npc)}.NoHome"), -100, 0);
    }
}