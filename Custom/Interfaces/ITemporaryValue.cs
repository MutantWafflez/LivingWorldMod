using LivingWorldMod.Custom.Structs;

namespace LivingWorldMod.Custom.Interfaces {
    /// <summary>
    /// Interface that purely exists in order to create collections of
    /// <seealso cref="TemporaryValue{T}"/> where the type of T differs.
    /// </summary>
    public interface ITemporaryValue {
        /// <summary>
        /// The "Name" or purpose for why this object exists, for searching in collections.
        /// </summary>
        public string Name {
            get;
        }
    }
}