using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalNPCs {
    /// <summary>
    /// Simple GlobalNPC that prevents spawning if the player is within a village.
    /// </summary>
    public class VillageSpawnPreventionNPC : GlobalNPC {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
            if (WorldCreationSystem.Instance.villageZones.Any(zone => zone.Contains(spawnInfo.Player.Center.ToTileCoordinates()))) {
                pool[0] = 0;
            }
        }
    }
}