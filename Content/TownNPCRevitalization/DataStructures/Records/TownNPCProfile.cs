using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using Newtonsoft.Json;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

/// <summary>
/// Data Structure that represents/holds a "Profile" of a Town NPC, which includes their attack/mood/event/sleep/etc. information.
/// </summary>
public readonly record struct TownNPCProfile(
    TownNPCAttackData AttackData,
    List<IPersonalityTrait> Traits,
    SleepSchedule SleepSchedule,
    SleepThresholds SleepThresholds,
    OverlayProfile OverlayProfile
);