using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.NPCs.Hostile.Pyramid {
    /// <summary>
    /// Abstract class that is defined to be the base class of all monsters
    /// within the Pyramid Dungeon Subworld.
    /// </summary>
    public abstract class PyramidDungeonModNPC : ModNPC {
        /// <summary>
        /// The spawn weight of this NPC. Defaults to 1. Denotes how "heavy" this
        /// NPC is in terms of how much room spawn weight it takes away when it spawns.
        /// </summary>
        public virtual int SpawnWeight => 1;

        /// <summary>
        /// The spawn chance weight; the actual chances of this NPC being spawned in the first
        /// place in relation to all other NPCs.
        /// </summary>
        public virtual float SpawnChanceWeight => 1f;

        public PyramidGlobalNPC GlobalNPC => NPC.GetGlobalNPC<PyramidGlobalNPC>();

        public override float SpawnChance(NPCSpawnInfo spawnInfo) {
            if (
                !PyramidSubworld.IsInSubworld
                || ModContent.GetInstance<PyramidSubworld>().grid.GetRoomFromTilePosition(new Point(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY)) is not { IsActive: true } room
                || room.remainingEnemySpawnWeight < SpawnWeight) {
                return 0f;
            }
            room.remainingEnemySpawnWeight -= SpawnWeight;

            return SpawnChanceWeight;
        }
    }
}