using System;
using System.Collections.Generic;
using System.Reflection;
using LivingWorldMod.Common.Systems;
using LivingWorldMod.Content.WorldGenFeatures.Villages;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ModLoader;

namespace LivingWorldMod.Core.Patches {
    /// <summary>
    /// Class that contains the IL/On methods for patching various worldgen methods.
    /// </summary>
    public class WorldGenPatches : ILoadable {
        public void Load(Mod mod) {
            IL.Terraria.WorldGen.FillWallHolesInSpot += FillHolesInSpotPatch;
        }

        public void Unload() { }

        private void FillHolesInSpotPatch(ILContext il) {
            //For the Harpy Village (and potentially other structures in the future) we do not want the auto "filling of holes" to occur.
            //This filling of holes causes some houses for the harpy village to have their "supports" filled which destroys how the building is supposed to look
            ILCursor c = new ILCursor(il);

            byte itemLocalNumber = 6; //Called "item" in this case, but this is actually the local variable is the position of the wall "hole"

            //IL is quite simple in this case. We're going to hijack one of the checks that determines if a hole area is going to be filled or not.
            //All we do it return true if the point in question is in the Harpy village zone, which prevents the filling at that point
            c.ErrorOnFailedGotoNext(MoveType.After, i => i.MatchCallvirt(typeof(HashSet<Point>).GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance)));

            c.Emit(OpCodes.Ldloc_S, itemLocalNumber);
            c.EmitDelegate<Func<bool, Point, bool>>((originalValue, point) =>
                originalValue || WorldCreationSystem.Instance.GetTempWorldGenValue<Rectangle>(HarpyVillage.TemporaryZoneVariableName) is Rectangle rectangle && rectangle.Contains(point));
        }
    }
}