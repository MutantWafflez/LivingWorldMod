using LivingWorldMod.NPCs.Villagers;
using LivingWorldMod.Utils;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Reflection;
using static Mono.Cecil.Cil.OpCodes;
using Microsoft.Xna.Framework.Graphics;
using LivingWorldMod.UI;

namespace LivingWorldMod
{
    public class LivingWorldMod : Mod
    {
        internal static bool debugMode = false;
        internal static readonly int villageGiftCooldownTime = 60 * 60 * 24; //24 IRL minutes (24 in game hours)

        public override void PostUpdateEverything()
        {
            for (int repIndex = 0; repIndex < LWMWorld.villageReputation.Length; repIndex++)
            {
                if (LWMWorld.villageReputation[repIndex] > 100)
                    LWMWorld.villageReputation[repIndex] = 100;
                else if (LWMWorld.villageReputation[repIndex] < -100)
                    LWMWorld.villageReputation[repIndex] = -100;
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
        }

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