using LivingWorldMod.ID;
using LivingWorldMod.NPCs.Villagers;
using System.IO;
using Terraria;

namespace LivingWorldMod.Utilities.NetPackets
{
    public class VillagerData : ObjectData<Villager>
    {
        public VillagerData() : this(null) {} // allow for handler registration
        public VillagerData(Villager serialObj) : base(PacketID.VillagerData, serialObj) {}

        protected override void WriteId(BinaryWriter writer, Villager obj)
        {
            writer.Write((byte) obj.npc.whoAmI);
        }

        protected override Villager ReadId(BinaryReader reader)
        {
            return Main.npc[reader.ReadByte()].modNPC as Villager;
        }
    }
}