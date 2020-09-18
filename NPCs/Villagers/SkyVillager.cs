using LivingWorldMod.Utils;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.NPCs.Villagers
{

    public class SkyVillager : Villager
    {
        /*public bool isFlying = false;

        private int flyingFrame = 0;

        private int flyingFrameCounter = 0; //This is created due to npc.frameCounter being reverted to 0 in the Vanilla TownNPC code, so a variable must be made

        private int flightCooldown = 0; //Used for a cooldown on doing flight so no rare chances of instant landing/flying*/

        /*public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[npc.type] = 27;
        }*/

        /*public override string TownNPCName()
        {
            switch (WorldGen.genRand.Next(10))
            {
                case 0:
                    return "Merel";
                case 1:
                    return "Mari";
                case 2:
                    return "Wren";
                case 3:
                    return "Yona";
                case 4:
                    return "Jena";
                case 5:
                    return "Tori";
                case 6:
                    return "Loa";
                case 7:
                    return "Eve";
                case 8:
                    return "Rima";
                case 9:
                    return "Luyu";
                default:
                    return "Robin";
            }
        }*/

        /*public override bool PreAI()
        {
            //We want the Vanilla AI to run only when the Harpy is not flying, however otherwise, allow our AI to take over
            if (isFlying || (npc.ai[0] >= 1.1f && npc.ai[0] <= 1.3f))
            {
                AI(); //This is here because returning false in PreAI will also prevent our AI from running
                return false;
            }
            return base.PreAI();
        }

        public override void AI()
        {
            if (!isFlying)
            {
                flightCooldown--;
                if (flightCooldown < 0)
                    flightCooldown = 0;
                //All of this is for the Transition of being on the Ground to being in the Air
                if (npc.velocity.X == 0f && npc.ai[0] != 7f && flightCooldown <= 0)
                {
                    //Initial Jump
                    if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.Next(1501) == 0 && npc.ai[0] != 1.1f && npc.ai[0] != 1.2f)
                    {
                        npc.velocity.X *= 0f;
                        npc.velocity.Y = -8f;
                        npc.ai[0] = 1.1f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                        return;
                    }
                    //Be in Jump Animation momentarily
                    if (npc.ai[0] == 1.1f && ++npc.ai[1] > 15)
                    {
                        npc.ai[0] = 1.2f;
                        npc.ai[1] = 0f;
                        npc.netUpdate = true;
                        return;
                    }
                    //Begin flapping wings, adjusting against gravity
                    else if (npc.ai[0] == 1.2f && ++npc.ai[1] > 20)
                    {
                        npc.ai[0] = 1.3f;
                        npc.ai[1] = 0f;
                        npc.noGravity = true;
                        npc.netUpdate = true;
                        return;
                    }
                    //Has Positive Lift, begin moving upwards against gravity
                    else if (npc.ai[0] == 1.3f && ++npc.ai[1] < 40)
                    {
                        npc.velocity.Y -= 0.1f;
                        npc.netUpdate = true;
                        return;
                    }
                    //Lift has been balanced with gravity, steady out and allow for horizontal movement
                    else if (npc.ai[0] == 1.3f)
                    {
                        npc.ai[0] = 0f;
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        npc.velocity *= 0f;
                        isFlying = true;
                        npc.netUpdate = true;
                        return;
                    }
                }
            }
            else
            {
                //Placeholder conditions for when landing will take place, will be replaced when Village Structures are finished
                if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.Next(1501) == 0 && npc.ai[0] != -1f)
                {
                    npc.velocity *= 0f;
                    npc.velocity.Y = 0.01f;
                    npc.ai[0] = -1f;
                    npc.ai[1] = 0f;
                    npc.netUpdate = true;
                    return;
                }
                //Begin moving towards ground, slowly
                if (npc.ai[0] == -1f && (!npc.collideY || npc.velocity.Y != 0f))
                {
                    npc.velocity.Y += 0.1f;
                    if (npc.velocity.Y > 1.5f)
                        npc.velocity.Y = 1.5f;
                    return;
                }
                //On touching ground, revert back to walking animations, and allow for chat
                else if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[0] == -1f && (npc.collideY || npc.velocity.Y == 0f))
                {
                    npc.ai[0] = 0f;
                    npc.ai[1] = Main.rand.Next(300, 800); //Will stand still for a random interval instead of instant walking
                    npc.ai[2] = 0f;
                    npc.ai[3] = 0f;
                    npc.velocity *= 0f;
                    flightCooldown = Main.rand.Next(60 * 120, 60 * 240); //Random flight cooldown anywhere from 120 seconds to 240 seconds
                    isFlying = false;
                    npc.noGravity = false;
                    npc.netUpdate = true;
                    return;
                }
            }
        }*/

        /*public override void UpdateChatLists()
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
                getChat.Add("Humans... Despicable. Created those foul windows!");
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
            getChat.ConditionalStringAdd("I’m super thankful we’re above the clouds. I cannot stand my hair being wet.", Main.raining, 2);
            getChat.ConditionalStringAdd("Ahh, watching the rain fall from the clouds is so relaxing.", Main.raining, 2);
            //Solar Eclipse
            getChat.ConditionalStringAdd("It’s so fun watching all the monsters going crazy down there attacking people. I’m glad we’re somewhat safe up here.", Main.eclipse, 2);
            getChat.ConditionalStringAdd("What, are you scared or something?", Main.eclipse, 2);
        }

        public override void FindFrame(int frameHeight)
        {
            //Flying animation should take place when flying, obviously, and when the Harpy is adjusting against Gravity when first taking off
            if (npc.ai[0] != -1f && (isFlying || npc.ai[0] == 1.2f || npc.ai[0] == 1.3f))
            {
                npc.frame.Y = (21 + flyingFrame) * frameHeight;
                if (++flyingFrameCounter >= 6)
                {
                    if (++flyingFrame > 5)
                        flyingFrame = 0;
                    flyingFrameCounter = 0;
                }
            }
            //Slower flying animation when landing
            else if (isFlying && npc.ai[0] == -1f)
            {
                npc.frame.Y = (21 + flyingFrame) * frameHeight;
                if (++flyingFrameCounter >= 12)
                {
                    if (++flyingFrame > 5)
                        flyingFrame = 0;
                    flyingFrameCounter = 0;
                }
            }
        }*/
    }
}
