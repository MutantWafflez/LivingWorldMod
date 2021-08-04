using LivingWorldMod.Common.Systems;
using LivingWorldMod.Common.Systems.UI;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LivingWorldMod.Content.NPCs.Villagers {

    /// <summary>
    /// Base class for all of the villager NPCs in the mod. Has several properties that can be
    /// modified depending on the "personality" of the villagers.
    /// </summary>
    public abstract class Villager : ModNPC {

        /// <summary>
        /// What type of villager this class pertains to. Vital for several functions in the class
        /// and must be defined.
        /// </summary>
        public abstract VillagerType VillagerType {
            get;
        }

        /// <summary>
        /// The current status of the "relationship" between these villagers and the players.
        /// Returns the enum of said status.
        /// </summary>
        public VillagerRelationship RelationshipStatus {
            get {
                int reputation = ReputationSystem.GetVillageReputation(VillagerType);

                if (reputation <= HateThreshold) {
                    return VillagerRelationship.Hate;
                }
                else if (reputation > HateThreshold && reputation <= SevereDislikeThreshold) {
                    return VillagerRelationship.SevereDislike;
                }
                else if (reputation > SevereDislikeThreshold && reputation <= DislikeThreshold) {
                    return VillagerRelationship.Dislike;
                }
                else if (reputation >= LikeThreshold && reputation < LoveThreshold) {
                    return VillagerRelationship.Like;
                }
                else if (reputation >= LoveThreshold) {
                    return VillagerRelationship.Love;
                }

                return VillagerRelationship.Neutral;
            }
        }

        /// <summary>
        /// Threshold that the reputation must cross in order for these villagers to HATE the players.
        /// </summary>
        public virtual int HateThreshold => -95;

        /// <summary>
        /// Threshold that the reputation must cross in order for these villagers to SEVERELY
        /// DISLIKE the players.
        /// </summary>
        public virtual int SevereDislikeThreshold => -45;

        /// <summary>
        /// Threshold that the reputation must cross in order for these villagers to DISLIKE the
        /// players. The villagers will be considered "neutral" towards the players if the
        /// reputation is in-between the Dislike and Like thresholds.
        /// </summary>
        public virtual int DislikeThreshold => -15;

        /// <summary>
        /// Threshold that the reputation must cross in order for these villagers to LIKE the
        /// players. The villagers will be considered "neutral" towards the players if the
        /// reputation is in-between the Dislike and Like thresholds.
        /// </summary>
        public virtual int LikeThreshold => 15;

        /// <summary>
        /// Threshold that the reputation must cross in order for these villagers to LOVE the players.
        /// </summary>
        public virtual int LoveThreshold => 95;

        /// <summary>
        /// List of possible names that these villagers can have.
        /// </summary>
        public abstract List<string> PossibleNames {
            get;
        }

        /// <summary>
        /// Dialogue that is added to the list of reputation dialogue depending on the current
        /// event, if any, that is occurring.
        /// </summary>
        public abstract WeightedRandom<string> EventDialogue {
            get;
        }

        /// <summary>
        /// Possible dialogue that these villagers will say in the shop screen when opened by the player.
        /// </summary>
        public abstract WeightedRandom<string> ShopDialogue {
            get;
        }

        /// <summary>
        /// Possible dialogue that these villagers will say when they SEVERELY DISLIKE the players.
        /// </summary>
        public abstract WeightedRandom<string> SevereDislikeDialogue {
            get;
        }

        /// <summary>
        /// Possible dialogue that these villagers will say when they DISLIKE the players.
        /// </summary>
        public abstract WeightedRandom<string> DislikeDialogue {
            get;
        }

        /// <summary>
        /// Possible dialogue that these villagers will say when they are NEUTRAL to the players.
        /// </summary>
        public abstract WeightedRandom<string> NeutralDialogue {
            get;
        }

        /// <summary>
        /// Possible dialogue that these villagers will say when they LIKE the players.
        /// </summary>
        public abstract WeightedRandom<string> LikeDialogue {
            get;
        }

        /// <summary>
        /// Possible dialogue that these villagers will say when they LOVE the players.
        /// </summary>
        public abstract WeightedRandom<string> LoveDialogue {
            get;
        }

        public override string Texture => IOUtilities.LWMSpritePath + $"/NPCs/Villagers/{VillagerType}/{VillagerType}Style1";

        public override void AutoStaticDefaults() {
            base.AutoStaticDefaults();
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
                Velocity = 1f,
                Direction = -1
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetDefaults() {
            NPC.width = 25;
            NPC.height = 40;
            NPC.friendly = RelationshipStatus != VillagerRelationship.Hate;
            NPC.lifeMax = 500;
            NPC.defense = 15;
            NPC.knockBackResist = 0.5f;
            NPC.aiStyle = 7;
            AnimationType = NPCID.Guide;
        }

        //public override void ActsLikeTownNPC => true;

        //public override bool? SpawnsWithCustomName => true;

        public override string TownNPCName() => PossibleNames[WorldGen.genRand.Next(0, PossibleNames.Count)];

        public override void SetChatButtons(ref string button, ref string button2) {
            button = Language.GetTextValue("LegacyInterface.28"); //"Shop"
            button2 = "Reputation"; //TODO: Localization
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop) {
            //Shop Screen
            if (firstButton) {
                ShopUISystem.Instance.OpenShopUI(this);
            }
            //Reputation Screen
            else {
            }
        }

        public override bool CanChat() => RelationshipStatus != VillagerRelationship.Hate;

        public override string GetChat() {
            WeightedRandom<string> returnedList;

            switch (RelationshipStatus) {
                case VillagerRelationship.Hate:
                    return "..."; //The player will be unable to chat with any villagers if they are hated, but *just in case* they somehow do, make sure to have some kind of dialogue so an error isn't thrown

                case VillagerRelationship.SevereDislike:
                    returnedList = SevereDislikeDialogue;
                    break;

                case VillagerRelationship.Dislike:
                    returnedList = DislikeDialogue;
                    break;

                case VillagerRelationship.Neutral:
                    returnedList = NeutralDialogue;
                    break;

                case VillagerRelationship.Like:
                    returnedList = LikeDialogue;
                    break;

                case VillagerRelationship.Love:
                    returnedList = LoveDialogue;
                    break;

                default:
                    LivingWorldMod.Instance.Logger.Error("Villager Reputation isn't within the normal bounds!");
                    return "Somehow your reputation with us is broken. Contact a mod dev immediately!";
            }

            returnedList.AddList(EventDialogue);

            return returnedList;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            SpriteEffects spriteDirection = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(Mod.Assets.Request<Texture2D>(Texture.Replace("LivingWorldMod/", "")).Value, new Rectangle((int)(NPC.Right.X - (NPC.frame.Width / 1.5) - screenPos.X), (int)(NPC.Bottom.Y - NPC.frame.Height - screenPos.Y + 2f), NPC.frame.Width, NPC.frame.Height), NPC.frame, drawColor, NPC.rotation, default, spriteDirection, 0);
            return false;
        }
    }
}