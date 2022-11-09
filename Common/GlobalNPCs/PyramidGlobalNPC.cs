using System.Collections.Generic;
using LivingWorldMod.Content.Subworlds.Pyramid;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalNPCs {
    /// <summary>
    /// GlobalNPC that only applies to NPCs in the Pyramid Subworld.
    /// </summary>
    public class PyramidGlobalNPC : GlobalNPC {
        public PyramidRoom currentRoom;

        /// <summary>
        /// The rate of double updates. Defaults to 0.
        /// </summary>
        public float doubleUpdateRate;

        /// <summary>
        /// How many doubleUpdateRates have triggered so far. When it reaches
        /// 1 (or 100%), the NPC will double update.
        /// </summary>
        public float doubleUpdateTimer;

        /// <summary>
        /// Reference to this NPC's current list of curses in accordance to
        /// the room they are in.
        /// </summary>
        public List<PyramidRoomCurseType> CurrentCurses => currentRoom?.roomCurses ?? new List<PyramidRoomCurseType>();

        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => SubworldSystem.IsActive<PyramidSubworld>();

        public override void OnSpawn(NPC npc, IEntitySource source) {
            currentRoom = ModContent.GetInstance<PyramidSubworld>().Grid.GetEntityCurrentRoom(npc);
        }

        public override bool PreAI(NPC npc) {
            doubleUpdateRate = 0f;

            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Hyperactivity:
                        doubleUpdateRate += 0.5f;
                        break;
                }
            }

            doubleUpdateTimer += doubleUpdateRate;
            if (doubleUpdateTimer >= 1f) {
                doubleUpdateTimer = 0f;

                npc.AI();
            }

            return true;
        }
    }
}