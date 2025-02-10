using System;

namespace LivingWorldMod.DataStructures.Structs;

/// <summary>
///     Wrapper struct that allows for interpreting 1D arrays in different dimensions without actually copying or restructuring the internal array itself.
/// </summary>
public readonly ref struct ArrayInterpreter<T>  {
    private readonly ReadOnlySpan<T> _array;
    private readonly int[] _arrayDimensionSizes;

    public ArrayInterpreter(T[] array, params int[] dimensionSizes) {
        if (dimensionSizes.Length <= 0) {
            throw new ArgumentException("Must have minimum of 1 dimension argument.");
        }

        _array = array;
        _arrayDimensionSizes = dimensionSizes;
    }

    /// <summary>
    ///     Returns the value within the array passed within the constructor, treating the array as if it was an array of some n-dimensional array. The dimension of the array is determined by the amount of
    ///     dimension parameters passed in the constructor. For example, passing in 3 dimension parameters will treat the array as if it was 3 dimensional.
    /// </summary>
    public  T GetAtPosition(params int[] dimensionPositions) {
        if (_arrayDimensionSizes.Length != dimensionPositions.Length) {
            throw new ArgumentOutOfRangeException(nameof(dimensionPositions), $"Expected {_arrayDimensionSizes.Length} parameters for interpretation, got {dimensionPositions.Length}");
        }

        int endPosition = 0;
        for (int i = 0; i < _arrayDimensionSizes.Length - 1; i++) {
            endPosition += dimensionPositions[i] * _arrayDimensionSizes[i + 1];
        }

        return _array[endPosition + dimensionPositions[^1]];
    }
}