using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;

/// <summary>
///     Global NPC that acts as the foundation of the Town NPC Revitalization. Has minimal code on its own, but is infinitely expandable using <see cref="TownNPCModule" /> instances.
/// </summary>
public class TownGlobalNPC : GlobalNPC {
    /// <summary>
    ///     Dictionary that maps a given NPC's type to any additional NPC display names this mod may add for said NPC.
    /// </summary>
    private static readonly Dictionary<int, LocalizedTextGroup> AdditionalTownNPCNames = [];

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

                if (upTile.HasUnactuatedTile && Main.tileSolid[upTile.TileType] && !Main.tileSolidTop[upTile.TileType]) {
                    return false;
                }
            }
        }

        return true;
    }

    // TODO: Fit the Travelling Merchant into all of this
    /// <summary>
    ///     This method determines if a given NPC is a "full" Town NPC. A "full" Town NPC is any NPC that uses <see cref="NPCAIStyleID.Passive" />, and is NOT a Town Pet. This includes the Guide,
    ///     Merchant, Nurse, Princess, Angler, and so on.
    /// </summary>
    public static bool IsValidFullTownNPC(NPC entity, bool lateInstantiation) => lateInstantiation
        && entity.aiStyle == NPCAIStyleID.Passive
        && entity.townNPC
        && !NPCID.Sets.IsTownPet[entity.type];

    /// <summary>
    ///     This method determines if a given NPC is a valid Town Pet. A valid Town NPC is any NPC that uses <see cref="NPCAIStyleID.Passive" />, and matches with the <see cref="NPCID.Sets.IsTownPet" />
    ///     but also is NOT a Town Slime (i.e. <see cref="NPCID.Sets.IsTownSlime" />.
    /// </summary>
    public static bool IsValidTownPet(NPC entity, bool lateInstantiation) => lateInstantiation
        && entity.aiStyle == NPCAIStyleID.Passive
        && entity.townNPC
        && NPCID.Sets.IsTownPet[entity.type]
        && !NPCID.Sets.IsTownSlime[entity.type];

    /// <summary>
    ///     Simple method that simply returns if this NPC is a valid Town NPC of ANY variety; that is, whether its a "full" Town NPC or a Town Pet.
    /// </summary>
    public static bool IsAnyValidTownNPC(NPC entity, bool lateInstantiation) => IsValidFullTownNPC(entity, lateInstantiation) || IsValidTownPet(entity, lateInstantiation);

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => IsAnyValidTownNPC(entity, lateInstantiation);

    public override void SetDefaults(NPC entity) {
        if (ModLoader.isLoading) {
            return;
        }

        List<GlobalNPC> entityGlobals = [];
        foreach (GlobalNPC npc in entity.EntityGlobals) {
            entityGlobals.Add(npc);
        }

        _prioritizedModules = entityGlobals.OfType<TownNPCModule>().OrderBy(module => module.UpdatePriority).ToList();
    }

    public override void ModifyNPCNameList(NPC npc, List<string> nameList) {
        string additionalNameListKey = $"AdditionalTownNPCNames.{LWMUtils.GetNPCTypeNameOrIDName(npc.type)}".PrependModKey();
        if (!AdditionalTownNPCNames.TryGetValue(npc.type, out LocalizedTextGroup additionalNames)) {
            AdditionalTownNPCNames[npc.type] = additionalNames = new LocalizedTextGroup(Lang.CreateDialogFilter(additionalNameListKey));
        }

        nameList.AddRange(additionalNames.Texts.Select(text => text.Value));
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