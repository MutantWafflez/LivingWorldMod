using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.NPCs.Villagers
{
    public class SkyVillager : Villager
    {
        public SkyVillager() : base("Harpy", 1) { }

        public override void SetDefaults()
        {
            base.SetDefaults();
            isMerchant = true;
        }

        public override string GetChat()
        {
            WeightedRandom<string> chatChoices = new WeightedRandom<string>();

            chatChoices.Add("Huh? Don't take it personally if we like, rip off your arms and eat you or something.");
            chatChoices.Add("I don't get it, what's so wrong with being bird-brained?");
            chatChoices.Add("No, I don't want any crackers. And stop calling me Polly!");

            return chatChoices;
        }

        public override string TownNPCName()
        {
            switch (WorldGen.genRand.Next(2))
            {
                case 0:
                    return "Harpsicord";
                case 1:
                    return "Darpy";
                default:
                    return "Name? I don't have a name. (Report to Devs!)";
            }
        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            shop.item[nextSlot].SetDefaults(ItemID.Feather);
            nextSlot++;
        }
    }
}
