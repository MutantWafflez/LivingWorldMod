using System;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

public readonly record struct SleepProfile(TimeOnly StartTime, TimeOnly EndTime);