using Terraria.ModLoader;

namespace LivingWorldMod.Commands
{
	public class RefreshShops : ModCommand
	{
		public override string Command => "rshops";

		public override CommandType Type => CommandType.World;

		public override string Description => "Refreshes the daily shops of all NPCs with daily shops, as if a new day passed.";

		public override string Usage => "/rshops";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			ModContent.GetInstance<LWMWorld>().RefreshDailyShops();
			caller.Reply("Daily shops have been refreshed.");
		}
	}
}