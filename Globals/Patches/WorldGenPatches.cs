using System;
using System.Collections.Generic;
using System.Reflection;
using LivingWorldMod.Content.Villages.HarpyVillage.WorldGenFeatures;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Globals.Systems;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LivingWorldMod.Globals.Patches;

/// <summary>
/// Class that contains the IL/On methods for patching various worldgen methods.
/// </summary>
public class WorldGenPatches : LoadablePatch {
    public override void LoadPatches() {
        IL_WorldGen.FillWallHolesInSpot += FillHolesInSpotPatch;
    }

    private void FillHolesInSpotPatch(ILContext il) {
        currentContext = il;

        //For the Harpy Village (and potentially other structures in the future) we do not want the auto "filling of holes" to occur.
        //This filling of holes causes some houses for the harpy village to have their "supports" filled which destroys how the building is supposed to look
        ILCursor c = new(il);

        c.GotoNext(i => i.MatchCallvirt(typeof(HashSet<Point>).GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance)!));

        int spotLocalVar = -1;
        c.GotoPrev(i => i.MatchLdloc(out spotLocalVar));
        c.GotoNext(i => i.MatchBrfalse(out _));

        //IL is quite simple in this case. We're going to hijack one of the checks that determines if a hole area is going to be filled or not.
        //All we do it return true if the point in question is in the Harpy village zone, which prevents the filling at that point
        c.Emit(OpCodes.Ldloc_S, (byte)spotLocalVar);
        c.EmitDelegate<Func<bool, Point, bool>>((originalValue, point) =>
            originalValue || WorldCreationSystem.Instance.GetTempWorldGenValue<Rectangle>(HarpyVillage.TemporaryZoneVariableName) is Rectangle rectangle && rectangle.Contains(point));
    }
}