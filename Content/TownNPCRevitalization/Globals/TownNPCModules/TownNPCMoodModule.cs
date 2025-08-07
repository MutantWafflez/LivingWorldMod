using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

/// <summary>
///     Class that handles the new "mood" feature that replaces Town NPC happiness.
/// </summary>
public sealed class TownNPCMoodModule : TownNPCModule {
    public const float MaxMoodValue = 100f;
    public const float MinMoodValue = 0f;

    private const int UpdateMoodThreshold = LWMUtils.RealLifeSecond * 15;

    private readonly List<MoodModifierInstance> _currentMoodModifiers = [];

    private int _moodUpdateTimer;

    public override int UpdatePriority => 2;

    public IReadOnlyList<MoodModifierInstance> CurrentMoodModifiers => _currentMoodModifiers;

    public float CurrentMood => Utils.Clamp(
        BaseMoodValue + CurrentMoodModifiers.Sum(instance => instance.MoodOffset),
        MinMoodValue,
        MaxMoodValue
    );

    private static int BaseMoodValue => 50;

    /// <summary>
    ///     Returns the flavor text localization key prefix for the given npc, accounting for if the npc is modded or not.
    /// </summary>
    public static string GetFlavorTextKeyPrefix(NPC npc) => npc.ModNPC is not null ? npc.ModNPC.GetLocalizationKey("TownNPCMood") : $"TownNPCMood_{NPCID.Search.GetName(npc.type)}";

    /// <summary>
    ///     Returns the flavor text localization key prefix for the given npc type, accounting for if the npc is modded or not.
    /// </summary>
    public static string GetFlavorTextKeyPrefix(int npcType) => npcType >= NPCID.Count ? NPCLoader.GetNPC(npcType).GetLocalizationKey("TownNPCMood") : $"TownNPCMood_{NPCID.Search.GetName(npcType)}";

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => TownGlobalNPC.IsValidFullTownNPC(entity, lateInstantiation) && !NPCID.Sets.NoTownNPCHappiness[entity.type];

    public override void UpdateModule() {
        if (Main.LocalPlayer.TalkNPC is null || Main.npcShop > 0 || ++_moodUpdateTimer < UpdateMoodThreshold) {
            return;
        }

        _moodUpdateTimer = 0;

        RevitalizationNPCPatches.ProcessMoodOverride(Main.ShopHelper, Main.LocalPlayer, NPC);
    }

    public void AddModifier(DynamicLocalizedText descriptionText, DynamicLocalizedText flavorText, int moodOffset) {
        _currentMoodModifiers.Add(new MoodModifierInstance (descriptionText, flavorText, moodOffset));
    }

    public void ClearModifiers() {
        _currentMoodModifiers.Clear();
    }
}