using System.IO;

namespace LivingWorldMod.Utilities
{
    public interface BinarySerializable
    {
        void Write(BinaryWriter writer);
        
        void Read(BinaryReader reader);
    }
}