using System;
using System.Linq;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Classes.TownNPCModules;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.Localization;

namespace LivingWorldMod.Core.Patches;

/// <summary>
/// Patches that deal with Town NPC happiness.
/// </summary>
public sealed class HappinessPatches : LoadablePatch {
    private const float MinCostModifier = 0.67f;
    private const float MaxCostModifier = 1.5f;

    public override void LoadPatches() {
        On_ShopHelper.ProcessMood += MoodOverhaulChanges;
        IL_ShopHelper.AddHappinessReportText += HijackReportText;
    }

    private void MoodOverhaulChanges(On_ShopHelper.orig_ProcessMood orig, ShopHelper self, Player player, NPC npc) {
        orig(self, player, npc);
        if (!npc.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
            return;
        }

        TownNPCMoodModule moodModule = globalNPC.MoodModule;
        if (npc.life < npc.lifeMax) {
            moodModule.AddModifier("Injured", LWMUtils.GetLWMTextValue("TownNPCMoodFlavorText.Injured"), 0);
        }

        if (BirthdayParty.PartyIsUp && BirthdayParty.GenuineParty && BirthdayParty.CelebratingNPCs.Contains(npc.whoAmI)) {
            moodModule.AddModifier("Party", LWMUtils.GetLWMTextValue("TownNPCMoodFlavorText.Party"), 0);
        }

        float currentMood = moodModule.CurrentMood;
        self._currentPriceAdjustment = MathHelper.Lerp(MinCostModifier, MaxCostModifier, 1f - currentMood / TownNPCMoodModule.MaxMoodValue);
        // TODO: Localize properly (or replace with full UI for Part 2)
        self._currentHappiness =
            $"Current Mood: {(int)currentMood}/{(int)TownNPCMoodModule.MaxMoodValue}\n"
            + string.Join('\n', moodModule.GetFlavorTextAndModifiers().Select(flavorTextAndModifer => {
                    (string flavorText, float moodModifier) = flavorTextAndModifer;
                    return $"\"{flavorText}\" ({(moodModifier >= 0 ? "+" : "")}{moodModifier})";
                })
            );
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
                globalNPC.MoodModule.AddModifier(keyCategory.Split('_')[0], flavorText, 0);
            }
        });
    }
}