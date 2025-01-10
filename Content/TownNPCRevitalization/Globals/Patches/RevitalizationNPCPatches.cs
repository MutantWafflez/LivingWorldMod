using System;
using System.Collections.Generic;
using System.Reflection;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.DataStructures.Records;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches that assists with the various overhauls of the Town NPC Revitalization.
/// </summary>
public class RevitalizationNPCPatches : LoadablePatch {
    private const float MinCostModifier = 0.67f;
    private const float MaxCostModifier = 1.5f;

    private static readonly Gradient<float> ShopCostModifierGradient = new (MathHelper.Lerp, (0f, MaxCostModifier), (0.5f, 1f), (1f, MinCostModifier));

    public static void ProcessMoodOverride(ShopHelper shopHelper, Player player, NPC npc) {
        if (NPCID.Sets.NoTownNPCHappiness[npc.type] || !npc.TryGetGlobalNPC(out TownNPCMoodModule moodModule)) {
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

        shopHelper._currentPriceAdjustment = ShopCostModifierGradient.GetValue(moodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue);
    }

    private static void DrawNPCExtrasConsumptionPatch(ILContext il) {
        currentContext = il;

        Type[] drawParameterTypes = [typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float)];
        MethodInfo spriteBatchMethod = typeof(SpriteBatch).GetMethod(
            nameof(SpriteBatch.Draw),
            BindingFlags.Instance | BindingFlags.Public,
            drawParameterTypes
        )!;

        // TL;DR: Edit that "consumes" the SpriteBatch calls in DrawNPCExtras and re-routes them to TownNPCSpriteModule
        // Long version: In order to conserve compatibility for other mods editing this method, we find every instance of a SpriteBatch.Draw call and consume all the instructions preceding it, and
        // then copying them so they can be run through our own delegate (see SpecialTownNPCDrawExtras below), capping it off with a branch instruction to preserve the old instructions so that vanilla
        // can use them still for the applicable NPCs. This method provides the best of compatibility as well as actual functionality, instead of straight-up removing the Draw call (like before)

        ILCursor c = new (il);
        while (c.TryGotoNext(i => i.MatchCallvirt(spriteBatchMethod))) {
            int spriteBatchDrawCallInstrIndex = c.Index;
            c.GotoPrev(i => i.MatchLdsfld<Main>(nameof(Main.spriteBatch)));

            List<Instruction> spriteBatchInstrs = [];
            for (int i = c.Index + 1; i < spriteBatchDrawCallInstrIndex; i++) {
                spriteBatchInstrs.Add(c.Instrs[i]);
            }

            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_2);
            foreach (Instruction instr in spriteBatchInstrs) {
                c.Emit(instr.OpCode, instr.Operand);
            }

            c.EmitDelegate(SpecialTownNPCDrawExtras);

            ILLabel branchLabel = c.DefineLabel();
            c.Emit(OpCodes.Brtrue_S, branchLabel);
            c.GotoNext(MoveType.After, i => i.MatchCallvirt(spriteBatchMethod)).MarkLabel(branchLabel);
        }
    }

    private static bool SpecialTownNPCDrawExtras(
        NPC npc,
        bool beforeDraw,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRect,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth
    ) {
        if (!npc.TryGetGlobalNPC(out TownNPCSpriteModule spriteModule)) {
            return false;
        }

        int drawLayer = beforeDraw ? -1 : 1;
        spriteModule.RequestDraw(new TownNPCDrawRequest(texture, position, sourceRect, Origin: origin, SpriteEffect: effects, UsesAbsolutePosition: true, DrawLayer: drawLayer));
        return true;
    }

    private static void HappinessUIPatch(ILContext il) {
        // Patch that opens up our Happiness UI instead of displaying the NPC text.
        currentContext = il;

        ILCursor c = new(il);

        c.GotoNext(i => i.MatchLdsfld<Main>(nameof(Main.npcChatFocus4)));
        c.GotoNext(i => i.MatchRet());
        c.EmitDelegate(
            () => {
                Main.npcChatText = "";
                HappinessUISystem.Instance.OpenHappinessState(Main.LocalPlayer.TalkNPC);
            }
        );
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

    private static void NPCCollisionUpdatePatch(ILContext il) {
        // Hijacks collision for Town NPCs. Gives us full control.
        currentContext = il;

        ILCursor c = new(il);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<NPC, bool>>(
            npc => {
                if (!npc.TryGetGlobalNPC(out TownNPCCollisionModule collisionModule)) {
                    return false;
                }

                collisionModule.UpdateCollision();
                return true;
            }
        );
        c.Emit(OpCodes.Brfalse_S, c.DefineLabel());
        c.Emit(OpCodes.Ret);

        ILLabel normalCollisionLabel = c.MarkLabel();
        c.GotoPrev(i => i.MatchBrfalse(out _));
        c.Next!.Operand = normalCollisionLabel;
    }

    public override void LoadPatches() {
        IL_Main.DrawNPCExtras += DrawNPCExtrasConsumptionPatch;
        IL_Main.GUIChatDrawInner += HappinessUIPatch;
        IL_ShopHelper.ProcessMood += ProcessMoodOverridePatch;
        IL_NPC.UpdateCollision += NPCCollisionUpdatePatch;
    }
}