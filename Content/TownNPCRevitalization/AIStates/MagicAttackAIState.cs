using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class MagicAttackAIState : TownNPCAIState {
    public override int ReservedStateInteger => 14;

    public override void DoState(TownGlobalNPC globalNPC, NPC npc) {
        TownNPCCombatModule combatModule = globalNPC.CombatModule;
        TownNPCDataSystem.townNPCProjectileAttackData.TryGetValue(npc.type, out TownNPCProjAttackData attackData);
        float auraLightMultiplier = 1f;

        NPCLoader.TownNPCAttackStrength(npc, ref attackData.projDamage, ref attackData.knockBack);
        NPCLoader.TownNPCAttackCooldown(npc, ref attackData.attackCooldown, ref attackData.maxValue);
        NPCLoader.TownNPCAttackProj(npc, ref attackData.projType, ref attackData.attackDelay);
        NPCLoader.TownNPCAttackProjSpeed(npc, ref attackData.speedMult, ref attackData.gravityCorrection, ref attackData.randomOffset);
        NPCLoader.TownNPCAttackMagic(npc, ref auraLightMultiplier);
        if (Main.expertMode) {
            attackData.projDamage = (int)(attackData.projDamage * Main.GameModeInfo.TownNPCDamageMultiplier);
        }

        attackData.projDamage = (int)(attackData.projDamage * combatModule.CurrentDamageMultiplier);

        npc.ai[1] -= 1f;
        npc.localAI[3] += 1f;

        if (npc.localAI[3] == attackData.attackDelay && Main.netMode != NetmodeID.MultiplayerClient) {
            Vector2 projVelocity = Vector2.Zero;
            if (combatModule.AttackLocation is { } location) {
                projVelocity = npc.DirectionTo(
                    location.Center
                    + new Vector2(
                        0f,
                        (0f - attackData.gravityCorrection) * MathHelper.Clamp(npc.Distance(location.Center) / attackData.dangerDetectRange, 0f, 1f)
                    )
                );
            }

            if (projVelocity.HasNaNs() || projVelocity == Vector2.Zero) {
                projVelocity = new Vector2(npc.spriteDirection, 0f);
            }

            projVelocity *= attackData.speedMult;
            projVelocity += Utils.RandomVector2(Main.rand, 0f - attackData.randomOffset, attackData.randomOffset);

            switch (npc.type) {
                case NPCID.Wizard: {
                    int projCount = Utils.SelectRandom(Main.rand, 1, 1, 1, 1, 2, 2, 3);

                    for (int projIndex = 0; projIndex < projCount; projIndex++) {
                        Vector2 randomOffsetTwo = Utils.RandomVector2(Main.rand, -3.4f, 3.4f);

                        Projectile projectile = Projectile.NewProjectileDirect(
                            npc.GetSource_FromAI(),
                            npc.Center + new Vector2(0, -2f),
                            projVelocity + randomOffsetTwo,
                            attackData.projType,
                            attackData.projDamage,
                            attackData.knockBack,
                            Main.myPlayer
                        );

                        projectile.npcProj = true;
                        projectile.noDropItem = true;
                    }

                    break;
                }
                case NPCID.Truffle: {
                    if (combatModule.AttackLocation is { } location2) {
                        Vector2 projPosition = location2.position - location2.size * 2f + location2.size * Utils.RandomVector2(Main.rand, 0f, 1f) * 5f;

                        int firstAirTileY = 10;
                        while (firstAirTileY > 0 && WorldGen.SolidTile(Framing.GetTileSafely((int)projPosition.X / 16, (int)projPosition.Y / 16))) {
                            firstAirTileY--;

                            projPosition = location2.position - location2.size * 2f + location2.size * Utils.RandomVector2(Main.rand, 0f, 1f) * 5f;
                        }

                        Projectile projectile = Projectile.NewProjectileDirect(
                            npc.GetSource_FromAI(),
                            projPosition,
                            Vector2.Zero,
                            attackData.projType,
                            attackData.projDamage,
                            attackData.knockBack,
                            Main.myPlayer
                        );

                        projectile.npcProj = true;
                        projectile.noDropItem = true;
                    }

                    break;
                }
                case NPCID.Princess: {
                    if (combatModule.AttackLocation is { } location2) {
                        Vector2 projPosition = location2.position + location2.size * Utils.RandomVector2(Main.rand, 0f, 1f) * 1f;

                        int firstAirTileY = 5;
                        while (firstAirTileY > 0 && WorldGen.SolidTile(Framing.GetTileSafely((int)projPosition.X / 16, (int)projPosition.Y / 16))) {
                            firstAirTileY--;

                            projPosition = location2.position + location2.size * Utils.RandomVector2(Main.rand, 0f, 1f) * 1f;
                        }

                        Projectile projectile = Projectile.NewProjectileDirect(
                            npc.GetSource_FromAI(),
                            projPosition,
                            Vector2.Zero,
                            attackData.projType,
                            attackData.projDamage,
                            attackData.knockBack,
                            Main.myPlayer
                        );

                        projectile.npcProj = true;
                        projectile.noDropItem = true;
                    }

                    break;
                }
                case NPCID.Dryad: {
                    Projectile projectile = Projectile.NewProjectileDirect(
                        npc.GetSource_FromAI(),
                        npc.Center + new Vector2(0, -2f),
                        projVelocity,
                        attackData.projType,
                        attackData.projDamage,
                        attackData.knockBack,
                        Main.myPlayer,
                        ai1: npc.whoAmI
                    );

                    projectile.npcProj = true;
                    projectile.noDropItem = true;
                    break;
                }
                default: {
                    Projectile projectile = Projectile.NewProjectileDirect(
                        npc.GetSource_FromAI(),
                        npc.Center + new Vector2(0, -2f),
                        projVelocity,
                        attackData.projType,
                        attackData.projDamage,
                        attackData.knockBack,
                        Main.myPlayer
                    );

                    projectile.npcProj = true;
                    projectile.noDropItem = true;
                    break;
                }
            }
        }

        if (auraLightMultiplier > 0f) {
            Vector3 lightVector = NPCID.Sets.MagicAuraColor[npc.type].ToVector3() * auraLightMultiplier;
            Lighting.AddLight(npc.Center, lightVector.X, lightVector.Y, lightVector.Z);
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