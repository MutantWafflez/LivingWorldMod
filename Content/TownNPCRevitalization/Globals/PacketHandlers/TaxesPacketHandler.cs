using System.IO;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.Globals.ModTypes;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.PacketHandlers;

/// <summary>
///     Packet Handler that handles all the MP syncing functionality of the new Taxes overhaul.
/// </summary>
public class TaxesPacketHandler : PacketHandler {
    /// <summary>
    ///     Sent by a client when they change a given NPCs tax values, which is then verified by the server and sent to all other clients, assuming the new tax values are valid.
    /// </summary>
    public const byte ChangeTaxValue = 0;

    public override void HandlePacket(BinaryReader reader, int fromWhomst) {
        byte packetType = reader.ReadByte();

        switch (packetType) {
            case ChangeTaxValue: {
                int npcType = reader.ReadInt32();
                NPCTaxValues newTaxValues = new(reader.ReadInt32(), reader.ReadSingle());

                if (Main.netMode == NetmodeID.Server) {
                    ModPacket packet = GetPacket();

                    packet.Write(npcType);

                    // If the sent tax values are invalid, respond to client that sent the packet with the correct, server-side tax values
                    if (!TaxesSystem.AreValidTaxValues(newTaxValues)) {
                        NPCTaxValues serverTaxValuesForNPC = TaxesSystem.Instance.GetTaxValuesOrDefault(npcType);

                        packet.Write(serverTaxValuesForNPC.PropertyTax);
                        packet.Write(serverTaxValuesForNPC.SalesTax);

                        packet.Send(fromWhomst);
                        break;
                    }

                    TaxesSystem.Instance.SubmitNewTaxValues(npcType, newTaxValues);

                    packet.Write(newTaxValues.PropertyTax);
                    packet.Write(newTaxValues.SalesTax);

                    packet.Send();
                    break;
                }

                // Client
                TaxesSystem.Instance.SubmitNewTaxValues(npcType, newTaxValues);
                TaxSheetUISystem.Instance.correspondingUIState.RefreshStateWithCurrentNPC();
                break;
            }
        }
    }
}