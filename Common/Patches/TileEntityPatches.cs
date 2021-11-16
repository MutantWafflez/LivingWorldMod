using LivingWorldMod.Content.Tiles.Interactables;
using Terraria;
using Terraria.ModLoader;
using On.Terraria.GameContent.Tile_Entities;
using Terraria.GameContent;

namespace LivingWorldMod.Common.Patches {

    /// <summary>
    /// Class that handles patches for vanilla Tile Entities.
    /// </summary>
    public class TileEntityPatches : ILoadable {
        public void Load(Mod mod) {
            TETeleportationPylon.IsTileValidForEntity += PylonValidityCheck;
            TETeleportationPylon.TryGetPylonTypeFromTileCoords += PlacedTileToPylonType;
            On.Terraria.GameContent.TeleportPylonsSystem.HasPylonOfType += CheckForExisingPylonType;
            On.Terraria.GameContent.TeleportPylonsSystem.HowManyNPCsDoesPylonNeed += NPCRequirementForPylon;
            On.Terraria.GameContent.TeleportPylonsSystem.IsPlayerNearAPylon += PlayerNearPylon;
        }

        private int NPCRequirementForPylon(On.Terraria.GameContent.TeleportPylonsSystem.orig_HowManyNPCsDoesPylonNeed orig, TeleportPylonsSystem self, TeleportPylonInfo info, Player player) {
            // Waystones don't need NPCs, so we can remove that requirement here
            return info.TypeOfPylon == (TeleportPylonType)10 ? 0 : orig(self, info, player);
        }

        public void Unload() { }

        private bool PylonValidityCheck(TETeleportationPylon.orig_IsTileValidForEntity orig, Terraria.GameContent.Tile_Entities.TETeleportationPylon self, int x, int y) {
            // Add waystones to be valid for pylon tile entities
            return Main.tile[x, y].type == ModContent.TileType<WaystoneTile>() || orig(self, x, y);
        }

        private bool PlacedTileToPylonType(TETeleportationPylon.orig_TryGetPylonTypeFromTileCoords orig, Terraria.GameContent.Tile_Entities.TETeleportationPylon self, int x, int y, out TeleportPylonType pylonType) {
            // Add waystones under the special type "Waystone" or 10
            if (Main.tile[x, y + 1].type == ModContent.TileType<WaystoneTile>()) {
                pylonType = (TeleportPylonType)10;
                return true;
            }

            return orig(self, x, y, out pylonType);
        }

        private bool CheckForExisingPylonType(On.Terraria.GameContent.TeleportPylonsSystem.orig_HasPylonOfType orig, TeleportPylonsSystem self, TeleportPylonType pylonType) {
            // If the tile type is for a waystone, we can have an infinite amount of them, so return false even if multiple exist
            return pylonType != (TeleportPylonType)10 && orig(self, pylonType);
        }

        private bool PlayerNearPylon(On.Terraria.GameContent.TeleportPylonsSystem.orig_IsPlayerNearAPylon orig, Player player) {
            //Add waystones as a choice to be near pylons
            return player.IsTileTypeInInteractionRange(ModContent.TileType<WaystoneTile>()) || orig(player);
        }
    }
}
