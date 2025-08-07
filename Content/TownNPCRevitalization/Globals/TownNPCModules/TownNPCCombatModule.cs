using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

/// <summary>
///     Module for the Town NPC revitalization that handles combat for
///     a given Town NPC.
/// </summary>
public sealed class TownNPCCombatModule : TownNPCModule {
    public override int UpdatePriority => -1;

    public float CurrentDamageMultiplier {
        get;
        private set;
    }

    public PreciseRectangle? AttackLocation {
        get;
        private set;
    }

    public bool IsAttacking => NPC.ai[0] >= TownNPCAIState.GetStateInteger<ThrowAttackAIState>() && NPC.ai[0] <= TownNPCAIState.GetStateInteger<MeleeAttackAIState>();

    public override void UpdateModule() {
        SetCombatStats();
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        float dangerDetectRange = 200f;
        if (NPCID.Sets.DangerDetectRange[NPC.type] != -1) {
            dangerDetectRange = NPCID.Sets.DangerDetectRange[NPC.type];
        }

        bool isCapableOfViolence = true;
        float? closestHostileNPCDistance = null;

        bool enemyNearby = false;
        AttackLocation = null;
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC otherNPC = Main.npc[i];

            if (!otherNPC.active) {
                continue;
            }

            bool modCanHit = NPCLoader.CanHitNPC(otherNPC, NPC);
            if (!modCanHit) {
                continue;
            }

            float distanceToNPC = otherNPC.Distance(NPC.Center);
            if (!otherNPC.active
                || otherNPC.friendly
                || otherNPC.damage <= 0
                || !(distanceToNPC < dangerDetectRange)
                || (!otherNPC.noTileCollide && !Collision.CanHit(NPC.Center, 0, 0, otherNPC.Center, 0, 0))) {
                continue;
            }

            enemyNearby = true;
            if (distanceToNPC >= closestHostileNPCDistance) {
                continue;
            }

            closestHostileNPCDistance = distanceToNPC;
            AttackLocation = otherNPC.CanBeChasedBy(NPC) ? otherNPC.GetPreciseRectangle() : AttackLocation;
        }

        if (enemyNearby && NPCID.Sets.PrettySafe[NPC.type] != -1 && closestHostileNPCDistance is { } value && NPCID.Sets.PrettySafe[NPC.type] < value) {
            isCapableOfViolence = NPCID.Sets.AttackType[NPC.type] > -1;
            /*
            int wanderState = TownNPCAIState.GetStateInteger<WanderAIState>();
            if (npc.ai[0] != wanderState) {
                TownGlobalNPC.RefreshToState(npc, wanderState);
            }*/
        }

        if (--NPC.localAI[1] > 0) {
            return;
        }

        NPC.localAI[1] = 0;

        if (IsAttacking || NPC.velocity.Y != 0f || NPC.GetGlobalNPC<TownNPCSleepModule>().IsAsleep) {
            return;
        }

