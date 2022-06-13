using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TileEntities.Interactables;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalNPCs {
    /// <summary>
    /// Simple GlobalNPC that prevents spawning if the player is within a village.
    /// </summary>
    public class VillageSpawnPreventionNPC : GlobalNPC {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
            if (TileEntity.ByID.Values.OfType<VillageShrineEntity>().ToList().Any(shrine => shrine.villageZone.ContainsPoint(spawnInfo.Player.Center))) {
                pool[0] = 0;
            }
        }
    }
}