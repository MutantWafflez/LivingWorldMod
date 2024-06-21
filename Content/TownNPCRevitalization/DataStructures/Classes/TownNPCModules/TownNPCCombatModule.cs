using System.Collections.Generic;
using Hjson;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

/// <summary>
///     Module for the Town NPC revitalization that handles combat for
///     a given Town NPC.
/// </summary>
public sealed class TownNPCCombatModule : TownNPCModule {
    public static IReadOnlyDictionary<int, TownNPCProjAttackData> projAttackData;
    public static IReadOnlyDictionary<int, TownNPCMeleeAttackData> meleeAttackData;

    public TownNPCCombatModule(NPC npc) : base(npc) { }

    public bool IsAttacking => npc.ai[0] >= TownNPCAIState.GetStateInteger<ThrowAttackAIState>() && npc.ai[0] <= TownNPCAIState.GetStateInteger<MeleeAttackAIState>();

    public float CurrentDamageMultiplier {
        get;
        private set;
    }

    public PreciseRectangle? AttackLocation {
        get;
        private set;
    }

    public override void Load() {
        JsonObject jsonAttackData = LWMUtils.GetJSONFromFile("Assets/JSONData/TownNPCAttackData.json").Qo();

        JsonObject projJSONAttackData = jsonAttackData["ProjNPCs"].Qo();
        JsonObject meleeJSONAttackData = jsonAttackData["MeleeNPCs"].Qo();

        Dictionary<int, TownNPCProjAttackData> projDict = [];
        foreach ((string npcName, JsonValue jsonValue) in projJSONAttackData) {
            JsonObject jsonObject = jsonValue.Qo();
            int npcType = NPCID.Search.GetId(npcName);

            projDict[npcType] = new TownNPCProjAttackData(
                jsonObject.Qi("projType"),
                jsonObject.Qi("projDamage"),
                (float)jsonObject.Qd("knockBack"),
                (float)jsonObject.Qd("speedMult"),
                jsonObject.Qi("attackDelay"),
                jsonObject.Qi("attackCooldown"),
                jsonObject.Qi("maxValue"),
                jsonObject.Qi("gravityCorrection"),
                NPCID.Sets.DangerDetectRange[npcType],
                (float)jsonObject.Qd("randomOffset")
            );
        }
        projAttackData = projDict;

        Dictionary<int, TownNPCMeleeAttackData> meleeDict = [];
        foreach ((string npcName, JsonValue jsonValue) in meleeJSONAttackData) {
            JsonObject jsonObject = jsonValue.Qo();
            int npcType = NPCID.Search.GetId(npcName);

            meleeDict[npcType] = new TownNPCMeleeAttackData(
                jsonObject.Qi("attackCooldown"),
                jsonObject.Qi("maxValue"),
                jsonObject.Qi("damage"),
                (float)jsonObject.Qd("knockBack"),
                jsonObject.Qi("itemWidth"),
                jsonObject.Qi("itemHeight")
            );
        }
        meleeAttackData = meleeDict;
    }

