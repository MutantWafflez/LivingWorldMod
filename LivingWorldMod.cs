using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.ModLoader;
using LivingWorldMod.UI;
using LivingWorldMod.Utilities;
using LivingWorldMod.NPCs.Villagers;

namespace LivingWorldMod
{
    public class LivingWorldMod : Mod
    {
        internal static bool debugMode = true;

        public static readonly int maximumReputationValue = 200; //The upper cap of the reputation value
        internal static int[] villageGiftPreferences;

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

            if (myPlayer.IsWithinRangeOfNPC(ModContent.NPCType<SkyVillager>(), 16 * 75))
            {
                music = GetSoundSlot(SoundType.Music, "Sounds/Music/SkyVillageMusic");
                priority = MusicPriority.Environment;
            }
        }
        #endregion

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
            #endregion

            #region UI Initialization
            if (!Main.dedServ && Main.netMode != NetmodeID.Server)
            {
                HarpyShrineInterface = new UserInterface();
                HarpyShrineState = new ShrineUIState();
                HarpyShrineState.Activate();
                HarpyShrineInterface.SetState(HarpyShrineState);
            }
            #endregion
        }

        public override void PostSetupContent() 
        {
            villageGiftPreferences = new int[ItemLoader.ItemCount * (int) VillagerType.VillagerTypeCount];
            InitializeDefaultGiftPreferences();
        }

        public void InitializeDefaultGiftPreferences() 
        {
            //Harpy Villagers
            SetGiftValue((int)VillagerType.Harpy, ItemID.Worm, 3);
            SetGiftValue((int)VillagerType.Harpy, ItemID.FallenStar, 5);
            SetGiftValue((int)VillagerType.Harpy, ItemID.Feather, -3);
            SetGiftValue((int)VillagerType.Harpy, ItemID.GiantHarpyFeather, -5);
        }

        #endregion

        #region Mod Compatibility
        /// <summary>
        /// Modifies the gift value of a given item based on VillagerType.
        /// </summary>
        /// <param name="villagerType">The villager type to have their preference changed.</param>
        /// <param name="itemType">The item type that will have its gift value changed.</param>
        /// <param name="value">The new gift value of the given item type. Value between -5 and 5.</param>
        public static void SetGiftValue(VillagerType villagerType, int itemType, int value)
        {
            int index = (int) villagerType * ItemLoader.ItemCount + itemType;
            villageGiftPreferences[index] = Utils.Clamp(value, -5, 5);
        }

        /// <summary>
        /// Returns the gift value a given item type will have on a given villager type.
        /// </summary>
        /// <param name="villagerType">The villager type to find the specific preference of.</param>
        /// <param name="itemType">The type of item to have its reputation modifier checked.</param>
        public static int GetGiftValue(VillagerType villagerType, int itemType)
        {
            int index = (int) villagerType * ItemLoader.ItemCount + itemType;
            return villageGiftPreferences[index];
        }
        #endregion

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
                       InterfaceScaleType.Game));
            }
        }
        #endregion

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
            orig(self);
            if (LWMUtils.IsTypeOfVillager(self))
            {
                self.townNPC = false;
            }
        }

        private void NPC_VanillaFindFrame(On.Terraria.NPC.orig_VanillaFindFrame orig, NPC self, int frameHeight)
        {
            if (LWMUtils.IsTypeOfVillager(self))
            {
                self.townNPC = true;
            }
            orig(self, frameHeight);
            if (LWMUtils.IsTypeOfVillager(self))
            {
                self.townNPC = false;
            }
        }
        #endregion
    }
}
