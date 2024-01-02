using System;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Custom.Classes.TownNPCModules;
using LivingWorldMod.Custom.Structs;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCAIStates;

public class MeleeAttackAIState : TownNPCAIState {
    public override int ReservedStateInteger => 15;

    public override void DoState(TownGlobalNPC globalNPC, NPC npc) {
        TownNPCCombatModule combatModule = globalNPC.CombatModule;
        TownNPCMeleeAttackData attackData = TownNPCCombatModule.meleeAttackData[npc.type];

        // More vanilla hard-code
        if (npc.type == NPCID.TaxCollector && npc.GivenName == "Andrew") {
            attackData.damage *= 2;
            attackData.knockBack *= 2f;
        }

        NPCLoader.TownNPCAttackStrength(npc, ref attackData.damage, ref attackData.knockBack);
        NPCLoader.TownNPCAttackCooldown(npc, ref attackData.attackCooldown, ref attackData.maxValue);
        NPCLoader.TownNPCAttackSwing(npc, ref attackData.itemWidth, ref attackData.itemHeight);
        if (Main.expertMode) {
            attackData.damage = (int)(attackData.damage * Main.GameModeInfo.TownNPCDamageMultiplier);
        }

        attackData.damage = (int)(attackData.damage * combatModule.CurrentDamageMultiplier);
        npc.ai[1] -= 1f;

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Tuple<Vector2, float> swingStats = npc.GetSwingStats(NPCID.Sets.AttackTime[npc.type] * 2, (int)npc.ai[1], npc.spriteDirection, attackData.itemWidth, attackData.itemHeight);

            Rectangle itemRectangle = new((int)swingStats.Item1.X, (int)swingStats.Item1.Y, attackData.itemWidth, attackData.itemHeight);
            if (npc.spriteDirection == -1) {
                itemRectangle.X -= attackData.itemWidth;
            }

            itemRectangle.Y -= attackData.itemHeight;
            npc.TweakSwingStats(NPCID.Sets.AttackTime[npc.type] * 2, (int)npc.ai[1], npc.spriteDirection, ref itemRectangle);

            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC enemyNPC = Main.npc[i];
                if (!enemyNPC.active
                    || enemyNPC.immune[Main.myPlayer] != 0
                    || enemyNPC.dontTakeDamage
                    || enemyNPC.friendly
                    || enemyNPC.damage <= 0
                    || !itemRectangle.Intersects(enemyNPC.Hitbox)
                    || !enemyNPC.noTileCollide && !Collision.CanHit(npc.position, npc.width, npc.height, enemyNPC.position, enemyNPC.width, enemyNPC.height)) {
                    continue;
                }

                enemyNPC.SimpleStrikeNPC(attackData.damage, npc.spriteDirection, knockBack: attackData.knockBack, noPlayerInteraction: true);
                if (Main.netMode != NetmodeID.SinglePlayer) {
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, i, attackData.damage, attackData.knockBack, npc.spriteDirection);
                }

                enemyNPC.netUpdate = true;
                enemyNPC.immune[Main.myPlayer] = (int)npc.ai[1] + 2;
            }
        }

        if (npc.ai[1] > 0f) {
            return;
        }

        bool canHitEnemy = false;
        //if (_enemyNearby) {
        int directionToEnemy = combatModule.AttackLocation is { } location
            ? Math.Sign(npc.DirectionTo(location.Center).X)
            : 1;

        if (!Collision.CanHit(npc.Center, 0, 0, npc.Center + Vector2.UnitX * directionToEnemy * 32f, 0, 0)) {
            canHitEnemy = true;
        }

        if (canHitEnemy) {
            bool secondCanHitCheck = combatModule.AttackLocation is { } location2 && Collision.CanHit(npc.Center, 0, 0, location2.Center, 0, 0);

            if (secondCanHitCheck) {
                npc.ai[0] = 15f;
                npc.ai[1] = NPCID.Sets.AttackTime[npc.type];
                npc.ai[2] = npc.localAI[3] = 0f;

                npc.netUpdate = true;
            }
            else {
                canHitEnemy = false;
            }
        }
        //}

        if (canHitEnemy) {
            return;
        }

        npc.ai[0] = npc.ai[2] = 0f;
        npc.ai[1] = attackData.attackCooldown + Main.rand.Next(attackData.maxValue);
        npc.localAI[1] = npc.localAI[3] = attackData.attackCooldown / 2 + Main.rand.Next(attackData.maxValue);

        npc.netUpdate = true;
    }
}