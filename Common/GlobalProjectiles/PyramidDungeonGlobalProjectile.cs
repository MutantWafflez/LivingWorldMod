using System.Collections.Generic;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalProjectiles {
    /// <summary>
    /// Global Projectile that only functions in the Pyramid Subworld Dungeon.
    /// </summary>
    public class PyramidDungeonGlobalProjectile : GlobalProjectile {
        public override bool InstancePerEntity => true;

        /// <summary>
        /// Reference to this projectile's current list of curses in accordance to
        /// the room they are in.
        /// </summary>
        public List<PyramidRoomCurseType> CurrentCurses => currentRoom?.ActiveCurses ?? new List<PyramidRoomCurseType>();

        public PyramidRoom currentRoom;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => PyramidSubworld.IsInSubworld;

        public override void OnSpawn(Projectile projectile, IEntitySource source) {
            currentRoom = ModContent.GetInstance<PyramidSubworld>().grid.GetEntityCurrentRoom(projectile);

            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.DodgeOrDie:
                        if (projectile.hostile && projectile.velocity.Length() > 0f) {
                            projectile.velocity *= 2;
                        }
                        break;
                }
            }
        }
    }
}