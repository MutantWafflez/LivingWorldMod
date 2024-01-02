using System;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.ModTypes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LivingWorldMod.Content.TownNPCAIStates {
    public class NurseHealAIState : TownNPCAIState {
        public override int ReservedStateInteger => 13;

        public override void DoState(TownAIGlobalNPC globalNPC, NPC npc) {
            npc.ai[1] -= 1f;
            npc.localAI[3] += 1f;
            if (npc.localAI[3] == 1f && Main.netMode != NetmodeID.MultiplayerClient) {
                Vector2 projVelocity = npc.DirectionTo(Main.npc[(int)npc.ai[2]].Center + new Vector2(0f, -20f));
                if (projVelocity.HasNaNs() || Math.Sign(projVelocity.X) == -npc.spriteDirection) {
                    projVelocity = new Vector2(npc.spriteDirection, -1f);
                }

                projVelocity *= 8f;

                Projectile projectile = Projectile.NewProjectileDirect(
                    npc.GetSource_FromAI(),
                    npc.Center + new Vector2(0f, -2f),
                    projVelocity,
                    ProjectileID.NurseSyringeHeal,
                    0,
                    0f,
                    Main.myPlayer,
                    npc.ai[2]
                );

                projectile.npcProj = true;
                projectile.noDropItem = true;
            }

            if (npc.ai[1] > 0f) {
                return;
            }

            npc.ai[0] = npc.ai[2] = 0f;
            npc.ai[1] = 10 + Main.rand.Next(10);
            npc.localAI[3] = 5 + Main.rand.Next(10);

            npc.netUpdate = true;
        }
    }
}