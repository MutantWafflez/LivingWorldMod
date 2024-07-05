using System;
using System.Text.RegularExpressions;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches that deal with Town NPC happiness.
/// </summary>
public sealed partial class HappinessPatches : LoadablePatch {
    private const float MinCostModifier = 0.67f;
    private const float MaxCostModifier = 1.5f;

    private static readonly Regex TownNPCNameRegex = LoadNPCNameRegex();

    [GeneratedRegex(@"(.+\.(?<Name>.+)\.TownNPCMood|TownNPCMood_(?<Name>.+))")]
    private static partial Regex LoadNPCNameRegex();

    private static void AddToMoodModule(ILContext il) {
        currentContext = il;

        ILCursor c = new(il);

        // Jump AFTER vanilla _currentHappiness set, so our "Content" Mood modifier is properly ignored
        c.GotoNext(MoveType.After, i => i.Match(OpCodes.Stfld));
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_2);
        c.EmitDelegate<Action<ShopHelper, NPC>>((shopHelper, npc) => {
            if (!npc.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
                return;
            }

            // To prevent the "content" modifier from showing up when other modifiers are present
            shopHelper._currentHappiness = " ";

            globalNPC.MoodModule.ResetStaticModifiers();
        });

        c.GotoLastInstruction();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_2);
        c.EmitDelegate<Action<ShopHelper, NPC>>((shopHelper, npc) => {
            if (!npc.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
                return;
            }

            TownNPCMoodModule moodModule = globalNPC.MoodModule;

            shopHelper._currentPriceAdjustment = MathHelper.Lerp(MinCostModifier, MaxCostModifier, 1f - moodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue);
        });
    }

    private static void HijackReportText(ILContext il) {
        currentContext = il;

        ILCursor c = new(il);

        // c.GotoLastInstruction();
        // c.GotoPrev(i => i.MatchCall(typeof(Language), nameof(Language.GetTextValueWith)));

        int townNPCNameKeyLocal = -1;
        c.GotoNext(i => i.MatchStloc(out townNPCNameKeyLocal));

        c.GotoLastInstruction();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloc, townNPCNameKeyLocal);
        c.Emit(OpCodes.Ldarg_1);
        c.Emit(OpCodes.Ldarg_2);
        c.EmitDelegate<Action<ShopHelper, string, string, object>>((shopHelper, townNPCLocalizationKey, moodModifierKey, flavorTextSubstituteObject) => {
            // Add modifiers as normal
            if (shopHelper._currentNPCBeingTalkedTo.TryGetGlobalNPC(out TownGlobalNPC globalNPC) && TownNPCNameRegex.Match(townNPCLocalizationKey) is { } match && match != Match.Empty) {
                // We split moodModifierKey for scenarios such as LovesNPC_Princess, where we want the mood modifier to be "LovesNPC" as a catch-all
                globalNPC.MoodModule.AddStaticModifier(moodModifierKey.Split('_')[0], Language.GetText($"{townNPCLocalizationKey}.{moodModifierKey}"), flavorTextSubstituteObject);
            }
        });
    }

    public override void LoadPatches() {
        IL_ShopHelper.ProcessMood += AddToMoodModule;
        IL_ShopHelper.AddHappinessReportText += HijackReportText;
    }
}