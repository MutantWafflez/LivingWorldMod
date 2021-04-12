using System.IO;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Custom.Enums;
using Terraria;

namespace LivingWorldMod.Custom.Classes.Packets
{
    public class PlayerData : ObjectData<LWMPlayer>
    {
        public PlayerData() : this(null) { } // allow for handler registration

        public PlayerData(LWMPlayer player) : base(PacketID.PlayerData, player) { }

        protected override void WriteId(BinaryWriter writer, LWMPlayer obj)
        {
            writer.Write((byte)obj.player.whoAmI);
        }

        protected override LWMPlayer ReadId(BinaryReader reader)
        {
            return Main.player[reader.ReadByte()].GetModPlayer<LWMPlayer>();
        }
    }
}