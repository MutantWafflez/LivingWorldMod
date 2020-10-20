using LivingWorldMod.NPCs.Villagers;
using System;
using Terraria.ModLoader;

namespace LivingWorldMod.Commands
{
    public class ReputationTweak : ModCommand
    {
        public override string Command => "rep";

        public override CommandType Type => CommandType.World;

        public override string Description => $"Modifies a given village's reputation. Cannot go below -{LivingWorldMod.maximumReputationValue} or above {LivingWorldMod.maximumReputationValue}.";

        public override string Usage => "/rep <VillageType> [value]" + "\nVillageType Legend: 0 = Sky, 1 = Lihzahrd";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 2 && int.TryParse(args[0], out int type))
            {
                if (type >= 0 && type < (int)VillagerType.VillagerTypeCount)
                {
                    if (int.TryParse(args[1], out int repValue))
                    {
                        if (Math.Abs(repValue) > LivingWorldMod.maximumReputationValue)
                        {
                            throw new UsageException($"Inputted reputation value is greater than {LivingWorldMod.maximumReputationValue} or less than -{LivingWorldMod.maximumReputationValue}.");
                        }
                        LWMWorld.villageReputation[type] = repValue;
                        caller.Reply("Village type " + (VillagerType)type + "'s reputation successfully changed to " + repValue);
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
