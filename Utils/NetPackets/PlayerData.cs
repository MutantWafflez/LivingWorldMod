using LivingWorldMod.ID;
using System;
using System.IO;
using Terraria;

namespace LivingWorldMod.Utilities.NetPackets
{
    public class PlayerData : LWMPacket
    {
        public int pid;
        public Guid guid;

        public PlayerData() : base(PacketID.PlayerData) {}
        
        protected override void Write(BinaryWriter writer)
        {
            writer.Write((byte) pid);
            writer.Write(guid.ToString());
        }

        protected override void Read(BinaryReader reader)
        {
            pid = reader.ReadByte();
            guid = Guid.Parse(reader.ReadString());
        }
        
        protected override void Handle(int sentFromPlayer)
        {
            LWMPlayer player = Main.player[pid].GetModPlayer<LWMPlayer>();
            player.guid = guid;
        }
    }
}