using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.PersonalityTraits;

/// <summary>
///     Trait that mimics the "FarFromHome" functionality from the vanilla NPC happiness system, that instead uses he numeric mood system of the Town NPC Revitalization.
/// </summary>
public class HomeProximityTrait : IPersonalityTrait {
    public void ApplyTrait(PersonalityHelperInfo info, ShopHelper shopHelperInstance) {
        NPC npc = info.NPC;
        if (Vector2.Distance(new Vector2 (npc.homeTileX, npc.homeTileY), npc.Center.ToTileCoordinates().ToVector2()) > 120f) {
            npc.GetGlobalNPC<TownGlobalNPC>()
                .MoodModule.AddModifier("TownNPCMoodDescription.FarFromHome".Localized(), Language.GetText($"{TownNPCMoodModule.GetFlavorTextKeyPrefix(npc)}.FarFromHome"), -20, 0);
        }
    }
}