        if (NPC.type == NPCID.Nurse && NPC.breath > 0) {
            int otherTownNPCIndex = -1;
            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC otherNPC = Main.npc[i];
                if (otherNPC.active
                    && otherNPC.townNPC
                    && otherNPC.life < otherNPC.lifeMax
                    && (otherTownNPCIndex == -1 || otherNPC.lifeMax - otherNPC.life > Main.npc[otherTownNPCIndex].lifeMax - Main.npc[otherTownNPCIndex].life)
                    && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, otherNPC.position, otherNPC.width, otherNPC.height)
                    && NPC.Distance(otherNPC.Center) < 500f) {
                    otherTownNPCIndex = i;
                }
            }

            if (otherTownNPCIndex != -1) {
                NPC.localAI[2] = NPC.localAI[3] = 0f;

                NPC.ai[0] = TownNPCAIState.GetStateInteger<NurseHealAIState>();
                NPC.ai[1] = 34f;
                NPC.ai[2] = otherTownNPCIndex;

                NPC.GetGlobalNPC<TownNPCPathfinderModule>().CancelPathfind();

                NPC.direction = NPC.position.X < Main.npc[otherTownNPCIndex].position.X ? 1 : -1;
                NPC.netUpdate = true;
                return;
            }
        }

        if (!enemyNearby
            || !isCapableOfViolence
            || AttackLocation is null
            || NPCID.Sets.AttackAverageChance[NPC.type] <= 0
            || !Main.rand.NextBool(NPCID.Sets.AttackAverageChance[NPC.type] * 2)) {
            return;
        }

        AttemptTriggerAttack();
    }

    public void RequestAttackToLocation(PreciseRectangle attackLocation) {
        if (IsAttacking) {
            return;
        }

        AttackLocation = attackLocation;
        AttemptTriggerAttack();
    }

    private void AttemptTriggerAttack() {
        bool canHitAttackLocation = Collision.CanHit(NPC.Center, 0, 0, AttackLocation!.Value.Center, 0, 0);

        //Hard-coded vanilla check. Not much I can do about it :(
        if (canHitAttackLocation && NPC.type == NPCID.BestiaryGirl) {
            canHitAttackLocation = Vector2.Distance(NPC.Center, AttackLocation.Value.Center) <= 50f;
        }

        if (!canHitAttackLocation) {
            return;
        }

        NPC.ai[0] = NPCID.Sets.AttackType[NPC.type] switch {
            0 => TownNPCAIState.GetStateInteger<ThrowAttackAIState>(),
            1 => TownNPCAIState.GetStateInteger<FirearmAttackAIState>(),
            2 => TownNPCAIState.GetStateInteger<MagicAttackAIState>(),
            _ => TownNPCAIState.GetStateInteger<MeleeAttackAIState>()
        };
        NPC.ai[1] = NPCID.Sets.AttackTime[NPC.type];
        NPC.ai[2] = NPCID.Sets.AttackType[NPC.type] switch {
            1 => NPC.DirectionTo(AttackLocation.Value.Center).Y,
            _ => 0f
        };

        IOnTownNPCAttack.Invoke(NPC);
        NPC.GetGlobalNPC<TownNPCPathfinderModule>().CancelPathfind();
        
        NPC.localAI[3] = 0f;
        NPC.direction = NPC.position.X < AttackLocation.Value.position.X ? 1 : -1;
        NPC.netUpdate = true;
    }

    private void SetCombatStats() {
        float damageMultiplier = 1f;

        // Vanilla easter egg added in 1.4.4
        if (NPC.type == NPCID.TaxCollector && NPC.GivenName == "Andrew") {
            NPC.defDefense = 200;
        }

        if (Main.masterMode) {
            NPC.defense = NPC.dryadWard ? NPC.defDefense + 14 : NPC.defDefense;
        }
        else if (Main.expertMode) {
            NPC.defense = NPC.dryadWard ? NPC.defDefense + 10 : NPC.defDefense;
        }
        else {
            NPC.defense = NPC.dryadWard ? NPC.defDefense + 6 : NPC.defDefense;
        }

        if (NPC.combatBookWasUsed) {
            damageMultiplier += 0.2f;
            NPC.defense += 6;
        }

        if (NPC.combatBookVolumeTwoWasUsed) {
            damageMultiplier += 0.2f;
            NPC.defense += 6;
        }

        if (NPC.downedBoss1) {
            damageMultiplier += 0.1f;
            NPC.defense += 3;
        }

        if (NPC.downedBoss2) {
            damageMultiplier += 0.1f;
            NPC.defense += 3;
        }

        if (NPC.downedBoss3) {
            damageMultiplier += 0.1f;
            NPC.defense += 3;
        }

        if (NPC.downedQueenBee) {
            damageMultiplier += 0.1f;
            NPC.defense += 3;
        }

        if (Main.hardMode) {
            damageMultiplier += 0.4f;
            NPC.defense += 12;
        }

        if (NPC.downedQueenSlime) {
            damageMultiplier += 0.15f;
            NPC.defense += 6;
        }

        if (NPC.downedMechBoss1) {
            damageMultiplier += 0.15f;
            NPC.defense += 6;
        }

        if (NPC.downedMechBoss2) {
            damageMultiplier += 0.15f;
            NPC.defense += 6;
        }

        if (NPC.downedMechBoss3) {
            damageMultiplier += 0.15f;
            NPC.defense += 6;
        }

        if (NPC.downedPlantBoss) {
            damageMultiplier += 0.15f;
            NPC.defense += 8;
        }

        if (NPC.downedEmpressOfLight) {
            damageMultiplier += 0.15f;
            NPC.defense += 8;
        }

        if (NPC.downedGolemBoss) {
            damageMultiplier += 0.15f;
            NPC.defense += 8;
        }

        if (NPC.downedAncientCultist) {
            damageMultiplier += 0.15f;
            NPC.defense += 8;
        }

        NPCLoader.BuffTownNPC(ref damageMultiplier, ref NPC.defense);
        CurrentDamageMultiplier = damageMultiplier;
    }
}