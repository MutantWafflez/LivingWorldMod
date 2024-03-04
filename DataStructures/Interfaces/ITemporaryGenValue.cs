using LivingWorldMod.DataStructures.Structs;

namespace LivingWorldMod.DataStructures.Interfaces;

/// <summary>
/// Interface that purely exists in order to create collections of
/// <seealso cref="TemporaryGenValue{T}"/> where the type of T differs.
/// </summary>
public interface ITemporaryGenValue {
    /// <summary>
    /// The "Name" or purpose for why this object exists, for searching in collections.
    /// </summary>
    public string Name {
        get;
    }
}