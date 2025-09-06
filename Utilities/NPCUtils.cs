using System;
using System.Collections.Generic;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.DataStructures.Structs;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.Villages.Globals.Systems;
using LivingWorldMod.Content.Villages.HarpyVillage.Materials;
using LivingWorldMod.Content.Villages.HarpyVillage.NPCs;
using LivingWorldMod.DataStructures.Structs;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Utilities;

// Class that handles all utility functions for NPCs.
public static partial class LWMUtils {
    /// <summary>
    ///     Returns the price multiplier that will affect shop prices depending on the status of a
    ///     villager's relationship with the players.
    /// </summary>
    public static float GetPriceMultiplierFromRep(Villager villager) {
        if (villager.RelationshipStatus == VillagerRelationship.Neutral) {
            return 1f;
        }

        ReputationThresholdData thresholds = ReputationSystem.Instance.villageThresholdData[villager.VillagerType];

        float reputationValue = ReputationSystem.Instance.GetNumericVillageReputation(villager.VillagerType);
        float centerPoint = (thresholds.likeThreshold - thresholds.dislikeThreshold) / 2f;

        return MathHelper.Clamp(1 - reputationValue / (ReputationSystem.VillageReputationConstraint - centerPoint) / 2f, 0.67f, 1.67f);
    }

    /// <summary>
    ///     Returns the count of all defined villager types as defined by the <seealso cref="VillagerType" /> enum.
    /// </summary>
    /// <returns></returns>
    public static int GetTotalVillagerTypeCount() => Enum.GetValues<VillagerType>().Length;

    /// <summary>
    ///     Gets &amp; returns the respective NPC for the given villager type passed in.
    /// </summary>
    /// <param name="type"> The type of villager you want to NPC equivalent of. </param>
    public static int VillagerTypeToNPCType(VillagerType type) {
        return type switch {
            VillagerType.Harpy => ModContent.NPCType<HarpyVillager>(),
            _ => -1
        };
    }

    /// <summary>
    ///     Gets &amp; returns the respective Respawn Item for the given villager type passed in.
    /// </summary>
    /// <param name="type"> The type of villager you want to Respawn Item equivalent of. </param>
    public static int VillagerTypeToRespawnItemType(VillagerType type) {
        return type switch {
            VillagerType.Harpy => ModContent.ItemType<HarpyEgg>(),
            _ => -1
        };
    }

    /// <summary>
    ///     Returns whether or not the specified entity is under a roof or not.
    /// </summary>
    /// <param name="entity"> The entity in question. </param>
    /// <param name="maxRoofHeight"> The maximum height from the top of the entity that can be considered to be a "roof".  </param>
    /// <returns></returns>
    public static bool IsEntityUnderRoof(Entity entity, int maxRoofHeight = 32) {
        for (int i = 0; i < maxRoofHeight; i++) {
            if (!WorldGen.InWorld((int)((entity.Center.X + entity.direction) / 16), (int)(entity.Center.Y / 16) - i)) {
                return false;
            }

            Tile tile = Framing.GetTileSafely(entity.Center.ToTileCoordinates16() + new Point16(0, -i));
            if (tile.HasTile && Main.tileSolid[tile.TileType]) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Returns a precise version of the rectangle bounding box of this npc.
    /// </summary>
    public static PreciseRectangle GetPreciseRectangle(this NPC npc) => new(npc.position, npc.Size);

    /// <summary>
    ///     Returns the first NPC that meets the passed in function requirement.
    /// </summary>
    /// <remarks>
    ///     Note that <see cref="NPC.active" /> is checked by default, along-side the predicate.
    /// </remarks>
    public static NPC GetFirstNPC(Predicate<NPC> npcPredicate) {
        foreach (NPC npc in Main.ActiveNPCs) {
            if (npcPredicate.Invoke(npc)) {
                return npc;
            }
        }

        return null;
    }

    /// <summary>
    ///     Returns all NPCs that meet the passed in function requirement.
    /// </summary>
    /// <remarks>
    ///     Note that <see cref="NPC.active" /> is checked by default, along-side the predicate.
    /// </remarks>
    public static List<NPC> GetAllNPCs(Predicate<NPC> npcPredicate) {
        List<NPC> npcList = [];
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc.active && npcPredicate.Invoke(npc)) {
                npcList.Add(npc);
            }
        }

        return npcList;
    }

    /// <summary>
    ///     Gets and returns either the type name (if a modded NPC) or <see cref="NPCID" /> name (if a vanilla NPC), based on the passed in npc type.
    /// </summary>
    public static string GetNPCTypeNameOrIDName(int npcType) => npcType >= NPCID.Count ? NPCLoader.GetNPC(npcType).Name : NPCID.Search.GetName(npcType);

    /// <summary>
    ///     Whether this entity is facing left, i.e. <see cref="Entity.direction" /> is -1.
    /// </summary>
    public static bool IsFacingLeft(this Entity entity) => entity.direction == -1;
}