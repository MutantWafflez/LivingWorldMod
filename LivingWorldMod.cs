using LivingWorldMod.ID;
using LivingWorldMod.Items.Materials;
using LivingWorldMod.Items.Placeable.Ores;
using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.UI;
using LivingWorldMod.Utilities;
using LivingWorldMod.Utilities.Quests;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace LivingWorldMod
{
    public class LivingWorldMod : Mod
    {
        internal static bool debugMode = true;

        public static LivingWorldMod mod { get; private set; }

        public static readonly int maximumReputationValue = 200; //The upper cap of the reputation value
        internal static List<VillagerQuest>[] possibleQuests = new List<VillagerQuest>[(int)VillagerType.VillagerTypeCount];
        internal static int[] villageGiftPreferences;

        public LivingWorldMod()
        {
            LWMPacket.RegisterAllHandlers();
            mod = this;
        }

        #region Update Methods

        public override void PostUpdateEverything()
        {
            for (int repIndex = 0; repIndex < (int)VillagerType.VillagerTypeCount; repIndex++)
            {
                if (LWMWorld.reputation[repIndex] > maximumReputationValue)
                {
                    LWMWorld.reputation[repIndex] = maximumReputationValue;
                }
                else if (LWMWorld.reputation[repIndex] < 0)
                {
                    LWMWorld.reputation[repIndex] = 0;
                }
            }
        }

        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
            if (Main.myPlayer == -1 || Main.gameMenu || !Main.LocalPlayer.active)
            {
                return;
            }

            Player myPlayer = Main.player[Main.myPlayer];

            //42.5 block radius around the shrine for the music
            if (myPlayer.Distance(LWMWorld.GetShrineWorldPosition(VillagerType.Harpy)) <= 16 * 95)
            {
                music = GetSoundSlot(SoundType.Music, $"Sounds/Music/HarpyVillage{(Main.dayTime?"Day":"Night")}Music");
                priority = MusicPriority.Environment;
            }
        }

        #endregion Update Methods

        #region Loading

        public override void Load()
        {
            #region Villager Related Method Swaps

            //Allows our villagers to have NPC names despite not having a map head
            On.Terraria.NPC.TypeToHeadIndex += NPC_TypeToHeadIndex;
            //Bypasses the need for the townNPC bool to be true to use the townNPC animation type
            On.Terraria.NPC.VanillaFindFrame += NPC_VanillaFindFrame;
            //Sets the Villager's townNPC value to true only for the duration of the AI method
            On.Terraria.NPC.AI_007_TownEntities += NPC_AI_007_TownEntities;

            #endregion Villager Related Method Swaps

            #region UI Initialization

            if (!Main.dedServ && Main.netMode != NetmodeID.Server)
            {
                HarpyShrineInterface = new UserInterface();
                HarpyShrineState = new ShrineUIState();
                HarpyShrineState.Activate();
                HarpyShrineInterface.SetState(HarpyShrineState);
            }

            #endregion UI Initialization
        }

        public override void PostSetupContent()
        {
            villageGiftPreferences = new int[ItemLoader.ItemCount * (int)VillagerType.VillagerTypeCount];
            InitializeDefaultGiftPreferences();
            InitializeDefaultVillagerQuests();
        }

        public void InitializeDefaultGiftPreferences()
        {
            //Harpy Villagers
            SetGiftValue((int)VillagerType.Harpy, ItemID.Worm, 3);
            SetGiftValue((int)VillagerType.Harpy, ItemID.FallenStar, 5);
            SetGiftValue((int)VillagerType.Harpy, ItemID.Feather, -3);
            SetGiftValue((int)VillagerType.Harpy, ItemID.GiantHarpyFeather, -5);
        }
        
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PacketID type = (PacketID) reader.ReadByte();
            Logger.Debug("Received packet: "+type);
            if(!LWMPacket.TryHandlePacket(type, reader, whoAmI))
                Logger.Error($"Unknown packet type {type}.");
        }

        public void InitializeDefaultVillagerQuests() {

            //List Initialization
            for (int i = 0; i < possibleQuests.Length; i++) {
                possibleQuests[i] = new List<VillagerQuest>();
            }

            #region Harpy Quests
            possibleQuests[(int)VillagerType.Harpy].Add(new HarpyQuest(
                ItemID.HealingPotion,
                "Ouch… That was a pretty big fall, I think I might’ve messed up my wings. How am I supposed to fly with a broken wing? Human, can you bring me a Health Potion to ease the pain? I don’t think I could make it back home in my current state."
                ));
            possibleQuests[(int)VillagerType.Harpy].Add(new HarpyQuest(
                ModContent.ItemType<SkyBudItem>(),
                "Oh my gosh, oh my gosh. I am so doomed right now. I told the other harpies that I would bring back some Skybuds for them, and I can’t go back until I have them! Hey, can you bring me a Skybud real quick? I promise I’ll make it up to you."
                ));
            possibleQuests[(int)VillagerType.Harpy].Add(new HarpyQuest(
                ItemID.EnchantedNightcrawler,
                "Ahh… I’m starving. I just woke up and I’ve had nothing to eat. Oh, I’d do anything for an Enchanted Nightcrawler right about now. They’re a delicacy in our village. Do you think you could catch me a fresh wet, wriggly worm, magically combine it with starpower and bring it to me?!"
                ));
            possibleQuests[(int)VillagerType.Harpy].Add(new HarpyQuest(
                ModContent.ItemType<RoseQuartz>(),
                "I’m aware I just came out of unconsciousness after falling out of the sky and colliding with the hard ground, but I’d love to have a chunk of Rose Quartz! Imagine all the pretty things I could adorn with it! Could you fetch me some Rose Quartz? If it’s not too much trouble, of course."
                ));
            #endregion
        }

        #endregion Loading
        
        #region Mod Compatibility

        /// <summary>
        /// Modifies the gift value of a given item based on VillagerType.
        /// </summary>
        /// <param name="villagerType">The villager type to have their preference changed.</param>
        /// <param name="itemType">The item type that will have its gift value changed.</param>
        /// <param name="value">The new gift value of the given item type. Value between -5 and 5.</param>
        public static void SetGiftValue(VillagerType villagerType, int itemType, int value)
        {
            int index = (int)villagerType * ItemLoader.ItemCount + itemType;
            villageGiftPreferences[index] = Utils.Clamp(value, -5, 5);
        }

        /// <summary>
        /// Returns the gift value a given item type will have on a given villager type.
        /// </summary>
        /// <param name="villagerType">The villager type to find the specific preference of.</param>
        /// <param name="itemType">The type of item to have its reputation modifier checked.</param>
        public static int GetGiftValue(VillagerType villagerType, int itemType)
        {
            int index = (int)villagerType * ItemLoader.ItemCount + itemType;
            return villageGiftPreferences[index];
        }

        #endregion Mod Compatibility

        #region UI

        internal static UserInterface HarpyShrineInterface;
        internal static ShrineUIState HarpyShrineState;
        private GameTime _lastUpdateUiGameTime;

        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            if (HarpyShrineInterface?.CurrentState != null)
                HarpyShrineInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            //https://github.com/tModLoader/tModLoader/wiki/Vanilla-Interface-layers-values
            int interfaceLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interface Logic 1"));
            if (interfaceLayer != -1)
            {
                layers.Insert(interfaceLayer, new LegacyGameInterfaceLayer(
                    "LWM: HarpyShrine",
                    delegate
                    {
                        if (_lastUpdateUiGameTime != null && HarpyShrineInterface?.CurrentState != null)
                            HarpyShrineInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);

                        return true;
                    },
                       InterfaceScaleType.UI));
            }
        }

        #endregion UI

        #region Method Swaps

        private int NPC_TypeToHeadIndex(On.Terraria.NPC.orig_TypeToHeadIndex orig, int type)
        {
            if (type == ModContent.NPCType<SkyVillager>())
            {
                return type;
            }
            return orig(type);
        }

        private void NPC_AI_007_TownEntities(On.Terraria.NPC.orig_AI_007_TownEntities orig, NPC self)
        {
            if (LWMUtils.IsTypeOfVillager(self))
            {
                self.townNPC = true;
                self.homeTileX = (int)((Villager)self.modNPC).homePosition.X;
                self.homeTileY = (int)((Villager)self.modNPC).homePosition.Y;
            }
            else if (LWMUtils.IsTypeOfQuestVillager(self))
            {
                self.townNPC = true;
                self.homeTileX = (int)(self.position.X / 16);
                self.homeTileY = (int)(self.position.Y / 16);
            }
            orig(self);
            if (LWMUtils.IsTypeOfVillager(self) || LWMUtils.IsTypeOfQuestVillager(self))
            {
                self.townNPC = false;
            }
        }

        private void NPC_VanillaFindFrame(On.Terraria.NPC.orig_VanillaFindFrame orig, NPC self, int frameHeight)
        {
            if (LWMUtils.IsTypeOfVillager(self) || LWMUtils.IsTypeOfQuestVillager(self))
            {
                self.townNPC = true;
            }
            orig(self, frameHeight);
            if (LWMUtils.IsTypeOfVillager(self) || LWMUtils.IsTypeOfQuestVillager(self))
            {
                self.townNPC = false;
            }
        }

        #endregion Method Swaps
    }
}