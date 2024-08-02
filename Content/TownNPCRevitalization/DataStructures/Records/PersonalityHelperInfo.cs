using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

/// <summary>
///     Extremely similar record to the <see cref="Terraria.GameContent.Personalities.HelperInfo" /> struct that has more stored information for usage with the <see cref="IPersonalityTrait" /> interface
///     and its children.
/// </summary>
public record PersonalityHelperInfo(Player Player, NPC NPC, List<NPC> NearbyNPCs, bool[] NearbyNPCsByType, int NearbyHouseNPCCount, int NearbyVillageNPCCount);