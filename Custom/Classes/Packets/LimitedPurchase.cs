using System;
using System.Collections.Generic;
using System.IO;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using Terraria;

namespace LivingWorldMod.Custom.Classes.Packets {

    public class LimitedPurchase : LWMPacket {
        public int npcId;
        public int slotId;

        public LimitedPurchase() : base(PacketID.LimitedPurchase) { }

        protected override void Write(BinaryWriter writer) {
            writer.Write(npcId);
            writer.Write(slotId);
        }

        protected override void Read(BinaryReader reader) {
            npcId = reader.ReadInt32();
            slotId = reader.ReadInt32();
        }

        protected override void Handle(int sentFromPlayer) {
            // expected only from clients LivingWorldMod.mod.Logger.Debug("Limited purchase packet");

            // get npc
            NPC npc = Main.npc[npcId];
            if (!(npc?.modNPC is Villager villager)) {
                LivingWorldMod.Instance.Logger.Debug("no npc, skipping handle");
                return;
            }
            // get player shop
            Guid? id = Main.player[sentFromPlayer]?.GetModPlayer<LWMPlayer>()?.guid;
            if (id == null) {
                LivingWorldMod.Instance.Logger.Debug("no player / guid, skipping handle");
                return; // player not found
            }

            List<ShopItem> shop = villager.GetPlayerShop((Guid)id);
            ShopItem item = shop[slotId];
            if (item == null) {
                LivingWorldMod.Instance.Logger.Debug("no shop item, skipping handle");
                return; // shop or item slot not found
            }

            if (item.stackSize > 0) {
                item.stackSize--;
                // LivingWorldMod.mod.Logger.Debug("stack size reduced to " + item.stackSize);
            }
            else
                LivingWorldMod.Instance.Logger.Debug("stack size already at minimum");

            // although the stock is updated, and we should technically update... the client has
            // also decreased the shop item count. And no one else cares. So really we don't *have*
            // to do anything here and can save bandwidth. npc.netUpdate = true;
        }
    }
}