using LivingWorldMod.Content.Tiles.Interactables;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Patches {

    /// <summary>
    /// Class that handles patches for specifically Pylons.
    /// </summary>
    public class PylonPatches : ILoadable {
        public void Load(Mod mod) {
            On.Terraria.GameContent.TeleportPylonsSystem.IsPlayerNearAPylon += PlayerNearPylon;
        }

        public void Unload() { }

        private bool PlayerNearPylon(On.Terraria.GameContent.TeleportPylonsSystem.orig_IsPlayerNearAPylon orig, Terraria.Player player) {
            // Count waystones as "pylons" for teleportation
            return player.IsTileTypeInInteractionRange(ModContent.TileType<WaystoneTile>()) || orig(player);
        }
    }
}
