using LivingWorldMod.NPCs.Villagers;
using Terraria;
using LivingWorldMod.Utils;
using Terraria.ModLoader;

namespace LivingWorldMod
{
	public class LivingWorldMod : Mod
	{
        internal static bool debugMode = false;

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
    }
}