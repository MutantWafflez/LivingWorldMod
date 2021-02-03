using System.IO;

namespace LivingWorldMod.Utilities
{
    /// <summary>
    /// A basic tagging interface for classes that handle their own network serialization.
    /// syncMode is an optional parameter which may be utilized to differentiate different sets of data
    /// for the same object type; for example, if the player had multiple sets of custom data, they
    /// could create a private enum and use it to separate the different sets of data to sync.
    /// </summary>
    public interface BinarySerializable
    {
        void Write(BinaryWriter writer, byte syncMode = default);
        
        void Read(BinaryReader reader, byte syncMode = default);
    }
}