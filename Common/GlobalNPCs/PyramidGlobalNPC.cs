﻿using System.Collections.Generic;
using LivingWorldMod.Content.StatusEffects.Debuffs;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Nearsightedness:
                        if (Main.LocalPlayer.Center.Distance(npc.Center) >= 16 * 14f) {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        public override bool CheckDead(NPC npc) {
            foreach (PyramidRoomCurseType curse in CurrentCurses) {
                switch (curse) {
                    case PyramidRoomCurseType.Pacifism:
                        Player closestPlayer = null;
                        float closestDistance = float.MaxValue;

                        for (int i = 0; i < Main.maxPlayers; i++) {
                            Player player = Main.player[i];

                            if (!player.dead && player.Center.Distance(npc.Center) is float distance && distance < closestDistance) {
                                closestDistance = distance;
                                closestPlayer = player;
                            }
                        }

                        closestPlayer?.AddBuff(ModContent.BuffType<PacifistPlight>(), 60 * 5);
                        break;
                }
            }

            return true;
        }
    }
}