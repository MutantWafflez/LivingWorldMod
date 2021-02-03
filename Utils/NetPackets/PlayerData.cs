using LivingWorldMod.ID;
using System;
using System.IO;
using Terraria;

namespace LivingWorldMod.Utilities.NetPackets
{
    public class PlayerData : ObjectData<LWMPlayer>
    {
        public PlayerData() : this(null) {} // allow for handler registration
        public PlayerData(LWMPlayer player) : base(PacketID.PlayerData, player) {}

        protected override void WriteId(BinaryWriter writer, LWMPlayer obj)
        {
            writer.Write((byte) obj.player.whoAmI);
        }

        protected override LWMPlayer ReadId(BinaryReader reader)
        {
            return Main.player[reader.ReadByte()].GetModPlayer<LWMPlayer>();
        }
    }
}