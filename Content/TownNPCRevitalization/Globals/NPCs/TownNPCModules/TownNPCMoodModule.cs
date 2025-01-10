using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using LivingWorldMod.DataStructures.Structs;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;

/// <summary>
///     Class that handles the new "mood" feature that replaces Town NPC happiness.
/// </summary>
public sealed class TownNPCMoodModule : TownNPCModule {
    public const float MaxMoodValue = 100f;
    public const float MinMoodValue = 0f;

    private readonly List<MoodModifierInstance> _currentMoodModifiers = [];

    public override int UpdatePriority => 2;

    public IReadOnlyList<MoodModifierInstance> CurrentMoodModifiers => _currentMoodModifiers;

    public float CurrentMood => Utils.Clamp(
        BaseMoodValue + CurrentMoodModifiers.Sum(instance => instance.moodOffset),
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

    public override void UpdateModule(NPC npc) {
        for (int i = 0; i < _currentMoodModifiers.Count; i++) {
            MoodModifierInstance instance = _currentMoodModifiers[i];
            if (--instance.duration <= 0) {
                _currentMoodModifiers.RemoveAt(i--);
            }
            else {
                _currentMoodModifiers[i] = instance;
            }
        }

        if (Main.LocalPlayer.talkNPC == npc.whoAmI && Main.npcShop == 0)  {
            RevitalizationNPCPatches.ProcessMoodOverride(Main.ShopHelper, Main.LocalPlayer, npc);
        }
    }

    public void AddModifier(DynamicLocalizedText descriptionText, DynamicLocalizedText flavorText, int moodOffset, int duration = 1) {
        _currentMoodModifiers.Add(new MoodModifierInstance (descriptionText, flavorText, moodOffset, duration));
    }
}