using LivingWorldMod.DataStructures.Interfaces;

namespace LivingWorldMod.DataStructures.Structs;

/// <summary>
///     Struct that acts as a singular generic value type object to be stored temporarily
///     during the WorldGen process that will be discard afterwards.
///     Implements <seealso cref="ITemporaryGenValue" /> in order to create collections
///     of these objects even if the type of <seealso cref="T" /> differs between them.
/// </summary>
public struct TemporaryGenValue<T> (T value, string name) : ITemporaryGenValue {
    public T value = value;

    public string Name {
        get;
    } = name;
}