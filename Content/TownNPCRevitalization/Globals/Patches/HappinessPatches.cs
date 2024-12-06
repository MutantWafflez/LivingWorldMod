using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.DataStructures.Records;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches that deal with Town NPC happiness.
/// </summary>
[Autoload(false)]
public sealed class HappinessPatches : LoadablePatch {
    private const float MinCostModifier = 0.67f;
    private const float MaxCostModifier = 1.5f;

    private static readonly Gradient<float> ShopCostModifierGradient = new (MathHelper.Lerp, (0f, MaxCostModifier), (0.5f, 1f), (1f, MinCostModifier));

    public static void ProcessMoodOverride(ShopHelper shopHelper, Player player, NPC npc) {
        if (NPCID.Sets.NoTownNPCHappiness[npc.type] || !npc.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
            shopHelper._currentHappiness = "";
            shopHelper._currentPriceAdjustment = 1f;
            return;
        }

        // Happiness bar will disappear if the string is empty for certain NPCs
        shopHelper._currentHappiness = "A non-empty string";

        List<NPC> npcNeighbors = shopHelper.GetNearbyResidentNPCs(npc, out int npcsWithinHouse, out int npcsWithinVillage);
        bool[] npcNeighborsByType = new bool[NPCLoader.NPCCount];
        foreach (NPC npcNeighbor in npcNeighbors) {
            npcNeighborsByType[npcNeighbor.type] = true;
        }

        PersonalityHelperInfo info = new(player, npc, npcNeighbors, npcNeighborsByType, npcsWithinHouse, npcsWithinVillage);
        if (TownNPCDataSystem.PersonalityDatabase.TryGetValue(npc.type, out List<IPersonalityTrait> personalityTraits)) {
            foreach (IPersonalityTrait shopModifier in personalityTraits) {
                shopModifier.ApplyTrait(info, shopHelper);
            }
        }

        shopHelper._currentPriceAdjustment = ShopCostModifierGradient.GetValue(globalNPC.MoodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue);
    }

    private static void ProcessMoodOverridePatch(ILContext il) {
        currentContext = il;

        ILCursor c = new(il);

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1);
        c.Emit(OpCodes.Ldarg_2);
        c.EmitDelegate(ProcessMoodOverride);
        c.Emit(OpCodes.Ret);
    }

    public override void LoadPatches() {
        IL_ShopHelper.ProcessMood += ProcessMoodOverridePatch;
    }
}