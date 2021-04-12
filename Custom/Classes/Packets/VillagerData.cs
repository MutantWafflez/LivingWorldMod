using System.IO;
using LivingWorldMod.Content.NPCs.Villagers;
using LivingWorldMod.Custom.Enums;
using Terraria;

namespace LivingWorldMod.Custom.Classes.Packets
{
    public class VillagerData : ObjectData<Villager>
    {
        public VillagerData() : this(null) { } // allow for handler registration

        public VillagerData(Villager serialObj) : base(PacketID.VillagerData, serialObj) { }

        protected override void WriteId(BinaryWriter writer, Villager obj)
        {
            writer.Write((byte)obj.npc.whoAmI);
        }

        protected override Villager ReadId(BinaryReader reader)
        {
            return Main.npc[reader.ReadByte()].modNPC as Villager;
        }
    }
}