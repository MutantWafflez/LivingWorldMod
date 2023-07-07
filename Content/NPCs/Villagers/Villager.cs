using LivingWorldMod.Common.Systems;
using LivingWorldMod.Common.Systems.UI;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace LivingWorldMod.Content.NPCs.Villagers {
    /// <summary>
    /// Base class for all of the villager NPCs in the mod. Has several properties that can be
    /// modified depending on the "personality" of the villagers.
    /// </summary>
    public abstract class Villager : ModNPC {
        /// <summary>
        /// A list of shop items that this specific villager is selling at this very moment.
        /// </summary>
        public List<ShopItem> shopInventory;

        /// <summary>
        /// An array that holds all of the assets for the body sprites of this type of villager.
        /// </summary>
        [CloneByReference]
        public readonly Asset<Texture2D>[] bodyAssets;

        /// <summary>
        /// Any array that holds all of the assets for the head sprites of this type of villager.
        /// What a "head" asset for a villager means depends on the type of villager. For the Harpy
        /// Villagers, for example, the head assets are different types of hair.
        /// </summary>
        [CloneByReference]
        public readonly Asset<Texture2D>[] headAssets;

        /// <summary>
        /// The body sprite type that this specific villager has.
        /// </summary>
        public int bodySpriteType;

        /// <summary>
        /// The head sprite type that this specific villager has.
        /// </summary>
        public int headSpriteType;

        /// <summary>
        /// What type of villager this class pertains to. Vital for several functions in the class
        /// and must be defined.
        /// </summary>
        public abstract VillagerType VillagerType {
            get;
        }

        /// <summary>
        /// A list of ALL POSSIBLE shop items that villagers of this given type can ever sell. This
        /// list is checked upon every restock.
        /// </summary>
        public abstract WeightedRandom<ShopItem> ShopPool {
            get;
        }

        /// <summary>
        /// Count of the total amount of variation in terms of body sprites for this specific
        /// villager type. Defaults to 5.
        /// </summary>
        public virtual int BodyAssetVariations => 5;

        /// <summary>
        /// Count of the total amount of variation in terms of head sprites for this specific
        /// villager type. Defaults to 5.
        /// </summary>
        public virtual int HeadAssetVariations => 5;

        /// <summary>
        /// Shorthand get property for acquiring the current relationship status of whatever type of village this villager belongs to.
        /// </summary>
        public VillagerRelationship RelationshipStatus => ReputationSystem.Instance.GetVillageRelationship(VillagerType);

        public sealed override string Texture => LivingWorldMod.LWMSpritePath + $"NPCs/Villagers/{VillagerType}/DefaultStyle";

        public override bool IsCloneable => true;

        /// <summary>
        /// A counter for how long this Villager has been homeless for, used for automatically leaving
        /// </summary>
        private int _homelessCounter;

        public Villager() {
            bodyAssets = new Asset<Texture2D>[BodyAssetVariations];
            headAssets = new Asset<Texture2D>[HeadAssetVariations];

            for (int i = 0; i < BodyAssetVariations; i++) {
                bodyAssets[i] = ModContent.Request<Texture2D>(LivingWorldMod.LWMSpritePath + $"NPCs/Villagers/{VillagerType}/Body{i}");
            }

            for (int i = 0; i < HeadAssetVariations; i++) {
                headAssets[i] = ModContent.Request<Texture2D>(LivingWorldMod.LWMSpritePath + $"NPCs/Villagers/{VillagerType}/Head{i}");
            }
        }

        public override ModNPC NewInstance(NPC entity) {
            Villager instance = (Villager)base.NewInstance(entity);

            instance.RestockShop();

            instance.bodySpriteType = Main.rand.Next(BodyAssetVariations);
            instance.headSpriteType = Main.rand.Next(HeadAssetVariations);

            return instance;
        }

        public override ModNPC Clone(NPC newEntity) {
            Villager clone = (Villager)base.Clone(newEntity);

            clone.shopInventory = shopInventory;

            clone.bodySpriteType = bodySpriteType;
            clone.headSpriteType = headSpriteType;

            clone._homelessCounter = _homelessCounter;

            return clone;
        }

        public sealed override void AutoStaticDefaults() {
            base.AutoStaticDefaults();

            //The PR is here, and I am loving it 
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.SpawnsWithCustomName[Type] = true;
            NPCID.Sets.AllowDoorInteraction[Type] = true;

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

        public override bool NeedSaving() => true;

        public override void SaveData(TagCompound tag) {
            tag["HeadType"] = headSpriteType;
            tag["BodyType"] = bodySpriteType;
            tag["Shop"] = shopInventory;

            tag["DisplayName"] = NPC.GivenName;
            tag["HomeTileX"] = NPC.homeTileX;
            tag["HomeTileY"] = NPC.homeTileY;
            tag["IsHomeless"] = NPC.homeless;
        }

        public override void LoadData(TagCompound tag) {
            headSpriteType = tag.GetInt("HeadType");
            bodySpriteType = tag.GetInt("BodyType");
            shopInventory = tag.Get<List<ShopItem>>("Shop");

            NPC.GivenName = tag.GetString("DisplayName");
            NPC.homeTileX = tag.GetInt("HomeTileX");
            NPC.homeTileY = tag.GetInt("HomeTileY");
            NPC.homeless = tag.GetBool("IsHomeless");
        }

        public override bool CheckActive() => false;

        public override List<string> SetNPCNameList() => LocalizationUtils.GetAllStringsFromCategory($"VillagerNames.{VillagerType}") is { } names && names.elements.Count > 0
            ? new List<string>() { names.Get() }
            : base.SetNPCNameList();

        public override void SetChatButtons(ref string button, ref string button2) {
            button = Language.GetTextValue("LegacyInterface.28"); //"Shop"
            button2 = LocalizationUtils.GetLWMTextValue("Common.Reputation");
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shopName) {
            //Shop Screen
            if (firstButton) {
                ModContent.GetInstance<ShopUISystem>().OpenShopUI(this);
            }
            //Reputation Screen
            else { }
        }

        public override bool CanChat() => RelationshipStatus != VillagerRelationship.Hate;

        public sealed override string GetChat() {
            if (RelationshipStatus == VillagerRelationship.Hate) {
                return "..."; //The player should be unable to chat with the villagers if they are hated, but just in case that occurs, return something in order to prevent a error thrown
            }

            return DialogueSystem.Instance.GetDialogue(VillagerType, RelationshipStatus, DialogueType.Normal);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            SpriteEffects spriteDirection = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D bodyTexture = bodyAssets[bodySpriteType].Value;
            Texture2D headTexture = headAssets[headSpriteType].Value;

            Rectangle drawArea = new Rectangle((int)(NPC.Right.X - NPC.frame.Width / 1.5 - screenPos.X),
                (int)(NPC.Bottom.Y - NPC.frame.Height - screenPos.Y + 2f),
                NPC.frame.Width,
                NPC.frame.Height);

            spriteBatch.Draw(bodyTexture, drawArea, NPC.frame, drawColor, NPC.rotation, default, spriteDirection, 0);
            spriteBatch.Draw(headTexture, drawArea, NPC.frame, drawColor, NPC.rotation, default, spriteDirection, 0);

            return false;
        }

        public override void PostAI() {
            //We only want this to run on Server/SP
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                return;
            }

            if (NPC.homeless) {
                _homelessCounter++;
            }
            else {
                _homelessCounter = 0;
                return;
            }

            if (_homelessCounter >= 60 * 60 * 2) {
                Color leavingColor = new Color(255, 25, 25);
                string leavingText = LocalizationUtils.GetLWMTextValue($"Event.VillagerLeft.{VillagerType}", new object[] { NPC.GivenOrTypeName });

                NPC.active = false;
                if (Main.netMode == NetmodeID.Server) {
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(leavingText), leavingColor);
                    NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
                }
                else {
                    Main.NewText(leavingText, leavingColor);
                    SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);
                }
            }
        }

        /// <summary>
        /// Restocks the shop of this villager, drawing from the SpawnPool property.
        /// </summary>
        public void RestockShop() {
            shopInventory = new List<ShopItem>();

            int shopLength = Main.rand.Next(6, 8);

            do {
                ShopItem returnedItem = ShopPool;

                if (shopInventory.All(item => item != returnedItem)) {
                    shopInventory.Add(returnedItem);
                }
            }
            while (shopInventory.Count < shopLength);
        }
    }
}