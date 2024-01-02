using System;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Commands.DebugCommands;

public class ReputationCommand : DebugCommand {
    public override string Command => "rep";

    public override string Description => "Modifies a given village's reputation. Cannot go below -100 or above 100.";

    public override string Usage => "/rep <VillageType> [value]";

    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        if (args.Length == 2 && int.TryParse(args[0], out int type)) {
            if (type >= 0 && type < NPCUtils.GetTotalVillagerTypeCount()) {
                if (int.TryParse(args[1], out int repValue)) {
                    if (Math.Abs(repValue) > ReputationSystem.VillageReputationConstraint) {
                        throw new UsageException("Inputted reputation value is greater than 100 or less than -100.");
                    }
                    ReputationSystem.Instance.SetVillageReputation((VillagerType)type, repValue);
                    caller.Reply("Village type " + (VillagerType)type + "'s reputation successfully changed to " + repValue);
                }
                else {
                    throw new UsageException("Reputation value was not an Integer.");
                }
            }
            else {
                throw new UsageException("Did not input correct Village Type: " + type);
            }
        }
        else {
            throw new UsageException("Did not input a Reputation Value or Village Type.");
        }
    }
}