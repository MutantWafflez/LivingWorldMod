using System.Collections.Generic;
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
}