using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LivingWorldMod.Utils;
using Terraria.Utilities;

namespace LivingWorldMod.NPCs.Villagers
{
    [AutoloadHead]
    public class SkyVillager : Villager
    {

        public SkyVillager() : base("Harpy", 1, VillagerType.SkyVillager) { }

        public override void SetDefaults()
        {
            base.SetDefaults();
            isMerchant = true;
        }

        public override string TownNPCName()
        {
            return "Test";
        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            shop.item[nextSlot].SetDefaults(ItemID.Feather);
            nextSlot++;
        }

        public override void UpdateChatLists()
        {
            reputationChat.Clear();
            getChat.Clear();
            //Reputation Based Chat
            if (isNegativeRep || Main.bloodMoon)
            {
                if (isNegativeRep && !Main.bloodMoon)
                    reputationChat.Add("You totally suck. You’d better make things better, or you’ll be ripped apart limb by limb the next time you even set foot here!");
                getChat.Add("Don’t take it personally if we like, rip off your arms and eat you or something.");
                getChat.Add("How do you even live with those repulsive meat sticks in place of wings?");
                getChat.Add("You really ruffle my feathers, landwalker!");
                getChat.Add("How’s the weather down there? I hope it’s horrible.");
                getChat.Add("Leave me alone you ugly landwalking son of a- squak!");
                getChat.Add("Do you want my feathers going down your back like a stegosaurus?", 0.66);
                getChat.Add("Are all the flightless this fowl and annoying...?");
            }
            else if (isNeutralRep)
            {
                reputationChat.Add("I don’t have much of an opinion on you. You’re alright I guess, I don’t mind you being here.");
                getChat.Add("I don’t get it, what’s so wrong with being bird-brained?");
                getChat.Add("No, I don’t want any crackers. And stop calling me Polly!", 0.66);
                getChat.Add("I hope I can be at the front of the V formation at least once before I die.");
                getChat.Add("Flying through the sky is like, so freeing. You should try it sometime.");
                getChat.Add("You’re asking how we reproduce if we’re all female? Don’t be silly.", 0.33);
                getChat.Add("I’m sort of scared of flying too high and floating into space or the sun or something.");
                getChat.Add("You’ll buy something, won’t you? You didn’t come here for nothing, right?");
            }
            else if (isPositiveRep || isMaxRep)
            {
                if (isPositiveRep)
                    reputationChat.Add("Hey, you aren’t so bad for a human. I hope you’ll visit us more often, we don’t have much to do up here in the sky anyway.");
                else
                    reputationChat.Add("The village loves you! It sucks that you aren’t a Harpy, but we don’t mind. Your charisma definitely compensates for it, plus, you’re like, pretty cute for a human. What? I didn’t say anything.");
                getChat.Add(Main.player[Main.myPlayer].name + "! You want to buy something, right? Or did you just want to talk to me?");
                getChat.Add("Aren’t my feathers pretty today? I spent all day grooming them.");
                getChat.Add("Does this top make my wings look fat?", 0.66);
                getChat.Add("You should sit with me on my perch sometime. The breeze is so nice, it really helps my zen.");
                getChat.Add("You aren’t so bad for a landwalking human. Come visit again sometime!");
                getChat.Add("It gets sort of boring up here. But it’s not so bad when you come around.");
                getChat.Add("ptoo- Sorry, I got a feather in my mouth again. Happens every time.", 0.66);
                getChat.Add("No I’m not flirting with you! Stupid human!", 0.33);
            }
            //General Event Chat

            //Rain
            getChat.ConditionalStringAdd("I’m super thankful we’re above the clouds. I cannot stand my hair being wet.", Main.raining);
            getChat.ConditionalStringAdd("Ahh, watching the rain fall from the clouds is so relaxing.", Main.raining);
            //Solar Eclipse
            getChat.ConditionalStringAdd("It’s so fun watching all the monsters going crazy down there attacking people. I’m glad we’re somewhat safe up here.", Main.eclipse);
            getChat.ConditionalStringAdd("What, are you scared or something?", Main.eclipse);
        }
    }
}
