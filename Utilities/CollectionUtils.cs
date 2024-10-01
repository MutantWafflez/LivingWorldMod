using System;
using LivingWorldMod.DataStructures.Structs;
using Terraria.Utilities;

namespace LivingWorldMod.Utilities;

// Utilities class that holds methods for Collections.
public static partial class LWMUtils {
    /// <summary>
    ///     Adds all of the objects and their weights within another list into this list.
    /// </summary>
    /// <param name="originalList"> List that will be added to. </param>
    /// <param name="list">
    ///     List that will have its objects and theirs weights added to the other list.
    /// </param>
    public static void AddRange<T>(this WeightedRandom<T> originalList, WeightedRandom<T> list) {
        foreach ((T obj, double weight) in list.elements) {
            originalList.Add(obj, weight);
        }
    }

    /// <summary>
    ///     Adds to the <see cref="WeightedRandom{T}" /> only if the given condition returns true.
    /// </summary>
    /// <param name="list"> List to add to. </param>
    /// <param name="obj"> Object to add to the list. </param>
    /// <param name="condition">
    ///     Condition that determines whether or not to add the given object to the list.
    /// </param>
    /// <param name="weight"> The potential weight of the object, if required. </param>
    public static void AddConditionally<T>(this WeightedRandom<T> list, T obj, bool condition, double weight = 1) {
        if (condition) {
            list.Add(obj, weight);
        }
    }

    /// <summary>
    ///     Returns the value in the passed in array, treating the array as if it was an array of some n-dimensional array. The dimension of the array is determined by the amount of
    ///     <see cref="ArrayDimensionData" />
    ///     parameters that are passed in. For example, passing in 3 dimension parameters will treat the array as if it was 3 dimensional.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="dimensionDatas"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetValueAsArrayOfVariableDimension<T>(this T[] array, params ArrayDimensionData[] dimensionDatas) {
        int endPosition = 0;
        for (int i = 0; i < dimensionDatas.Length - 1; i++) {
            endPosition += dimensionDatas[i].dimensionPosition * dimensionDatas[i + 1].dimensionSize;
        }

        return array[endPosition + dimensionDatas[^1].dimensionPosition];
    }
}