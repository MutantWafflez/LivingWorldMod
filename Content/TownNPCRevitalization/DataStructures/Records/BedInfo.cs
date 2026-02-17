using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

/// <summary>
///     Data Structure that holds information related to bed tiles, Vanilla or modded.
/// </summary>
public readonly record struct BedInfo(int TileID, int TileStyle, int SleepDirection, Vector2 AnchorPosition, Vector2 SleepDrawOffset);