using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

/// <summary>
///     Global NPC that acts as the foundation of the Town NPC Revitalization. Has minimal code on its own, but is infinitely expandable using <see cref="TownNPCModule" /> instances.
/// </summary>
public class TownGlobalNPC : GlobalNPC {
    private IReadOnlyList<TownNPCModule> _prioritizedModules;

    public override bool InstancePerEntity => true;

    public static bool IsValidStandingPosition(NPC npc, Point tilePos) {
        bool foundTileToStandOn = false;
        int npcTileWidth = (int)Math.Ceiling(npc.width / 16f);
        for (int i = 0; i < npcTileWidth; i++) {
            Tile floorTile = Main.tile[tilePos + new Point(i, 1)];
            if (!floorTile.HasUnactuatedTile || floorTile.IsHalfBlock || (!Main.tileSolidTop[floorTile.TileType] && !Main.tileSolid[floorTile.TileType])) {
                continue;
            }

            foundTileToStandOn = true;
            break;
        }

        if (!foundTileToStandOn) {
            return false;
        }

        int npcTileHeight = (int)Math.Ceiling(npc.height / 16f);
        for (int i = 0; i < npcTileHeight; i++) {
            for (int j = 0; j < npcTileWidth; j++) {
                Tile upTile = Main.tile[tilePos.X + j, tilePos.Y - i];

                if (upTile.HasUnactuatedTile && Main.tileSolid[upTile.TileType]) {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool EntityIsValidTownNPC(NPC entity, bool lateInstantiation) => lateInstantiation
        && entity.aiStyle == NPCAIStyleID.Passive
        && entity.townNPC
        && !NPCID.Sets.IsTownPet[entity.type]
        && !NPCID.Sets.IsTownSlime[entity.type]
        && entity.type != NPCID.OldMan
        && entity.type != NPCID.TravellingMerchant;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => EntityIsValidTownNPC(entity, lateInstantiation);

    public override void SetDefaults(NPC entity) {
        List<GlobalNPC> entityGlobals = [];
        foreach (GlobalNPC npc in entity.EntityGlobals) {
            entityGlobals.Add(npc);
        }

        _prioritizedModules = entityGlobals.OfType<TownNPCModule>().OrderBy(module => module.UpdatePriority).ToList();
    }

    public override bool PreAI(NPC npc) {
        npc.aiStyle = -1;
        return true;
    }

    public override void AI(NPC npc) {
        SetMiscNPCFields(npc);

        foreach (TownNPCModule module in _prioritizedModules) {
            module.UpdateModule();
        }
    }

    public override void PostAI(NPC npc) {
        // To make vanilla still draw extras properly
        npc.aiStyle = NPCAIStyleID.Passive;
    }

    private void SetMiscNPCFields(NPC npc) {
        npc.dontTakeDamage = false;
        npc.rotation = 0f;
        NPC.ShimmeredTownNPCs[npc.type] = npc.IsShimmerVariant;

        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        if (npc.HasBuff(BuffID.Shimmer)) {
            pathfinderModule.CancelPathfind();
        }

        if (npc.type == NPCID.SantaClaus && Main.netMode != NetmodeID.MultiplayerClient && !Main.xMas) {
            npc.StrikeInstantKill();
        }

        switch (npc.type) {
            case NPCID.Golfer:
                NPC.savedGolfer = true;
                break;
            case NPCID.TaxCollector:
                NPC.savedTaxCollector = true;
                NPC.taxCollector = true;
                break;
            case NPCID.GoblinTinkerer:
                NPC.savedGoblin = true;
                break;
            case NPCID.Wizard:
                NPC.savedWizard = true;
                break;
            case NPCID.Mechanic:
                NPC.savedMech = true;
                break;
            case NPCID.Stylist:
                NPC.savedStylist = true;
                break;
            case NPCID.Angler:
                NPC.savedAngler = true;
                break;
            case NPCID.DD2Bartender:
                NPC.savedBartender = true;
                break;
        }

        npc.directionY = -1;
        if (npc.direction == 0) {
            npc.direction = 1;
        }

        if (npc.velocity.Y == 0f && pathfinderModule.IsPathfinding) {
            npc.velocity *= 0.75f;
        }

        if (npc.type != NPCID.Mechanic) {
            return;
        }

        int wrenchWhoAmI = NPC.lazyNPCOwnedProjectileSearchArray[npc.whoAmI];
        bool wrenchFound = false;

        if (Main.projectile.IndexInRange(wrenchWhoAmI)) {
            Projectile projectile = Main.projectile[wrenchWhoAmI];
            if (projectile.active && projectile.type == ProjectileID.MechanicWrench && projectile.ai[1] == npc.whoAmI) {
                wrenchFound = true;
            }
        }

        npc.localAI[0] = wrenchFound.ToInt();
    }
}