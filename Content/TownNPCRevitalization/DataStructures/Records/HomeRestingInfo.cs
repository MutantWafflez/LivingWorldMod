using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Enums;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

/// <summary>
///     Data Structure that holds resting information for a particular home/NPC, used for teleportation/sleep locations.
/// </summary>
public record struct HomeRestingInfo(Point PathfindEndPos, Point ActualRestTilePos, NPCRestType RestType, BedInfo? BedInfo = null);