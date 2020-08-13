using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.NPCs.Villagers
{
    [AutoloadHead]
    public class SkyVillager : Villager
    {
        public static readonly string[] skyHostile =
        {
            "You suck.",
        };

        public static readonly string[] skyNeutral =
        {
            "You're alright, I suppose."
        };

        public static readonly string[] skyFriendly =
        {
            "You're great!"
        };

        public SkyVillager() : base("Harpy", 1, VillagerType.SkyVillager, skyHostile, skyNeutral, skyFriendly) { }

        public override void SetDefaults()
        {
            base.SetDefaults();
            isMerchant = true;
        }

        public override string TownNPCName()
        {
            return "Test";
        }

        public override string GetChat()
        {
            WeightedRandom<string> chatChoices = new WeightedRandom<string>();

            chatChoices.Add("Huh? Don't take it personally if we like, rip off your arms and eat you or something.");
            chatChoices.Add("I don't get it, what's so wrong with being bird-brained?");
            chatChoices.Add("No, I don't want any crackers. And stop calling me Polly!");

            return chatChoices;
        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            shop.item[nextSlot].SetDefaults(ItemID.Feather);
            nextSlot++;
        }
    }
}
