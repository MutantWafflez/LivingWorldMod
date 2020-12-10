using LivingWorldMod.Items.Accessories;
using LivingWorldMod.Items.Placeable.Paintings;
using LivingWorldMod.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.NPCs.Villagers
{
    public class SkyVillager : Villager
    {
        #region AI Data

        public bool isFlying = false;

        private int flyingFrame = 0; //Used for what animation frame of flying the harpy is in
        private int flyingFrameCounter = 0; //This is created due to npc.frameCounter being reverted to 0 in the Vanilla TownNPC code, so a variable must be made
        private int flightCooldown = 0;

        #endregion AI Data

        #region Villager Class Overrides

        public override WeightedRandom<string> GetDialogueText()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();

            if (isHatedRep || isNegativeRep || Main.bloodMoon)
            {
                chat.Add("Don’t take it personally if we like, rip off your arms and eat you or something.");
                chat.Add("How do you even live with those repulsive meat sticks in place of wings?");
                chat.Add("You really ruffle my feathers, landwalker!");
                chat.Add("How’s the weather down there? I hope it’s horrible.");
                chat.Add("Leave me alone you ugly landwalking son of a- squak!");
                chat.Add("Do you want my feathers going down your back like a stegosaurus?", 0.66);
                chat.Add("Are all the flightless this fowl and annoying...?");
                chat.Add("Humans... Despicable. Created those foul windows!");
            }
            else if (isNeutralRep)
            {
                chat.Add("I don’t get it, what’s so wrong with being bird-brained?");
                chat.Add("No, I don’t want any crackers. And stop calling me Polly!", 0.66);
                chat.Add("I hope I can be at the front of the V formation at least once before I die.");
                chat.Add("Flying through the sky is like, so freeing. You should try it sometime.");
                chat.Add("You’re asking how we reproduce if we’re all female? Don’t be silly.", 0.33);
                chat.Add("I’m sort of scared of flying too high and floating into space or the sun or something.");
                chat.Add("You’ll buy something, won’t you? You didn’t come here for nothing, right?");
            }
            else if (isPositiveRep || isMaxRep)
            {
                chat.Add(Main.LocalPlayer.name + "! You want to buy something, right? Or did you just want to talk to me?");
                chat.Add("Aren’t my feathers pretty today? I spent all day grooming them.");
                chat.Add("Does this top make my wings look fat?", 0.66);
                chat.Add("You should sit with me on my perch sometime. The breeze is so nice, it really helps my zen.");
                chat.Add("You aren’t so bad for a landwalking human. Come visit again sometime!");
                chat.Add("It gets sort of boring up here. But it’s not so bad when you come around.");
                chat.Add("ptoo- Sorry, I got a feather in my mouth again. Happens every time.", 0.66);
                chat.Add("No I’m not flirting with you! Stupid human!", 0.33);
            }
            //General Event Chat

            //Rain
            chat.ConditionalStringAdd("I’m super thankful we’re above the clouds. I cannot stand my hair being wet.", Main.raining, 2);
            chat.ConditionalStringAdd("Ahh, watching the rain fall from the clouds is so relaxing.", Main.raining, 2);
            //Solar Eclipse
            chat.ConditionalStringAdd("It’s so fun watching all the monsters going crazy down there attacking people. I’m glad we’re somewhat safe up here.", Main.eclipse, 2);
            chat.ConditionalStringAdd("What, are you scared or something?", Main.eclipse, 2);

            return chat;
        }

        public override WeightedRandom<string> GetReputationText()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();

            chat.ConditionalStringAdd("You totally suck. You’d better make things better, or you’ll be ripped apart limb by limb the next time you even set foot here!", isNegativeRep || isHatedRep);
            chat.ConditionalStringAdd("I don’t have much of an opinion on you. You’re alright I guess, I don’t mind you being here.", isNeutralRep);
            chat.ConditionalStringAdd("Hey, you aren’t so bad for a human. I hope you’ll visit us more often, we don’t have much to do up here in the sky anyway.", isPositiveRep);
            chat.ConditionalStringAdd("The village loves you! It sucks that you aren’t a Harpy, but we don’t mind. Your charisma definitely compensates for it, plus, you’re like, pretty cute for a human. What? I didn’t say anything.", isMaxRep);

            return chat;
        }

        public override List<string> GetPossibleNames()
        {
            List<string> names = new List<string>
            {
                "Merel",
                "Mari",
                "Wren",
                "Yona",
                "Jena",
                "Tori",
                "Loa",
                "Eve",
                "Rima",
                "Luyu",
                "Robin"
            };
            return names;
        }

        public override bool CanChat() => !isFlying;

        #endregion Villager Class Overrides

        #region Normal Overrides

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[npc.type] = 27;
        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            shop.item[nextSlot].SetDefaults(ModContent.ItemType<FeatherBag>());
            nextSlot++;
            shop.item[nextSlot].SetDefaults(ModContent.ItemType<SkyBustTileItem>());
            nextSlot++;
            shop.item[nextSlot].SetDefaults(ModContent.ItemType<OneStarTileItem>());
            nextSlot++;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.type == ProjectileID.HarpyFeather)
            {
                return false;
            }
            return base.CanBeHitByProjectile(projectile);
        }

        #endregion Normal Overrides

        #region Animation & AI

        public override void AI()
        {
            //This check is IMPORTANT. We don't want to use PreAI cause that obstructs functionality of some other mods,
            //so changing aiStyle is a much better way of doing this so that other mods can still properly block this AI from
            //running if need be.
            if (isFlying || (npc.ai[0] >= 1.1f && npc.ai[0] <= 1.3f))
                npc.aiStyle = -1;
            else
                npc.aiStyle = 7;

            if (isFlying)
            {
                npc.width = 45;
                npc.height = 50;
            }
            else
            {
                npc.width = 25;
                npc.height = 40;
            }

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
        }

        #endregion Animation & AI
    }
}