using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class FirearmAttackAIState : TownNPCAIState {
    public const int StateInteger = 12;

    public override int ReservedStateInteger => StateInteger;

    public override void DoState( NPC npc) {
        TownNPCCombatModule combatModule = npc.GetGlobalNPC<TownNPCCombatModule>();
        TownNPCAttackData attackData = TownNPCDataSystem.ProfileDatabase[npc.type].AttackData;

        bool inBetweenShots = false;
        // Everything in this switch block is vanilla hardcode, unmanagable with a JSON file, so here it stays
        switch (npc.type) {
            case NPCID.ArmsDealer: {
                if (Main.hardMode) {
                    attackData.damage = 15;
                    if (npc.localAI[3] > attackData.attackDelay) {
                        attackData.attackDelay = 10;
                        inBetweenShots = true;
                    }

                    while (npc.localAI[3] > attackData.attackDelay && attackData.attackDelay < 30) {
                        attackData.attackDelay += 10;
                        inBetweenShots = true;
                    }
                }

                break;
            }
            case NPCID.Painter: {
                if (npc.localAI[3] > attackData.attackDelay) {
                    attackData.attackDelay = 12;
                    inBetweenShots = true;
                }

                if (npc.localAI[3] > attackData.attackDelay) {
                    attackData.attackDelay = 24;
                    inBetweenShots = true;
                }

                if (Main.hardMode) {
                    attackData.damage += 2;
                }

                break;
            }
            case NPCID.TravellingMerchant: {
                if (Main.hardMode) {
                    attackData.damage = 30;
                    attackData.projType = 357;
                }

                break;
            }
            case NPCID.Guide: {
                if (Main.hardMode) {
                    attackData.projType = 2;
                    attackData.attackCooldown = 15;
                    attackData.maxValue = 10;
                    attackData.damage += 6;
                }

                break;
            }
            case NPCID.Steampunker: {
                if (npc.localAI[3] > attackData.attackDelay) {
                    attackData.attackDelay = 8;
                    inBetweenShots = true;
                }

                if (npc.localAI[3] > attackData.attackDelay) {
                    attackData.attackDelay = 16;
                    inBetweenShots = true;
                }

                break;
            }
            case NPCID.Pirate: {
                if (npc.localAI[3] > attackData.attackDelay) {
                    attackData.attackDelay = 16;
                    inBetweenShots = true;
                }

                while (npc.localAI[3] > attackData.attackDelay && attackData.attackDelay < 48) {
                    attackData.attackDelay += 8;
                    inBetweenShots = true;
                }

                if (npc.localAI[3] == 0f && combatModule.AttackLocation is { } location && npc.Distance(location.Center) < NPCID.Sets.PrettySafe[npc.type]) {
                    attackData.randomOffset = 0.1f;
                    attackData.projType = 162;
                    attackData.damage = 50;
                    attackData.knockBack = 10f;
                    attackData.speedMult = 24f;
                }

                break;
            }
            case NPCID.Cyborg:
                attackData.projType = Utils.SelectRandom(Main.rand, 134, 133, 135);
                switch (attackData.projType) {
                    case 135:
                        attackData.speedMult = 12f;
                        attackData.damage = 30;
                        attackData.attackCooldown = 30;
                        attackData.maxValue = 10;
                        attackData.knockBack = 7f;
                        attackData.randomOffset = 0.2f;
                        break;
                    case 133:
                        attackData.speedMult = 10f;
                        attackData.damage = 25;
                        attackData.attackCooldown = 10;
                        attackData.maxValue = 1;
                        attackData.knockBack = 6f;
                        attackData.randomOffset = 0.2f;
                        break;
                    case 134:
                        attackData.speedMult = 13f;
                        attackData.damage = 20;
                        attackData.attackCooldown = 20;
                        attackData.maxValue = 10;
                        attackData.knockBack = 4f;
                        attackData.randomOffset = 0.1f;
                        break;
                }

                break;
        }

        NPCLoader.TownNPCAttackStrength(npc, ref attackData.damage, ref attackData.knockBack);
        NPCLoader.TownNPCAttackCooldown(npc, ref attackData.attackCooldown, ref attackData.maxValue);
        NPCLoader.TownNPCAttackProj(npc, ref attackData.projType, ref attackData.attackDelay);
        NPCLoader.TownNPCAttackProjSpeed(npc, ref attackData.speedMult, ref attackData.gravityCorrection, ref attackData.randomOffset);
        NPCLoader.TownNPCAttackShoot(npc, ref inBetweenShots);

        if (Main.expertMode) {
            attackData.damage = (int)(attackData.damage * Main.GameModeInfo.TownNPCDamageMultiplier);
        }

        attackData.damage = (int)(attackData.damage * combatModule.CurrentDamageMultiplier);

        npc.ai[1] -= 1f;
        npc.localAI[3] += 1f;

        if (npc.localAI[3] == attackData.attackDelay && Main.netMode != NetmodeID.MultiplayerClient) {
            Vector2 projVelocity = Vector2.Zero;
            if (combatModule.AttackLocation is { } location) {
                projVelocity = npc.DirectionTo(location.Center + new Vector2(0f, -attackData.gravityCorrection));
            }

            if (projVelocity.HasNaNs() || projVelocity == Vector2.Zero) {
                projVelocity = new Vector2(npc.spriteDirection, 0f);
            }

            projVelocity *= attackData.speedMult;
            projVelocity += Utils.RandomVector2(Main.rand, 0f - attackData.randomOffset, attackData.randomOffset);

            Projectile projectile = Projectile.NewProjectileDirect(
                npc.GetSource_FromAI(),
                npc.Center + new Vector2(0, -2),
                projVelocity,
                attackData.projType,
                attackData.damage,
                attackData.knockBack,
                Main.myPlayer,
                ai1: npc.type == NPCID.Painter ? Main.rand.Next(12) / 6f : 0f
            );

            projectile.npcProj = true;
            projectile.noDropItem = true;
        }

        if (npc.localAI[3] == attackData.attackDelay && inBetweenShots && combatModule.AttackLocation is { } location2) {
            Vector2 vector = npc.DirectionTo(location2.Center);
            if (vector.Y is <= 0.5f and >= -0.5f) {
                npc.ai[2] = vector.Y;
            }
        }


        if (npc.ai[1] > 0f) {
            return;
        }

        npc.ai[0] = npc.ai[2] = 0f;
        npc.ai[1] = attackData.attackCooldown + Main.rand.Next(attackData.maxValue);
        npc.localAI[1] = npc.localAI[3] = attackData.attackCooldown / 2 + Main.rand.Next(attackData.maxValue);

        npc.netUpdate = true;
    }
}