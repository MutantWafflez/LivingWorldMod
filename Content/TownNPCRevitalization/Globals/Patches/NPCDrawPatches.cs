using System;
using System.Collections.Generic;
using System.Reflection;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches related to drawing NPCs.
/// </summary>
public sealed class NPCDrawPatches : LoadablePatch {
    public override void LoadPatches() {
        IL_Main.DrawNPCExtras += DrawNPCExtrasConsumptionEdit;
    }

    private static void DrawNPCExtrasConsumptionEdit(ILContext il) {
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
        if (!npc.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
            return false;
        }

        int drawLayer = beforeDraw ? -1 : 1;
        globalNPC.SpriteModule.RequestDraw(new TownNPCDrawRequest(texture, position, sourceRect, Origin: origin, SpriteEffect: effects, UsesAbsolutePosition: true, DrawLayer: drawLayer));
        return true;
    }
}