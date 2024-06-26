using System;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.Localization;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches that deal with Town NPC happiness.
/// </summary>
public sealed class HappinessPatches : LoadablePatch {
    private const float MinCostModifier = 0.67f;
    private const float MaxCostModifier = 1.5f;

    public override void LoadPatches() {
        IL_ShopHelper.ProcessMood += AddToMoodModule;
        IL_ShopHelper.AddHappinessReportText += HijackReportText;
    }

    private void AddToMoodModule(ILContext il) {
        currentContext = il;

        ILCursor c = new(il);

        c.GotoLastInstruction();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_2);
        c.EmitDelegate<Action<ShopHelper, NPC>>((shopHelper, npc) => {
            if (!npc.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
                return;
            }

            TownNPCMoodModule moodModule = globalNPC.MoodModule;
            moodModule.ResetStaticModifiers();
            if (npc.life < npc.lifeMax) {
                moodModule.AddStaticModifier("Injured", LocalizedText.Empty);
            }

            if (BirthdayParty.PartyIsUp && BirthdayParty.GenuineParty && BirthdayParty.CelebratingNPCs.Contains(npc.whoAmI)) {
                moodModule.AddStaticModifier("Party", LocalizedText.Empty);
            }

            float currentMood = moodModule.CurrentMood;
            shopHelper._currentPriceAdjustment = MathHelper.Lerp(MinCostModifier, MaxCostModifier, 1f - currentMood / TownNPCMoodModule.MaxMoodValue);
            // TODO: Localize properly (or replace with full UI for Part 2)
            shopHelper._currentHappiness = "Not empty string here";
            // $"Current Mood: {(int)currentMood}/{(int)TownNPCMoodModule.MaxMoodValue}\n"
            // + string.Join('\n', moodModule.GetFlavorTextAndModifiers().Select(flavorTextAndModifer => {
            //         (string flavorText, float moodModifier) = flavorTextAndModifer;
            //         return $"\"{flavorText}\" ({(moodModifier >= 0 ? "+" : "")}{moodModifier})";
            //     })
            // );
        });
    }

    private void HijackReportText(ILContext il) {
        currentContext = il;

        ILCursor c = new(il);

        c.GotoLastInstruction();
        c.GotoPrev(i => i.MatchCall(typeof(Language), nameof(Language.GetTextValueWith)));

        int flavorTextLocal = -1;
        c.GotoNext(i => i.MatchStloc(out flavorTextLocal));

        c.GotoLastInstruction();
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1);
        c.Emit(OpCodes.Ldloc, flavorTextLocal);
        c.EmitDelegate<Action<ShopHelper, string, string>>((shopHelper, keyCategory, flavorText) => {
            // To prevent the "content" modifier from showing up when other modifiers are present
            shopHelper._currentHappiness = " ";

            // Add modifiers as normal
            if (shopHelper._currentNPCBeingTalkedTo.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
                //TODO: Rework to add into mood system
                globalNPC.MoodModule.AddStaticModifier(keyCategory.Split('_')[0], LocalizedText.Empty);
            }
        });
    }
}