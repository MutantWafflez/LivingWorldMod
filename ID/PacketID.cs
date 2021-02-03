using LivingWorldMod.Utilities.NetPackets;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.ID
{
    /// <summary>
    /// Packet types. Each one is more or less expected to have an associated subclass of LWMPacket.
    /// IMPORTANT: Remember to add new packet types to RegisterAllHandlers()! 
    /// </summary>
    public enum PacketID: byte
    {
        PlayerData, // client -> server, syncs GUID
        VillagerData,
        LimitedPurchase, // client -> server, notification of purchases
        PacketCount
    }

    public abstract class LWMPacket
    {
        public readonly PacketID packetType;
        
        protected LWMPacket(PacketID id)
        {
            packetType = id;
        }
        
        /// <summary>
        /// Write packet data to a writer, usually going to be a ModPacket.
        /// This is intended to be called in Send, so it is protected to prevent confusion about
        /// which method to use.
        /// </summary>
        /// <param name="writer">The binary writer</param>
        protected abstract void Write(BinaryWriter writer);
        
        /// <summary>
        /// Reads the data written in Write into the fields of this object.
        /// Both this and Handle are expected to be called from LWMPacket.TryHandlePackets,
        /// so this is protected to avoid confusion.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        protected abstract void Read(BinaryReader reader);

        /// <summary>
        /// Writes this packet to a ModPacket binary writer and sends it out.
        /// </summary>
        /// <param name="modInstance"></param>
        /// <param name="toWho"></param>
        /// <param name="fromWho"></param>
        public void Send(Mod modInstance, int toWho, int fromWho)
        {
            ModPacket packet = modInstance.GetPacket();
            packet.Write((byte) packetType);
            Write(packet);
            packet.Send(toWho, fromWho);
        }

        public void SendToServer(Mod modInstance)
        {
            Send(modInstance, -1, Main.myPlayer);
        }

        public void SendToAll(Mod modInstance)
        {
            Send(modInstance, -1, -1);
        }
        
        protected abstract void Handle(int sentFromPlayer);
        
        
        private static readonly Action<BinaryReader, int>[] _packetHandlers = new Action<BinaryReader, int>[(int) PacketID.PacketCount];
        // private static readonly Dictionary<Type, PacketID> _packetTypeToID = new Dictionary<Type, PacketID>();
        
        private static void RegisterHandler<T>() where T : LWMPacket, new()
        {
            T dummy = new T();
            _packetHandlers[(byte) dummy.packetType] = (reader, sender) =>
            {
                T packet = new T();
                packet.Read(reader);
                packet.Handle(sender);
            };
        }

        public static bool TryHandlePacket(PacketID packetType, BinaryReader reader, int sentFromPlayer)
        {
            Action<BinaryReader, int> handler = _packetHandlers[(byte) packetType];
            handler?.Invoke(reader, sentFromPlayer);
            return handler != null;
        }

        /// <summary>
        /// This method registers all the different classes of packet to their respective PacketIDs, so that
        /// Mod.HandlePacket can use array lookup to find the right handler.
        /// </summary>
        internal static void RegisterAllHandlers()
        {
            RegisterHandler<PlayerData>();
            RegisterHandler<VillagerData>();
            RegisterHandler<LimitedPurchase>();
        }
    }
}