#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Interfaces;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems.UI;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.DataStructures.Records;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.GameContent;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;

/// <summary>
///     Patches that assists with the various overhauls of the Town NPC Revitalization.
/// </summary>
public class RevitalizationNPCPatches : LoadablePatch {
    private const float MinCostModifier = 0.67f;
    private const float MaxCostModifier = 1.5f;

    private static readonly Gradient<float> ShopCostModifierGradient = new (MathHelper.Lerp, (0f, MaxCostModifier), (0.5f, 1f), (1f, MinCostModifier));

    private static readonly MethodInfo AddExtrasToNPCDrawingInfo = typeof(RevitalizationNPCPatches).GetMethod(nameof(AddExtrasToNPCDrawing), BindingFlags.Public | BindingFlags.Static)!;
    private static MethodBody? _drawNPCExtrasBody;
    private ILHook? _addExtrasToNPCDrawingHook;

    public static void ProcessMoodOverride(ShopHelper shopHelper, Player player, NPC npc) {
        if (NPCID.Sets.NoTownNPCHappiness[npc.type] || !npc.TryGetGlobalNPC(out TownNPCMoodModule moodModule)) {
            shopHelper._currentHappiness = "";
            shopHelper._currentPriceAdjustment = 1f;
            return;
        }

        moodModule.ClearModifiers();

        // Happiness bar will disappear if the string is empty for certain NPCs
        // TODO: Dialect compatibility
        shopHelper._currentHappiness =
            "Whoops! You're not supposed to see this. If you have the mod Dialect installed and enabled, make sure it's set to \"Vanilla\" for the happiness button to work."
            + " If you're not using Dialect and/or it's not enabled. Please report this to the Living World Mod developers!";

        List<NPC> npcNeighbors = shopHelper.GetNearbyResidentNPCs(npc, out int npcsWithinHouse, out int npcsWithinVillage);
        bool[] npcNeighborsByType = new bool[NPCLoader.NPCCount];
        foreach (NPC npcNeighbor in npcNeighbors) {
            npcNeighborsByType[npcNeighbor.type] = true;
        }

        PersonalityHelperInfo info = new(player, npc, npcNeighbors, npcNeighborsByType, npcsWithinHouse, npcsWithinVillage);
        if (TownNPCDataSystem.PersonalityDatabase.TryGetValue(npc.type, out List<IPersonalityTrait>? personalityTraits)) {
            foreach (IPersonalityTrait shopModifier in personalityTraits) {
                shopModifier.ApplyTrait(info, shopHelper);
            }
        }

        HappinessUISystem.Instance.UIState.RefreshModifierList();

        shopHelper._currentPriceAdjustment = ShopCostModifierGradient.GetValue(moodModule.CurrentMood / TownNPCMoodModule.MaxMoodValue);
    }

    /// <summary>
    ///     A method that, at runtime, is IL edited to be an exact copy of <see cref="Main.DrawNPCExtras" />, with all Draw calls replaced with <see cref="TownNPCSpriteModule.RequestDraw" /> calls instead.
    ///     This is done to ensure compability with any other mods calling <see cref="Main.DrawNPCExtras" /> or IL editing it.
    /// </summary>
    /// <remarks>
    ///     The parameter <see cref="parameterPadding" /> is required due to the original <see cref="Main.DrawNPCExtras" /> method being non-<see langword="static" />. Ldarg.0 is never used within the method
    ///     however, so we add the additional argument "padding" so that all the remaining ldarg.# instructions reference the proper argument. This parameter doesn't do anything, so it can be any value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AddExtrasToNPCDrawing(
        bool parameterPadding,
        NPC npc,
        bool beforeDraw,
        float addHeight,
        float addY,
        Color npcColor,
        Vector2 halfSize,
        SpriteEffects npcSpriteEffect,
        Vector2 screenPosition
    ) { }

    private static void DrawNPCExtrasConsumptionPatch(ILContext il) {
        currentContext = il;

        // Edit that "consumes" the SpriteBatch calls in DrawNPCExtras and re-routes them to TownNPCSpriteModule
        ILCursor c = new (il);
        LWMUtils.CloneMethodBodyToCursor(_drawNPCExtrasBody, c);

        Type[] drawParameterTypes = [typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float)];
        MethodInfo spriteBatchMethod = typeof(SpriteBatch).GetMethod(
            nameof(SpriteBatch.Draw),
            BindingFlags.Instance | BindingFlags.Public,
            drawParameterTypes
        )!;

        while (c.TryGotoNext(i => i.MatchCallvirt(spriteBatchMethod))) {
            c.Remove();
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate(SpritebatchRerouteMethod);
        }

        _drawNPCExtrasBody = null;
    }

    private static void SpritebatchRerouteMethod(
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
        if (!npc.TryGetGlobalNPC(out TownNPCSpriteModule spriteModule)) {
            return;
        }

        int drawLayer = beforeDraw ? -1 : 1;
        spriteModule.RequestDraw(new TownNPCDrawRequest(texture, position, origin, sourceRect, color, rotation, new Vector2(scale), effects, true,  drawLayer));
    }

    private static void HappinessUIPatch(ILContext il) {
        // Patch that opens up our Happiness UI instead of displaying the NPC text.
        currentContext = il;

        ILCursor c = new(il);

        c.GotoNext(i => i.MatchLdsfld<Main>(nameof(Main.npcChatFocus4)));
        c.GotoNext(i => i.MatchRet());
        c.EmitDelegate(() => {
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
        c.EmitDelegate<Func<NPC, bool>>(npc => {
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

    private static void DrawsPartyHatPatch(ILContext il) {
        // Small edit that will block drawing an NPC's party hat during a party if they wish to sleep
        currentContext = il;

        ILCursor c = new (il);

        c.Emit(OpCodes.Ldarg_0);
        // If this delegate returns false, prevent hat drawing
        c.EmitDelegate<Func<NPC, bool>>(npc => {
                if (!npc.TryGetGlobalNPC(out TownNPCSleepModule sleepModule)) {
                    return true;
                }

                return !sleepModule.WantsToSleep;
            }
        );
        Instruction branchInstr = c.Emit(OpCodes.Brfalse_S, c.DefineLabel()).Prev;

        c.ErrorOnFailedGotoNext(i => i.MatchLdcI4(0), i => i.MatchRet());
        branchInstr.Operand = c.MarkLabel();
    }

    private static void SkipVanillaFindFrame(On_NPC.orig_VanillaFindFrame orig, NPC self, int num, bool isLikeATownNPC, int type) {
        // Skips VanillaFindFrame for any NPCs that have the Revitalization enabled, since we handle it ourselves in TownNPCAnimationModule.cs
        if (TownGlobalNPC.IsAnyValidTownNPC(self, true)) {
            return;
        }

        orig(self, num, isLikeATownNPC, type);
    }

    public override void LoadPatches() {
        IL_Main.DrawNPCExtras += il => _drawNPCExtrasBody = il.Body;
        _addExtrasToNPCDrawingHook = new ILHook(AddExtrasToNPCDrawingInfo, DrawNPCExtrasConsumptionPatch);

        IL_Main.GUIChatDrawInner += HappinessUIPatch;
        IL_ShopHelper.ProcessMood += ProcessMoodOverridePatch;
        IL_NPC.UpdateCollision += NPCCollisionUpdatePatch;
        IL_NPC.UsesPartyHat += DrawsPartyHatPatch;

        On_NPC.VanillaFindFrame += SkipVanillaFindFrame;
    }

    public override void Unload() {
        _addExtrasToNPCDrawingHook?.Dispose();
        _addExtrasToNPCDrawingHook = null;
    }
}