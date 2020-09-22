using LivingWorldMod.NPCs.Villagers;
using System;
using Terraria.ModLoader;

namespace LivingWorldMod.Commands
{
    public class ReputationTweak : ModCommand
    {
        public override string Command => "rep";

        public override CommandType Type => CommandType.World;

        public override string Description => "Modifies a given village's reputation. Cannot go below -100 or above 100.";

        public override string Usage => "/rep <VillageType> [value]" + "\nVillageType Legend: 0 = Sky, 1 = Lihzahrd";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 2 && int.TryParse(args[0], out int type))
            {
                if (type >= 0 && type < (int)VillagerType.VillagerTypeCount)
                {
                    if (int.TryParse(args[1], out int repValue))
                    {
                        if (Math.Abs(repValue) > 100)
                        {
                            throw new UsageException("Inputted reputation value is greater than 100 or less than -100.");
                        }
                        LWMWorld.villageReputation[type] = repValue;
                        caller.Reply("Village type " + type + "'s reputation successfully changed to " + repValue);
                    }
                    else
                    {
                        throw new UsageException("Reputation value was not an Integer.");
                    }
                }
                else
                {
                    throw new UsageException("Did not input correct Village Type: " + type);
                }
            }
            else
            {
                throw new UsageException("Did not input a Reputation Value or Village Type.");
            }
        }

    }
}
