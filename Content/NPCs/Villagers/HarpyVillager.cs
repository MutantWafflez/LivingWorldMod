using System.Collections.Generic;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.Content.NPCs.Villagers {

    public class HarpyVillager : Villager {
        public override VillagerType VillagerType => VillagerType.Harpy;

        public override List<string> PossibleNames => new List<string> {
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

        public override WeightedRandom<string> EventDialogue {
            get {
                WeightedRandom<string> list = new WeightedRandom<string>();

                //Rain
                list.AddConditionally("I’m super thankful we’re above the clouds. I cannot stand my hair being wet.", Main.raining, 2);
                list.AddConditionally("Ahh, watching the rain fall from the clouds is so relaxing.", Main.raining, 2);
                //Solar Eclipse
                list.AddConditionally("It’s so fun watching all the monsters going crazy down there attacking people. I’m glad we’re somewhat safe up here.", Main.eclipse, 2);
                list.AddConditionally("What, are you scared or something?", Main.eclipse, 2);

                return list;
            }
        }

        public override WeightedRandom<string> ShopDialogue {
            get {
                WeightedRandom<string> list = new WeightedRandom<string>();
                VillagerRelationship currentRelationship = RelationshipStatus;

                //Severe Dislike
                list.AddConditionally("You're lucky I am even letting you speak to me, let alone buy my stuff. Just know there'll be a markup for anything you buy due to all the trouble you've caused us.", currentRelationship == VillagerRelationship.SevereDislike);
                list.AddConditionally("You filthy landwalkers coming around the world barging into places like you own them. Just tell me what you want and I'll decide how much to mark it up by.", currentRelationship == VillagerRelationship.SevereDislike);
                //Dislike
                list.AddConditionally("You know, since there aren't any laws regulating prices and there really isn't any competition, due to the minor trouble you've put us through, I think your next product will cost a little extra.", currentRelationship == VillagerRelationship.Dislike);
                list.AddConditionally("Tread carefully, landwalker. Keep slipping up like you have been lately, and soon it'll cost an entire platinum to buy my wares!", currentRelationship == VillagerRelationship.Dislike);
                //Neutral
                list.AddConditionally("Nice of you to stop by, landwalker. Would appreciate the extra coin right now, everything is at the standard price!", currentRelationship == VillagerRelationship.Neutral);
                list.AddConditionally("What kind of harpy would I be to give you unfair prices if you really haven't made an impression?", currentRelationship == VillagerRelationship.Neutral);
                //Like
                list.AddConditionally("I wonder if my friends wanna go practice feather shooting later today... Erm- right, anything I got pique your interest landwalker?", currentRelationship == VillagerRelationship.Like);
                list.AddConditionally("You know, when you first came here, I thought you were like all the other landwalkers, but I was pleasantly surprised that's not the case. Have, like, a small discount, on me!", currentRelationship == VillagerRelationship.Like);
                //Love
                list.AddConditionally("Before you even mention it- no, actually, before you even THINK it, this discount I'm giving you is cause you've helped the village so much and not because I'm interested in you!", currentRelationship == VillagerRelationship.Love);
                list.AddConditionally("You know landwalker, have this discount on me. I guess you deserve it for helping the village so much, or whatever. Just buy something!", currentRelationship == VillagerRelationship.Love);

                return list;
            }
        }

        public override WeightedRandom<string> SevereDislikeDialogue {
            get {
                WeightedRandom<string> list = new WeightedRandom<string>();

                list.Add("Don’t take it personally if we like, rip off your arms and eat you or something.");
                list.Add("How do you even live with those repulsive meat sticks in place of wings?");
                list.Add("You really ruffle my feathers, landwalker!");
                list.Add("How’s the weather down there? I hope it’s horrible.");
                list.Add("Leave me alone you ugly landwalking son of a- squak!");
                list.Add("Do you want my feathers going down your back like a stegosaurus?", 0.66f);
                list.Add("Are all the flightless this fowl and annoying...?");
                list.Add("Humans... Despicable. Created those foul windows!");

                return list;
            }
        }

        public override WeightedRandom<string> DislikeDialogue {
            get {
                WeightedRandom<string> list = new WeightedRandom<string>();

                list.Add("Tread carefully, landwalker. You've upset a few of us here around the village.");
                list.Add("Sure, buy something if you want. But for the inconveniences you've caused some of us, there will be a bit of a markup.");
                list.Add("If you think some of us are giving you a nasty look, you're probably right.");
                list.Add("What do you want? I'm trying to relax here.");
                list.Add("You haven't been making the best impressions around here lately. You might want to change that, preferably soon.");
                list.Add("Oh hey. Can I help you at all? No? Then get out of my face.");
                list.Add("I'm gonna be straight with you, human. You've caused some problems for us. Nothing outrageous, but it's left a bit of a sour taste in our mouths. We may understand if they were just one time mistakes, but you better act carefully before things get very, very bad for you.", 0.34f);

                return list;
            }
        }

        public override WeightedRandom<string> NeutralDialogue {
            get {
                WeightedRandom<string> list = new WeightedRandom<string>();

                list.Add("I don’t get it, what’s so wrong with being bird-brained?");
                list.Add("I hope I can be at the front of the V formation at least once before I die.");
                list.Add("Flying through the sky is like, so freeing. You should try it sometime.");
                list.Add("I’m sort of scared of flying too high and floating into space or the sun or something.");
                list.Add("You’ll buy something, won’t you? You didn’t come here for nothing, right?");
                list.Add("No, I don’t want any crackers. And stop calling me Polly!", 0.66f);
                list.Add("You’re asking how we reproduce if we’re all female? Don’t be silly.", 0.33f);

                return list;
            }
        }

        public override WeightedRandom<string> LikeDialogue {
            get {
                WeightedRandom<string> list = new WeightedRandom<string>();

                list.Add($"{Main.LocalPlayer.name}! You want to buy something, right? Or did you just want to talk to me?");
                list.Add("Aren’t my feathers pretty today? I spent all day grooming them.");
                list.Add("You should sit with me on my perch sometime. The breeze is so nice, it really helps my zen.");
                list.Add("You aren’t so bad for a landwalking human. Come visit again sometime!");
                list.Add("It gets sort of boring up here. But it’s not so bad when you come around.");
                list.Add("Does this top make my wings look fat?", 0.66f);
                list.Add("ptoo- Sorry, I got a feather in my mouth again. Happens every time.", 0.66f);
                list.Add("No I’m not flirting with you! Stupid human!", 0.33f);

                return list;
            }
        }

        public override WeightedRandom<string> LoveDialogue {
            get {
                WeightedRandom<string> list = new WeightedRandom<string>();

                list.Add("Hey! There's my favorite human! How have things been going for you?");
                list.Add("You know, you're lucky you're super liked around here in the village. No way else would we be giving out discounts like we do for anyone else!");
                list.Add($"Heyyy {Main.LocalPlayer.name}! So happy you could stop by! You got some stories for us this time around? We'd love to hear them!");
                list.Add("You know, if I'm being honest, when you first appeared in the village, we weren't sure what to think of you. We never get landwalking visitors up here. But you know what? I'm glad you came!");
                list.Add("You don't find the phrase \"landwalker\" offensive, do you? We've been saying it for so long! Oh man...");
                list.Add("I don't know how you could ever handle going underground. The air must be so stale and dry... It's much better up here in the sky!");
                list.Add("So you're probably still wondering how we reproduce if we're all female, right? Well, I'll tell you if you gift us some Luminite.", 0.1f); //Soon.

                return list;
            }
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();

            Main.npcFrameCount[NPC.type] = 27;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,

                new FlavorTextBestiaryInfoElement("A villager from the Harpy Village in the sky. Don't ruffle their feathers or attempt flirtation if you value your life.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) {
            return 2f; //Debug
        }
    }
}