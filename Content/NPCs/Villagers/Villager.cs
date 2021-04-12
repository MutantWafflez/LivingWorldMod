using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Common.GlobalItems;
using LivingWorldMod.Common.Players;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Interfaces;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace LivingWorldMod.Content.NPCs.Villagers
{
    public abstract class Villager : ModNPC, IBinarySerializable
    {
        public static readonly string VILLAGER_SPRITE_PATH = nameof(LivingWorldMod) + "/Assets/Textures/NPCs/Villagers/";

        public readonly List<string> possibleNames;

        public bool isHatedRep;
        public bool isNegativeRep;
        public bool isNeutralRep = true; //This is set to true prematurely *just* in case UpdateReputationBools() isn't called.
        public bool isPositiveRep;
        public bool isMaxRep;

        public Vector2 homePosition;

        public bool isMerchant = true;

        // record of shop data for each player that opened the shop
        internal Dictionary<Guid, List<ShopItem>> shops;

        // shop template for all players, at full stock
        private List<ShopItem> dailyShop;

        internal abstract VillagerID VillagerType { get; }

        #region Sprite Properties

        private int _bodySprite;
        private Texture2D _bodyTexture;
        private int _hairSprite;
        private Texture2D _hairTexture;
        public override string Texture => VILLAGER_SPRITE_PATH + VillagerType + "/Body1";

        /*public override string[] AltTextures => new string[] {
            VILLAGER_SPRITE_PATH + VillagerType + "Style2",
            VILLAGER_SPRITE_PATH + VillagerType + "Style3"
        };*/

        protected abstract int SpriteVariationCount { get; }

        private int BodySprite
        {
            get => _bodySprite;
            set
            {
                if (_bodySprite == value) return;
                _bodySprite = value < 0 ? Main.rand.Next(0, SpriteVariationCount) : value;
                _bodyTexture = null;
            }
        }

        private Texture2D BodyTexture
        {
            get
            {
                if (_bodyTexture == null)
                    _bodyTexture = ModContent.GetTexture(VILLAGER_SPRITE_PATH + VillagerType + "/Body" + (_bodySprite + 1));
                return _bodyTexture;
            }
            set => _bodyTexture = value;
        }

        private int HairSprite
        {
            get => _hairSprite;
            set
            {
                if (_hairSprite == value) return;
                _hairSprite = value < 0 ? Main.rand.Next(0, SpriteVariationCount) : value;
                _hairTexture = null;
            }
        }

        private Texture2D HairTexture
        {
            get
            {
                if (_hairTexture == null)
                    _hairTexture = ModContent.GetTexture(VILLAGER_SPRITE_PATH + VillagerType + "/Hair" + (_hairSprite + 1));
                return _hairTexture;
            }
            set => _hairTexture = value;
        }

        #endregion Sprite Properties

        public Villager()
        {
            possibleNames = GetPossibleNames();
            shops = new Dictionary<Guid, List<ShopItem>>();
        }

        #region Defaults Methods

        public override bool CloneNewInstances => true;

        public override ModNPC Clone()
        {
            Villager clonedNPC = (Villager)base.Clone();
            clonedNPC.BodySprite = -1; // picks random
            clonedNPC.HairSprite = -1; // picks random
            clonedNPC.RefreshDailyShop();
            return clonedNPC;
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.ExtraTextureCount[npc.type] = 0;
        }

        public override void SetDefaults()
        {
            npc.width = 25;
            npc.height = 40;
            npc.friendly = true;
            npc.lifeMax = 500;
            npc.defense = 15;
            npc.knockBackResist = 0.5f;
            npc.aiStyle = 7;
            animationType = NPCID.Guide;

            RefreshDailyShop();
        }

        /*public override ModNPC NewInstance(NPC npcClone)
        {
            ModNPC modnpc = base.NewInstance(npcClone);
            // send one-time info about new NPC to sync custom data
            if(Main.netMode == NetmodeID.Server)
                new VillagerData(modnpc as Villager).SendToAll(mod);
            return modnpc;
        }*/

        #endregion Defaults Methods

        #region Update Methods

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            SpriteEffects spriteDirection = npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(BodyTexture, new Rectangle((int)(npc.Right.X - (npc.frame.Width / 1.5) - Main.screenPosition.X), (int)(npc.Bottom.Y - npc.frame.Height - Main.screenPosition.Y + 2f), npc.frame.Width, npc.frame.Height), npc.frame, drawColor, npc.rotation, default, spriteDirection, 0);
            spriteBatch.Draw(HairTexture, new Rectangle((int)(npc.Right.X - (npc.frame.Width / 1.5) - Main.screenPosition.X), (int)(npc.Bottom.Y - npc.frame.Height - Main.screenPosition.Y + 2f), npc.frame.Width, npc.frame.Height), npc.frame, drawColor, npc.rotation, default, spriteDirection, 0);
            return false;
        }

        public override bool CheckActive() => false;

        public override void PostAI()
        {
            UpdateReputationBools();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            // for now, write this info every time, until a better solution is found
            Write(writer);
            // send shop lists
            writer.Write(dailyShop != null);
            if (dailyShop == null)
                return;
            IOUtils.WriteList(writer, dailyShop);
            IOUtils.WriteDictionary(writer, shops, (w, id) => w.Write(id.ToString()), IOUtils.WriteList);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            // read static info
            Read(reader);
            // read shop lists
            bool read = reader.ReadBoolean();
            if (!read)
                return;
            dailyShop = IOUtils.ReadList<ShopItem>(reader);
            shops = IOUtils.ReadDictionary(reader, w => Guid.Parse(w.ReadString()), IOUtils.ReadList<ShopItem>);
        }

        #endregion Update Methods

        #region Serialization Methods

        public static void LoadVillager(TagCompound tag)
        {
            VillagerID villagerType = (VillagerID)tag.GetAsInt("type");
            int npcType;
            switch (villagerType)
            {
            case VillagerID.Harpy:
            default:
                npcType = ModContent.NPCType<SkyVillager>();
                break;
            }
            int npcIndex = NPC.NewNPC((int)tag.GetFloat("x"), (int)tag.GetFloat("y"),
                npcType);
            NPC npcAtIndex = Main.npc[npcIndex];
            npcAtIndex.GivenName = tag.GetString("name");
            Villager villager = (Villager)npcAtIndex.modNPC;
            villager.BodySprite = tag.TryGet<int>("mainSprite", -1);
            villager.HairSprite = tag.TryGet<int>("secondarySprite", -1);
            villager.homePosition = tag.Get<Vector2>("homePos");
            if (tag.ContainsKey("shop_template"))
            {
                villager.dailyShop = tag.GetList<ShopItem>("shop_template").ToList();
                villager.shops = IOUtils.LoadDictionary<Guid, List<ShopItem>, string>(tag, "shops", Guid.Parse);
            }
        }

        public void Write(BinaryWriter writer, byte syncMode = default)
        {
            writer.Write(npc.GivenName);
            writer.Write((byte)BodySprite);
            writer.Write((byte)HairSprite);
        }

        public void Read(BinaryReader reader, byte syncMode = default)
        {
            npc.GivenName = reader.ReadString();
            BodySprite = reader.ReadByte();
            HairSprite = reader.ReadByte();
        }

        public TagCompound Save()
        {
            TagCompound tag = new TagCompound
            {
                {"type", (int) VillagerType},
                {"mainSprite", BodySprite},
                {"secondarySprite", HairSprite},
                {"x", npc.Center.X},
                {"y", npc.Center.Y},
                {"name", npc.GivenName},
                {"homePos", homePosition}
            };
            if (dailyShop != null)
            {
                tag.Add("shop_template", dailyShop);
                // record the shop state for each player that opened the shop
                IOUtils.SaveDictionary(tag, "shops", shops, id => id.ToString());
            }

            return tag;
        }

        #endregion Serialization Methods

        #region Chat Methods

        public override bool CanChat() => true;

        public override string GetChat() => GetDialogueText();

        public override string TownNPCName() => possibleNames[WorldGen.genRand.Next(possibleNames.Count)];

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (isMerchant)
            {
                button = Language.GetTextValue("LegacyInterface.28");
                button2 = "Reputation";
            }
            else
            {
                button = "Reputation";
            }
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (firstButton && isMerchant)
            {
                shop = true;
            }
            else if (firstButton && !isMerchant)
            {
                Main.npcChatText = GetReputationText();
            }
            else if (!firstButton && isMerchant)
            {
                Main.npcChatText = GetReputationText();
            }
            else
            {
                shop = true;
            }
        }

        /// <summary>
        /// Method used to determine what is said to the player upon right click.
        /// </summary>
        /// <returns> Returns a value telling the player to contact a mod dev by default. </returns>
        public virtual WeightedRandom<string> GetDialogueText()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();
            chat.Add("If someone saw this text... I'd be scared and tell a mod dev immediately!");
            return chat;
        }

        /// <summary>
        /// Method used to determine what is said to the player based on the Village reputation upon
        /// pressing the Reputation button.
        /// </summary>
        /// <returns> Returns a value telling the player to contact a mod dev by default. </returns>
        public virtual WeightedRandom<string> GetReputationText()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();
            chat.Add("If someone saw this text... I'd be scared and tell a mod dev immediately!");
            return chat;
        }

        #endregion Chat Methods

        #region Shop Handling

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            Player player = Main.LocalPlayer;

            int npcId = player.talkNPC;
            NPC npc = npcId < 0 ? null : Main.npc[npcId];

            if (npc == null)
                return; // nothing we do at this point will make sense if the npc is null, so just return

            if (this.npc != npc)
            {
                // This deserves some explanation. All town-related actions are called only on one
                // townNPC, the one returned from NPCLoader.GetNPC(type). This in all likelihood
                // isn't even in the npc array. Regardless, we need to call SetupShop on the correct
                // Villager instance, the one we are talking with, hence we check player.talkNPC and
                // get the villager from that.
                ((Villager)npc.modNPC).SetupShop(shop, ref nextSlot);
                return;
            }

            if (dailyShop == null)
                return;

            LWMPlayer modPlayer = player.GetModPlayer<LWMPlayer>();
            Guid id = modPlayer.guid;
            List<ShopItem> playerShop = GetPlayerShop(id);

            float rep = LWMWorld.GetReputation(VillagerType);
            foreach (ShopItem itemSlot in playerShop)
            {
                if (!itemSlot.CanPurchase(modPlayer, rep))
                    continue;

                Item item = shop.item[nextSlot];
                itemSlot.Apply(item);
                item.buyOnce = true;
                LWMGlobalShopItem globalItem = item.GetGlobalItem<LWMGlobalShopItem>();
                // give the item a reference to the ShopItem so we can track inventory changes
                globalItem.SetPersistentStack(itemSlot);
                globalItem.isOriginalShopSlot = true;
                globalItem.isOutOfStock = item.stack <= 0;
                // set stack size to 1 so it doesn't remove the item completely
                if (item.stack <= 0) item.stack = 1;
                ++nextSlot;
            }
        }

        public List<ShopItem> GetPlayerShop(Guid playerId)
        {
            // fetch the shop for this player
            shops.TryGetValue(playerId, out List<ShopItem> playerShop);
            if (playerShop == null)
            {
                // clone the daily shop template
                playerShop = dailyShop.Select(item => item.Clone()).ToList();
                shops.Add(playerId, playerShop);
            }

            return playerShop;
        }

        public void RefreshDailyShop()
        {
            // create a new shop
            WeightedRandom<ShopItem> pitems = new WeightedRandom<ShopItem>(Main.rand.Next());
            int itemCount = GenerateDailyShop(pitems);
            List<ShopItem> items = new List<ShopItem>();
            for (int i = 0; i < itemCount; i++)
            {
                ShopItem item = pitems.Get();
                pitems.elements.Remove(pitems.elements.Find(tuple => tuple.Item1 == item));
                pitems.needsRefresh = true;
                items.Add(item);
            }
            dailyShop = items;

            // clear purchase history
            shops.Clear();
            // flag the npc for an update
            if (Main.netMode == NetmodeID.Server)
                npc.netUpdate = true;
        }

        /// <summary>
        /// Generates a weighted random of the possible items in the shop.
        /// </summary>
        /// <param name="items"> The object to add the items to </param>
        /// <returns>
        /// How many items to take out of the weighted random, to put in the actual shop.
        /// </returns>
        protected virtual int GenerateDailyShop(WeightedRandom<ShopItem> items)
        {
            return 0;
        }

        #endregion Shop Handling

        #region Miscellaneous Methods

        public virtual List<string> GetPossibleNames()
        {
            List<string> names = new List<string> {
                "Villager (Report to Mod Dev!)"
            };
            return names;
        }

        private void UpdateReputationBools()
        {
            float reputation = LWMWorld.GetReputation(VillagerType);
            if (reputation <= 0f)
            {
                isHatedRep = true;
                isNegativeRep = false;
                isNeutralRep = false;
                isPositiveRep = false;
                isMaxRep = false;
            }
            else if (reputation <= 50f && reputation > 0f)
            {
                isHatedRep = false;
                isNegativeRep = true;
                isNeutralRep = false;
                isPositiveRep = false;
                isMaxRep = false;
            }
            else if (reputation >= 100f && reputation <= 150f)
            {
                isHatedRep = false;
                isNegativeRep = false;
                isNeutralRep = true;
                isPositiveRep = false;
                isMaxRep = false;
            }
            else if (reputation > 150f && reputation < 200f)
            {
                isHatedRep = false;
                isNegativeRep = false;
                isNeutralRep = false;
                isPositiveRep = true;
                isMaxRep = false;
            }
            else if (reputation >= 200f)
            {
                isHatedRep = false;
                isNegativeRep = false;
                isNeutralRep = false;
                isPositiveRep = false;
                isMaxRep = true;
            }
        }

        #endregion Miscellaneous Methods
    }
}