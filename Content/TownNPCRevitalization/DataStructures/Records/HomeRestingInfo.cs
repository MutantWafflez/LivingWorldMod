using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Enums;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

public record struct HomeRestingInfo(Point PathfindEndPos, Point ActualRestTilePos, NPCRestType RestType);