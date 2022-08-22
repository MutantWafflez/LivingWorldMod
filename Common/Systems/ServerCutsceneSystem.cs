using LivingWorldMod.Common.Players;
using LivingWorldMod.Common.Systems.BaseSystems;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Systems {
    /// <summary>
    /// ModSystem loaded exclusively on the server side to update 
    /// </summary>
    [Autoload(Side = ModSide.Server)]
    public class ServerCutsceneSystem : BaseModSystem<ServerCutsceneSystem> {
        public override void PostUpdateEverything() {
            for (int i = 0; i < Main.maxPlayers; i++) {
                Player player = Main.player[i];
                if (!player.active || !player.TryGetModPlayer(out CutscenePlayer cutscenePlayer) || !cutscenePlayer.InCutscene) {
                    continue;
                }

                cutscenePlayer.CurrentCutscene.Update(player);
                if (cutscenePlayer.CurrentCutscene.IsFinished) {
                    cutscenePlayer.EndCutscene(true);
                }
            }
        }
    }
}