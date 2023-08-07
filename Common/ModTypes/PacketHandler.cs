using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.ModTypes {
    /// <summary>
    /// ModType that serves as a handler for specific types of packets, rather than having
    /// to use massive switch cases for each type of packet in Mod.HandlePacket.
    /// </summary>
    public abstract class PacketHandler : ModType {
        private static List<PacketHandler> allPacketHandlers;

        /// <summary>
        /// The numerical type of this Handler. Auto-assigned at initialization.
        /// </summary>
        public byte HandlerType {
            get;
            private set;
        }

        /// <summary>
        /// The count of all PacketHandler extensions/objects that exist.
        /// </summary>
        public static byte HandlerTypeCount {
            get;
            private set;
        }

        public sealed override void SetupContent() => SetStaticDefaults();

        public sealed override void SetStaticDefaults() {
            allPacketHandlers = ModContent.GetContent<PacketHandler>().ToList();
        }

        /// <summary>
        /// Returns the PacketHandler of the specified type. Returns null if the type doesn't exist.
        /// </summary>
        /// <param name="type"> The type of the PacketHandler being searched for. </param>
        public static PacketHandler GetHandler(byte type) => allPacketHandlers.FirstOrDefault(handler => handler.HandlerType == type);

        /// <summary>
        /// Equivalent to <seealso cref="Mod.HandlePacket"/>, but for this packet type specifically.
        /// </summary>
        /// <param name="reader"> The stream holding the packet's data. </param>
        /// <param name="fromWhomst"> If sent from a client, the client ID of said packet. </param>
        public abstract void HandlePacket(BinaryReader reader, int fromWhomst);

        /// <summary>
        /// Retrieves & returns a freshly created ModPacket of this Handler's type. Starts
        /// with the handler's type & packet type (in relation to this handler) on the
        /// stream, in that order. This method cannot be overriden as the packet
        /// containing HandlerType is required for functionality, but <seealso cref="HijackGetPacket"/>
        /// may be overriden.
        /// </summary>
        /// <param name="packetType"> The specific packet type in relation to this handler. </param>
        public ModPacket GetPacket(byte packetType = 0) {
            ModPacket packet = Mod.GetPacket();
            packet.Write(HandlerType);
            HijackGetPacket(ref packet, packetType);

            return packet;
        }

        /// <summary>
        /// Method called after <seealso cref="GetPacket"/> writes the HandlerType to the newly generated
        /// packet. By default, writes the packetType to the packet.
        /// </summary>
        /// <param name="packet"> The packet about to be returned in GetPacket. </param>
        /// <param name="packetType"> The type of packet requested when GetPacket was called. </param>
        protected virtual void HijackGetPacket(ref ModPacket packet, byte packetType) {
            packet.Write(packetType);
        }

        protected sealed override void Register() {
            ModTypeLookup<PacketHandler>.Register(this);
            HandlerType = HandlerTypeCount;
            HandlerTypeCount++;
        }
    }
}