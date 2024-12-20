using System;
using System.Reflection;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.DataStructures.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches related to drawing NPCs.
/// </summary>
public sealed class NPCDrawPatches : LoadablePatch {
    public override void LoadPatches() {
        IL_Main.DrawNPCExtras += DrawNPCExtrasConsumptionEdit;
    }

    private void DrawNPCExtrasConsumptionEdit(ILContext il) {
        currentContext = il;

        Type[] drawParameterTypes = [typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float)];
        MethodInfo spriteBatchMethod = typeof(SpriteBatch).GetMethod(
            nameof(SpriteBatch.Draw),
            BindingFlags.Instance | BindingFlags.Public,
            drawParameterTypes
        );

        // Edit that "consumes" the SpriteBatch calls in DrawNPCExtras and re-routes them to TownNPCSpriteModule

        ILCursor c = new (il);
        while (c.TryGotoNext(i => i.MatchCallvirt(spriteBatchMethod!))) {
            c.Remove();
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate(SpecialTownNPCDrawExtras);
        }
    }

    private void SpecialTownNPCDrawExtras(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRect,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth,
        NPC npc,
        bool beforeDraw
    ) {
        if (!npc.TryGetGlobalNPC(out TownGlobalNPC globalNPC)) {
            spriteBatch.Draw(texture, position, sourceRect, color, rotation, origin, scale, effects, layerDepth);
            return;
        }
        
        globalNPC.SpriteModule.RequestDraw(new TownNPCDrawRequest(texture, position, sourceRect, Origin: origin, UsesAbsolutePosition: true, DrawLayer: beforeDraw ? -1 : 1));
    }
}