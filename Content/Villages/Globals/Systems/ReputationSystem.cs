using System;
using System.Collections.Generic;
using Hjson;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.DataStructures.Structs;
using LivingWorldMod.Globals.Systems.BaseSystems;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Content.Villages.Globals.Systems;

/// <summary>
/// System that handles reputations for all the villages in the mod.
/// </summary>
public class ReputationSystem : BaseModSystem<ReputationSystem> {
    public const int VillageReputationConstraint = 100;
    public Dictionary<VillagerType, ReputationThresholdData> villageThresholdData;
    private int[] _villageReputation;

    public override void Load() {
        _villageReputation = new int[LWMUtils.GetTotalVillagerTypeCount()];
        villageThresholdData = new Dictionary<VillagerType, ReputationThresholdData>();

        JsonObject jsonReputationData = LWMUtils.GetJSONFromFile("Assets/JSONData/ReputationData.json").Qo();
        foreach (VillagerType type in Enum.GetValues<VillagerType>()) {
            JsonObject villageSpecificData = jsonReputationData[type.ToString()].Qo();

            villageThresholdData[type] = new ReputationThresholdData(
                villageSpecificData["Hate"].Qi(),
                villageSpecificData["SevereDislike"].Qi(),
                villageSpecificData["Dislike"].Qi(),
                villageSpecificData["Like"].Qi(),
                villageSpecificData["Love"].Qi()
            );
        }
    }

    public override void Unload() {
        _villageReputation = null;
    }

    public override void SaveWorldData(TagCompound tag) {
        tag["VillageReputation"] = _villageReputation;
    }

    public override void LoadWorldData(TagCompound tag) {
        int[] savedVillageRep = tag.GetIntArray("VillageReputation");
        //Make sure to keep the array size up to date in case a player is loading an old world
        if (savedVillageRep.Length != _villageReputation.Length) {
            Array.Resize(ref savedVillageRep, _villageReputation.Length);
            _villageReputation = savedVillageRep;
        }
        else {
            _villageReputation = savedVillageRep;
        }
    }

    /// <summary>
    /// Gets & returns the reputation value of the given villager type specified.
    /// </summary>
    /// <returns></returns>
    public int GetNumericVillageReputation(VillagerType villagerType) => _villageReputation[(int)villagerType];

    /// <summary>
    /// The current status of the "relationship" between the specified villager type and the players.
    /// Returns the enum of said status.
    /// </summary>
    //TODO: Revert back to commented expression when Reputation system is re-implemented
    public VillagerRelationship GetVillageRelationship(VillagerType villagerType) => VillagerRelationship.Love; /*{
        int reputation = GetNumericVillageReputation(villagerType);
        ReputationThresholdData thresholds = villageThresholdData[villagerType];

        if (reputation <= thresholds.hateThreshold) {
            return VillagerRelationship.Hate;
        }
        else if (reputation > thresholds.hateThreshold && reputation <= thresholds.severeDislikeThreshold) {
            return VillagerRelationship.SevereDislike;
        }
        else if (reputation > thresholds.severeDislikeThreshold && reputation <= thresholds.dislikeThreshold) {
            return VillagerRelationship.Dislike;
        }
        else if (reputation >= thresholds.likeThreshold && reputation < thresholds.loveThreshold) {
            return VillagerRelationship.Like;
        }
        else if (reputation >= thresholds.loveThreshold) {
            return VillagerRelationship.Love;
        }

        return VillagerRelationship.Neutral;
    }*/

    /// <summary>
    /// Changes the value of the specified village type's reputation BY the specified amount.
    /// </summary>
    public void ChangeVillageReputation(VillagerType villagerType, int changeAmount) {
        _villageReputation[(int)villagerType] += changeAmount;

        _villageReputation[(int)villagerType] = (int)MathHelper.Clamp(_villageReputation[(int)villagerType], -VillageReputationConstraint, VillageReputationConstraint);
    }

    /// <summary>
    /// Sets the value of the specified village type's reputation TO the specified amount.
    /// </summary>
    public void SetVillageReputation(VillagerType villagerType, int setValue) {
        _villageReputation[(int)villagerType] = setValue;

        _villageReputation[(int)villagerType] = (int)MathHelper.Clamp(_villageReputation[(int)villagerType], -VillageReputationConstraint, VillageReputationConstraint);
    }
}