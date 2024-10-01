using System;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;

public readonly record struct SleepSchedule(TimeOnly StartTime, TimeOnly EndTime);