    public override void Update() {
        SetCombatStats();
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        float dangerDetectRange = 200f;
        if (NPCID.Sets.DangerDetectRange[npc.type] != -1) {
            dangerDetectRange = NPCID.Sets.DangerDetectRange[npc.type];
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

            bool modCanHit = NPCLoader.CanHitNPC(otherNPC, npc);
            if (!modCanHit) {
                continue;
            }

            float distanceToNPC = otherNPC.Distance(npc.Center);
            if (!otherNPC.active
                || otherNPC.friendly
                || otherNPC.damage <= 0
                || !(distanceToNPC < dangerDetectRange)
                || !otherNPC.noTileCollide && !Collision.CanHit(npc.Center, 0, 0, otherNPC.Center, 0, 0)) {
                continue;
            }

            enemyNearby = true;
            if (distanceToNPC >= closestHostileNPCDistance) {
                continue;
            }

            closestHostileNPCDistance = distanceToNPC;
            AttackLocation = otherNPC.CanBeChasedBy(npc) ? otherNPC.GetPreciseRectangle() : AttackLocation;
        }

        if (enemyNearby && NPCID.Sets.PrettySafe[npc.type] != -1 && closestHostileNPCDistance is { } value && NPCID.Sets.PrettySafe[npc.type] < value) {
            isCapableOfViolence = NPCID.Sets.AttackType[npc.type] > -1;
            /*
            int wanderState = TownNPCAIState.GetStateInteger<WanderAIState>();
            if (npc.ai[0] != wanderState) {
                TownGlobalNPC.RefreshToState(npc, wanderState);
            }*/
        }

        if (--npc.localAI[1] > 0) {
            return;
        }
        npc.localAI[1] = 0;

        if (!enemyNearby
            || !isCapableOfViolence
            || IsAttacking
            || AttackLocation is null
            || npc.velocity.Y != 0f
            || NPCID.Sets.AttackAverageChance[npc.type] <= 0
            || !Main.rand.NextBool(NPCID.Sets.AttackAverageChance[npc.type] * 2)) {
            return;
        }

        if (npc.type == NPCID.Nurse && npc.breath > 0) {
            int otherTownNPCIndex = -1;
            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC otherNPC = Main.npc[i];
                if (otherNPC.active
                    && otherNPC.townNPC
                    && otherNPC.life != otherNPC.lifeMax
                    && (otherTownNPCIndex == -1 || otherNPC.lifeMax - otherNPC.life > Main.npc[otherTownNPCIndex].lifeMax - Main.npc[otherTownNPCIndex].life)
                    && Collision.CanHitLine(npc.position, npc.width, npc.height, otherNPC.position, otherNPC.width, otherNPC.height)
                    && npc.Distance(otherNPC.Center) < 500f) {
                    otherTownNPCIndex = i;
                }
            }

            if (otherTownNPCIndex != -1) {
                npc.localAI[2] = npc.localAI[3] = 0f;

                npc.ai[0] = TownNPCAIState.GetStateInteger<NurseHealAIState>();
                npc.ai[1] = 34f;
                npc.ai[2] = otherTownNPCIndex;

                npc.direction = npc.position.X < Main.npc[otherTownNPCIndex].position.X ? 1 : -1;

                npc.netUpdate = true;
            }
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
        bool canHitAttackLocation = Collision.CanHit(npc.Center, 0, 0, AttackLocation!.Value.Center, 0, 0);

        //Hard-coded vanilla check. Not much I can do about it :(
        if (canHitAttackLocation && npc.type == NPCID.BestiaryGirl) {
            canHitAttackLocation = Vector2.Distance(npc.Center, AttackLocation.Value.Center) <= 50f;
        }

        if (!canHitAttackLocation) {
            return;
        }

        npc.ai[0] = NPCID.Sets.AttackType[npc.type] switch {
            0 => TownNPCAIState.GetStateInteger<ThrowAttackAIState>(),
            1 => TownNPCAIState.GetStateInteger<FirearmAttackAIState>(),
            2 => TownNPCAIState.GetStateInteger<MagicAttackAIState>(),
            _ => TownNPCAIState.GetStateInteger<MeleeAttackAIState>()
        };
        npc.ai[1] = NPCID.Sets.AttackTime[npc.type];
        npc.ai[2] = NPCID.Sets.AttackType[npc.type] switch {
            1 => npc.DirectionTo(AttackLocation.Value.Center).Y,
            _ => 0f
        };

        GlobalNPC.PathfinderModule.CancelPathfind();
        npc.localAI[3] = 0f;
        npc.direction = npc.position.X < AttackLocation.Value.position.X ? 1 : -1;
        npc.netUpdate = true;
    }

    private void SetCombatStats() {
        float damageMultiplier = 1f;

        // Vanilla easter egg added in 1.4.4
        if (npc.type == NPCID.TaxCollector && npc.GivenName == "Andrew") {
            npc.defDefense = 200;
        }

        if (Main.masterMode) {
            npc.defense = npc.dryadWard ? npc.defDefense + 14 : npc.defDefense;
        }
        else if (Main.expertMode) {
            npc.defense = npc.dryadWard ? npc.defDefense + 10 : npc.defDefense;
        }
        else {
            npc.defense = npc.dryadWard ? npc.defDefense + 6 : npc.defDefense;
        }

        if (NPC.combatBookWasUsed) {
            damageMultiplier += 0.2f;
            npc.defense += 6;
        }

        if (NPC.combatBookVolumeTwoWasUsed) {
            damageMultiplier += 0.2f;
            npc.defense += 6;
        }

        if (NPC.downedBoss1) {
            damageMultiplier += 0.1f;
            npc.defense += 3;
        }

        if (NPC.downedBoss2) {
            damageMultiplier += 0.1f;
            npc.defense += 3;
        }

        if (NPC.downedBoss3) {
            damageMultiplier += 0.1f;
            npc.defense += 3;
        }

        if (NPC.downedQueenBee) {
            damageMultiplier += 0.1f;
            npc.defense += 3;
        }

        if (Main.hardMode) {
            damageMultiplier += 0.4f;
            npc.defense += 12;
        }

        if (NPC.downedQueenSlime) {
            damageMultiplier += 0.15f;
            npc.defense += 6;
        }

        if (NPC.downedMechBoss1) {
            damageMultiplier += 0.15f;
            npc.defense += 6;
        }

        if (NPC.downedMechBoss2) {
            damageMultiplier += 0.15f;
            npc.defense += 6;
        }

        if (NPC.downedMechBoss3) {
            damageMultiplier += 0.15f;
            npc.defense += 6;
        }

        if (NPC.downedPlantBoss) {
            damageMultiplier += 0.15f;
            npc.defense += 8;
        }

        if (NPC.downedEmpressOfLight) {
            damageMultiplier += 0.15f;
            npc.defense += 8;
        }

        if (NPC.downedGolemBoss) {
            damageMultiplier += 0.15f;
            npc.defense += 8;
        }

        if (NPC.downedAncientCultist) {
            damageMultiplier += 0.15f;
            npc.defense += 8;
        }

        NPCLoader.BuffTownNPC(ref damageMultiplier, ref npc.defense);
        CurrentDamageMultiplier = damageMultiplier;
    }
}