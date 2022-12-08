using System.IO;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.PacketHandlers {
    public class AccessoryPacketHandler : PacketHandler {
        /// <summary>
        /// Sent from client to client in order to ensure that <seealso cref="AccessoryPlayer"/>
        /// is synced properly.
        /// </summary>
        public const byte SyncAccessoryPlayer = 0;

        /// <summary>
        /// Sent from a client to the server and then to every other client when the initial
        /// client has an accessory type disabled.
        /// </summary>
        public const byte AddDisabledAccessoryType = 1;

        /// <summary>
        /// Sent from a client to the server and then to every other client when the initial
        /// client has an accessory slot disabled.
        /// </summary>
        public const byte AddDisabledAccessorySlot = 2;

        public override void HandlePacket(BinaryReader reader, int fromWhomst) {
            byte packetType = reader.ReadByte();

            switch (packetType) {
                case SyncAccessoryPlayer:
                    int playerIndex = reader.ReadInt32();
                    AccessoryPlayer accessoryPlayer = Main.player[playerIndex].GetModPlayer<AccessoryPlayer>();

                    //Clear both lists just in case
                    accessoryPlayer.disabledAccessoryTypes.Clear();
                    accessoryPlayer.disabledAccessorySlots.Clear();

                    int disabledTypeCount = reader.ReadInt32();
                    for (int i = 0; i < disabledTypeCount; i++) {
                        accessoryPlayer.disabledAccessoryTypes.Add(ReadDisabledAccessoryInstance(reader));
                    }

                    int disabledSlotCount = reader.ReadInt32();
                    for (int i = 0; i < disabledSlotCount; i++) {
                        accessoryPlayer.disabledAccessorySlots.Add(ReadDisabledAccessoryInstance(reader));
                    }
                    break;
                case AddDisabledAccessoryType:
                    AddDisabledAccessoryTypeOrSlot(reader, fromWhomst, true);
                    break;
                case AddDisabledAccessorySlot:
                    AddDisabledAccessoryTypeOrSlot(reader, fromWhomst, false);
                    break;
            }
        }

        /// <summary>
        /// Writes a <seealso cref="AccessoryPlayer.DisabledAccessoryInstance"/> to a <seealso cref="BinaryWriter"/>.
        /// </summary>
        public static void WriteDisabledAccessoryInstance(AccessoryPlayer.DisabledAccessoryInstance instance, BinaryWriter writer) {
            writer.Write(instance.durationTimer);
            writer.Write(instance.persistent);
            writer.Write(instance.typeOrSlot);
        }

        /// <summary>
        /// Creates a <seealso cref="AccessoryPlayer.DisabledAccessoryInstance"/> from data in a <seealso cref="BinaryReader"/>.
        /// </summary>
        public static AccessoryPlayer.DisabledAccessoryInstance ReadDisabledAccessoryInstance(BinaryReader reader) => new(reader.ReadInt32(), reader.ReadBoolean(), reader.ReadInt32());

        /// <summary>
        /// Adds a new Disabled Accessory type or slot. If typeOrSlot is true, a type. If false, a slot.
        /// </summary>
        private void AddDisabledAccessoryTypeOrSlot(BinaryReader reader, int fromWhomst, bool typeOrSlot) {
            if (Main.netMode == NetmodeID.Server) {
                AccessoryPlayer accessoryPlayer = Main.player[fromWhomst].GetModPlayer<AccessoryPlayer>();
                AccessoryPlayer.DisabledAccessoryInstance instance = ReadDisabledAccessoryInstance(reader);

                if (typeOrSlot) {
                    accessoryPlayer.disabledAccessoryTypes.Add(instance);
                }
                else {
                    accessoryPlayer.disabledAccessorySlots.Add(instance);
                }

                ModPacket packet = GetPacket(AddDisabledAccessoryType);
                packet.Write(fromWhomst);
                WriteDisabledAccessoryInstance(instance, packet);

                packet.Send(ignoreClient: fromWhomst);
            }
            else {
                AccessoryPlayer accessoryPlayer = Main.player[reader.ReadInt32()].GetModPlayer<AccessoryPlayer>();
                AccessoryPlayer.DisabledAccessoryInstance instance = ReadDisabledAccessoryInstance(reader);

                if (typeOrSlot) {
                    accessoryPlayer.disabledAccessoryTypes.Add(instance);
                }
                else {
                    accessoryPlayer.disabledAccessorySlots.Add(instance);
                }
            }
        }
    }
}