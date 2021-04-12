using System.IO;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Interfaces;

namespace LivingWorldMod.Custom.Classes.Packets
{
    /// <summary>
    /// This class represents a superclass for packets dedicated to identifying objects in order to
    /// relay serialization to the object itself. As such, it's primarily used to update data for
    /// specific objects.
    /// </summary>
    /// <typeparam name="T"> The type of object, which extends BinarySerializable </typeparam>
    public abstract class ObjectData<T> : LWMPacket where T : IBinarySerializable
    {
        public byte syncMode;

        private T serialObj;

        public ObjectData(PacketID id, T serialObj) : base(id)
        {
            this.serialObj = serialObj;
        }

        protected abstract void WriteId(BinaryWriter writer, T obj);

        protected abstract T ReadId(BinaryReader reader);

        protected override void Write(BinaryWriter writer)
        {
            writer.Write(syncMode);
            WriteId(writer, serialObj);
            serialObj.Write(writer, syncMode);
        }

        protected override void Read(BinaryReader reader)
        {
            syncMode = reader.ReadByte();
            T obj = ReadId(reader);
            obj.Read(reader, syncMode);
        }

        protected override void Handle(int sentFromPlayer) { }
    }
}