using LivingWorldMod.Common.Configs;
using LivingWorldMod.Content.Subworlds.Pyramid;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Players.DebugPlayers {
    /// <summary>
    /// ModPlayer that exists for Debug purposes. Only loads in Debug mode.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class DebugPlayer : ModPlayer {
        private bool _alreadyEnteredSubworld;

        public override bool IsLoadingEnabled(Mod mod) => LivingWorldMod.IsDebug;

        public override void OnEnterWorld(Player player) {
            if (ModContent.GetInstance<DebugConfig>().pyramidDebug && !_alreadyEnteredSubworld) {
                _alreadyEnteredSubworld = true;
                SubworldSystem.Enter<PyramidSubworld>();
            }
        }
    }
}