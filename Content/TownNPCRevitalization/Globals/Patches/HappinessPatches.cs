using System;
using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches that deal with Town NPC happiness.
/// </summary>
public sealed class HappinessPatches : LoadablePatch {
    private const float MinCostModifier = 0.67f;
    private const float MaxCostModifier = 1.5f;

    private static void AddToMoodModule(ILContext il) {
        currentContext = il;

        ILCursor c = new(il);

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1);
        c.Emit(OpCodes.Ldarg_2);
        c.EmitDelegate<Action<ShopHelper, Player, NPC>>(
            (shopHelper, player, npc) => {
                TownGlobalNPC globalNPC = npc.GetGlobalNPC<TownGlobalNPC>();

                // Happiness bar will disappear if the string is empty for certain NPCs
                shopHelper._currentHappiness = "A non-empty string";
                globalNPC.MoodModule.ResetStaticModifiers();

                List<NPC> npcNeighbors = shopHelper.GetNearbyResidentNPCs(npc, out int npcsWithinHouse, out int npcsWithinVillage);
                bool[] npcNeighborsByType = new bool[NPCLoader.NPCCount];
                foreach (NPC npcNeighbor in npcNeighbors) {
                    npcNeighborsByType[npcNeighbor.type] = true;
                }

                HelperInfo info = new() { player = player, npc = npc, NearbyNPCs = npcNeighbors, nearbyNPCsByType = npcNeighborsByType };
                if (shopHelper._database.TryGetProfileByNPCID(npc.type, out PersonalityProfile profile)) {
                    foreach (IShopPersonalityTrait shopModifier in profile.ShopModifiers) {
                        shopModifier.ModifyShopPrice(info, shopHelper);
                    }
                }

                // TODO: Is this necessary?
                new AllPersonalitiesModifier().ModifyShopPrice(info, shopHelper);
                shopHelper._currentPriceAdjustment = MathHelper.Lerp(MinCostModifier, MaxCostModifier, 1f - globalNPC.MoodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue);
            }
        );
        c.Emit(OpCodes.Ret);

        // c.GotoLastInstruction();
        // c.Emit(OpCodes.Ldarg_0);
        // c.Emit(OpCodes.Ldarg_2);
        // c.EmitDelegate<Action<ShopHelper, NPC>>(
        //     (shopHelper, npc) => {
        //         if (!npc.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
        //             return;
        //         }
        //
        //         TownNPCMoodModule moodModule = globalNPC.MoodModule;
        //
        //         shopHelper._currentPriceAdjustment = MathHelper.Lerp(MinCostModifier, MaxCostModifier, 1f - moodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue);
        //     }
        // );
    }

    // private static void HijackReportText(ILContext il) {
    //     currentContext = il;
    //
    //     ILCursor c = new(il);
    //
    //     // c.GotoLastInstruction();
    //     // c.GotoPrev(i => i.MatchCall(typeof(Language), nameof(Language.GetTextValueWith)));
    //
    //     int townNPCNameKeyLocal = -1;
    //     c.GotoNext(i => i.MatchStloc(out townNPCNameKeyLocal));
    //
    //     c.GotoLastInstruction();
    //     c.Emit(OpCodes.Ldarg_0);
    //     c.Emit(OpCodes.Ldloc, townNPCNameKeyLocal);
    //     c.Emit(OpCodes.Ldarg_1);
    //     c.Emit(OpCodes.Ldarg_2);
    //     c.EmitDelegate<Action<ShopHelper, string, string, object>>(
    //         (shopHelper, townNPCLocalizationKey, moodModifierKey, flavorTextSubstituteObject) => {
    //             if (shopHelper._currentNPCBeingTalkedTo.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
    //                 globalNPC.MoodModule.ConvertReportTextToStaticModifier(townNPCLocalizationKey, moodModifierKey, flavorTextSubstituteObject);
    //             }
    //         }
    //     );
    // }

    public override void LoadPatches() {
        IL_ShopHelper.ProcessMood += AddToMoodModule;
        // IL_ShopHelper.AddHappinessReportText += HijackReportText;
    }
}