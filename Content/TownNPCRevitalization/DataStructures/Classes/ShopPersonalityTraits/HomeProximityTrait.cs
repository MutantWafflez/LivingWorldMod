using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.ShopPersonalityTraits;

/// <summary>
///     Trait that mimics the "FarFromHome" functionality from the vanilla NPC happiness system, that instead uses he numeric mood system of the Town NPC Revitalization.
/// </summary>
public class HomeProximityTrait : IShopPersonalityTrait {
    public void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
        NPC npc = info.npc;
        if (Vector2.Distance(new Vector2 (npc.homeTileX, npc.homeTileY), npc.Center.ToTileCoordinates().ToVector2()) > 120f) {
            npc.GetGlobalNPC<TownGlobalNPC>()
                .MoodModule.AddModifier("TownNPCMoodDescription.FarFromHome".Localized(), Language.GetText($"{TownNPCMoodModule.GetFlavorTextKeyPrefix(info.npc)}.FarFromHome"), -20, 0);
        }
    }
}