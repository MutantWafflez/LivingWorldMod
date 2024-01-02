using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Structs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.TownNPCAIStates {
    public class FirearmAttackAIState : TownNPCAIState {
        public override int ReservedStateInteger => 12;

        public override void DoState(TownAIGlobalNPC globalNPC, NPC npc) {
            TownNPCCombatModule combatModule = globalNPC.CombatModule;
            TownNPCProjAttackData attackData = TownNPCCombatModule.projAttackData[npc.type];
            bool inBetweenShots = false;

            // Everything in this switch block is vanilla hardcode, unmanagable with a JSON file, so here it stays
            switch (npc.type) {
                case NPCID.ArmsDealer: {
                    if (Main.hardMode) {
                        attackData.projDamage = 15;
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
                        attackData.projDamage += 2;
                    }
                    break;
                }
                case NPCID.TravellingMerchant: {
                    if (Main.hardMode) {
                        attackData.projDamage = 30;
                        attackData.projType = 357;
                    }
                    break;
                }
                case NPCID.Guide: {
                    if (Main.hardMode) {
                        attackData.projType = 2;
                        attackData.attackCooldown = 15;
                        attackData.maxValue = 10;
                        attackData.projDamage += 6;
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
                        attackData.projDamage = 50;
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
                            attackData.projDamage = 30;
                            attackData.attackCooldown = 30;
                            attackData.maxValue = 10;
                            attackData.knockBack = 7f;
                            attackData.randomOffset = 0.2f;
                            break;
                        case 133:
                            attackData.speedMult = 10f;
                            attackData.projDamage = 25;
                            attackData.attackCooldown = 10;
                            attackData.maxValue = 1;
                            attackData.knockBack = 6f;
                            attackData.randomOffset = 0.2f;
                            break;
                        case 134:
                            attackData.speedMult = 13f;
                            attackData.projDamage = 20;
                            attackData.attackCooldown = 20;
                            attackData.maxValue = 10;
                            attackData.knockBack = 4f;
                            attackData.randomOffset = 0.1f;
                            break;
                    }
                    break;
            }

            NPCLoader.TownNPCAttackStrength(npc, ref attackData.projDamage, ref attackData.knockBack);
            NPCLoader.TownNPCAttackCooldown(npc, ref attackData.attackCooldown, ref attackData.maxValue);
            NPCLoader.TownNPCAttackProj(npc, ref attackData.projType, ref attackData.attackDelay);
            NPCLoader.TownNPCAttackProjSpeed(npc, ref attackData.speedMult, ref attackData.gravityCorrection, ref attackData.randomOffset);
            NPCLoader.TownNPCAttackShoot(npc, ref inBetweenShots);

            if (Main.expertMode) {
                attackData.projDamage = (int)(attackData.projDamage * Main.GameModeInfo.TownNPCDamageMultiplier);
            }

            attackData.projDamage = (int)(attackData.projDamage * combatModule.CurrentDamageMultiplier);

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
                    attackData.projDamage,
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
}