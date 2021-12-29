using LivingWorldMod.Common.Systems;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Common.Patches {
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

            //IL is quite simple in this case. All we are doing is going to override flag 5 which controls whether or not a certain
            // hole area is going to be filled or not. All we do it return true if the point in question is in the Harpy village zone, which prevents the filling at that point
            c.ErrorOnFailedGotoNext(i => i.MatchCallvirt(typeof(List<Point>).GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance)));

            //Move to brtrue instruction nearby and steal its pointing label
            c.Index += 9;
            ILLabel stolenTrueLabel = (ILLabel)c.Next.Operand;
            //Then, do our own check
            c.Index++;
            c.Emit(OpCodes.Ldloc_S, itemLocalNumber);
            c.EmitDelegate<Func<Point, bool>>(point => {
                //Checks if Harpy village zone is not null
                if (ModContent.GetInstance<WorldCreationSystem>().villageZones[(int)VillagerType.Harpy] is Rectangle rectangle) {
                    return rectangle.Contains(point);
                }

                return !WorldGen.InWorld(point.X, point.Y, 1);
            });
            c.Emit(OpCodes.Brfalse_S, stolenTrueLabel);
        }
    }
}