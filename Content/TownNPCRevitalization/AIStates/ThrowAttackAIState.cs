using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStatuctures.Classes.TownNPCModules;
using LivingWorldMod.DataStatuctures.Structs;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class ThrowAttackAIState : TownNPCAIState {
    public override int ReservedStateInteger => 10;

    public override void DoState(TownGlobalNPC globalNPC, NPC npc) {
        TownNPCCombatModule combatModule = globalNPC.CombatModule;
        TownNPCCombatModule.projAttackData.TryGetValue(npc.type, out TownNPCProjAttackData attackData);

        switch (npc.type) {
            case NPCID.BestiaryGirl:
                if (npc.ShouldBestiaryGirlBeLycantrope()) {
                    attackData.projType = 929;
                    attackData.projDamage = (int)(attackData.projDamage * 1.5f);
                }
                break;
        }

        NPCLoader.TownNPCAttackStrength(npc, ref attackData.projDamage, ref attackData.knockBack);
        NPCLoader.TownNPCAttackCooldown(npc, ref attackData.attackCooldown, ref attackData.attackCooldown);
        NPCLoader.TownNPCAttackProj(npc, ref attackData.projType, ref attackData.attackDelay);
        NPCLoader.TownNPCAttackProjSpeed(npc, ref attackData.speedMult, ref attackData.gravityCorrection, ref attackData.randomOffset);

        if (Main.expertMode) {
            attackData.projDamage = (int)(attackData.projDamage * Main.GameModeInfo.TownNPCDamageMultiplier);
        }
        attackData.projDamage = (int)(attackData.projDamage * combatModule.CurrentDamageMultiplier);

        npc.ai[1] -= 1f;
        npc.localAI[3] += 1f;
        if (npc.localAI[3] == attackData.attackDelay && Main.netMode != NetmodeID.MultiplayerClient) {
            Vector2 projVelocity = -Vector2.UnitY;
            if (combatModule.AttackLocation is { } location) {
                projVelocity = npc.DirectionTo(location.Center + new Vector2(0f,
                    (0f - attackData.gravityCorrection) * MathHelper.Clamp(npc.Distance(location.Center) / attackData.dangerDetectRange, 0f, 1f)));
            }

            if (projVelocity.HasNaNs() || projVelocity == -Vector2.UnitY) {
                projVelocity = new Vector2(npc.spriteDirection, -1f);
            }

            projVelocity *= attackData.speedMult;
            projVelocity += Utils.RandomVector2(Main.rand, 0f - attackData.randomOffset, attackData.randomOffset);

            float projectileAI1Value = npc.type switch {
                NPCID.Mechanic => npc.whoAmI,
                NPCID.SantaClaus => Main.rand.Next(5),
                _ => 0f
            };

            Projectile projectile = Projectile.NewProjectileDirect(
                npc.GetSource_FromAI(),
                npc.Center + new Vector2(0, -2f),
                projVelocity,
                attackData.projType,
                attackData.projDamage,
                attackData.knockBack,
                Main.myPlayer,
                0f,
                projectileAI1Value
            );

            projectile.npcProj = true;
            projectile.noDropItem = true;

            if (npc.type == NPCID.Golfer) {
                projectile.timeLeft = 480;
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