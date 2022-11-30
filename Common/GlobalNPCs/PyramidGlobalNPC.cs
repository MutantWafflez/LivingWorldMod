using System;
using System.Collections.Generic;
using LivingWorldMod.Content.StatusEffects.Debuffs;
using LivingWorldMod.Content.Subworlds.Pyramid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.GlobalNPCs {
    /// <summary>
    /// GlobalNPC that only applies to NPCs in the Pyramid Subworld.
    /// </summary>
    public class PyramidGlobalNPC : GlobalNPC {
        public override bool InstancePerEntity => true;

        /// <summary>
        /// Reference to this NPC's current list of curses in accordance to
        /// the room they are in.
        /// </summary>
        public List<PyramidRoomCurseType> CurrentCurses => currentRoom?.roomCurses ?? new List<PyramidRoomCurseType>();

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

        private bool _spriteBatchNeedsRestart;
        private bool _isBeingSpawned;
        private int _spawnAnimTimer;

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => SubworldSystem.IsActive<PyramidSubworld>();

        public override void SetStaticDefaults() {
            if (Main.netMode != NetmodeID.Server) {
                GameShaders.Misc["PyramidNPCSpawn"] = new MiscShaderData(new Ref<Effect>(Mod.Assets.Request<Effect>("Assets/Shaders/NPCs/PyramidNPCSpawn", AssetRequestMode.ImmediateLoad).Value), "PyramidNPCSpawn");
            }
        }

        public override void OnSpawn(NPC npc, IEntitySource source) {
            currentRoom = ModContent.GetInstance<PyramidSubworld>().Grid.GetEntityCurrentRoom(npc);
            _isBeingSpawned = true;
        }

        public override bool PreAI(NPC npc) {
            if (_isBeingSpawned) {
                if (++_spawnAnimTimer >= 190) {
                    _isBeingSpawned = false;
                    _spawnAnimTimer = 0;
                }

                return false;
            }

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
            if (_isBeingSpawned) {
                MiscShaderData spawnShader = GameShaders.Misc["PyramidNPCSpawn"];

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

                spawnShader.UseShaderSpecificData(new Vector4(_spawnAnimTimer / 60f + MathHelper.PiOver2, 0f, 0f, 0f));
                spawnShader.Apply(new DrawData(TextureAssets.Npc[npc.type].Value, new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.frame.Width, npc.frame.Height), npc.frame, drawColor));

                _spriteBatchNeedsRestart = true;

                return true;
            }


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

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if (_spriteBatchNeedsRestart) {
                _spriteBatchNeedsRestart = false;
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            }

            if (_isBeingSpawned && _spawnAnimTimer < 140) {
                Vector2 dustPos = new(npc.position.X, npc.position.Y + npc.height * (float)Math.Sin(_spawnAnimTimer / 70f + MathHelper.PiOver2));

                Dust.NewDust(dustPos, npc.width, 1, DustID.Sand);
            }
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