namespace LivingWorldMod.DataStructures.Structs;

/// <summary>
///     Struct that represents a dimension of an array, indicating the size of and position in said dimension.
/// </summary>
public readonly struct ArrayDimensionData(int dimensionPosition, int dimensionSize) {
    public readonly int dimensionPosition = dimensionPosition;
    public readonly int dimensionSize = dimensionSize;
}