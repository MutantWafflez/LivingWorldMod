using Terraria.ModLoader;

namespace LivingWorldMod
{
	public class LivingWorldMod : Mod
	{
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
    }
}