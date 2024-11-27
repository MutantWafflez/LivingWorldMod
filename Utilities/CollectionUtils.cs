using System.Collections.Generic;
using LivingWorldMod.DataStructures.Structs;
using Terraria.Utilities;

namespace LivingWorldMod.Utilities;

// Utilities class that holds methods for Collections.
public static partial class LWMUtils {
    /// <summary>
    ///     Adds to the <see cref="WeightedRandom{T}" /> only if the given condition returns true.
    /// </summary>
    /// <param name="list"> List to add to. </param>
    /// <param name="obj"> Object to add to the list. </param>
    /// <param name="condition">
    ///     Condition that determines whether or not to add the given object to the list.
    /// </param>
    /// <param name="weight"> The potential weight of the object, if required. </param>
    public static void AddConditionally<T>(this ICollection<T> list, T obj, bool condition) {
        if (condition) {
            list.Add(obj);
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
    public static T GetValueAsNDimensionalArray<T>(this T[] array, params ArrayDimensionData[] dimensionDatas) {
        int endPosition = 0;
        for (int i = 0; i < dimensionDatas.Length - 1; i++) {
            endPosition += dimensionDatas[i].dimensionPosition * dimensionDatas[i + 1].dimensionSize;
        }

        return array[endPosition + dimensionDatas[^1].dimensionPosition];
    }
}