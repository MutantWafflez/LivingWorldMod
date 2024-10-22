using Terraria.DataStructures;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;

/// <summary>
///     Wrapper struct for a <see cref="DrawData" /> struct that tacks on an timer for drawing over multiple updates.
/// </summary>
public struct TownNPCDrawData(DrawData drawData, int drawDuration = 0) {
    public DrawData drawData = drawData;
    public int drawDuration = drawDuration;

    public static implicit operator TownNPCDrawData(DrawData data) => new (data);
}