using LivingWorldMod.Custom.Interfaces;

namespace LivingWorldMod.Custom.Structs {
    /// <summary>
    /// Struct that acts as a singular generic value type object to be stored temporarily.
    /// Implements <seealso cref="ITemporaryValue"/> in order to create collections
    /// of these objects even if the type of <seealso cref="T"/> differs between them.
    /// </summary>
    public struct TemporaryValue<T> : ITemporaryValue {
        public T value;

        public TemporaryValue(T value, string name) {
            this.value = value;
            Name = name;
        }

        public string Name {
            get;
        }
    }
}