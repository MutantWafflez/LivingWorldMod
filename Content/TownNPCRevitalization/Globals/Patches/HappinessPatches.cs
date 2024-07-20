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

    /// <summary>
    ///     The first out value of <see cref="ShopHelper.GetNearbyResidentNPCs" />.
    /// </summary>
    public static int NPCCountWithinHouse {
        get;
        private set;
    }

    /// <summary>
    ///     The second out value of <see cref="ShopHelper.GetNearbyResidentNPCs" />.
    /// </summary>
    public static int NPCCountWithinVillage {
        get;
        private set;
    }

    public static void ProcessMoodOverride(ShopHelper shopHelper, Player player, NPC npc) {
        TownGlobalNPC globalNPC = npc.GetGlobalNPC<TownGlobalNPC>();

        // Happiness bar will disappear if the string is empty for certain NPCs
        shopHelper._currentHappiness = "A non-empty string";
        globalNPC.MoodModule.ResetStaticModifiers();

        List<NPC> npcNeighbors = shopHelper.GetNearbyResidentNPCs(npc, out int npcsWithinHouse, out int npcsWithinVillage);
        NPCCountWithinHouse = npcsWithinHouse;
        NPCCountWithinVillage = npcsWithinVillage;

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

        shopHelper._currentPriceAdjustment = MathHelper.Lerp(MinCostModifier, MaxCostModifier, 1f - globalNPC.MoodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue);
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