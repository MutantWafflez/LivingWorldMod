using LivingWorldMod.Common.Systems;
using LivingWorldMod.Custom.Enums;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
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

            byte itemLocalNumber = 8; //Called "item" in this case, but this is actually the local variable is the position of the wall "hole"
            byte flagFiveLocalNumber = 13;

            //IL is quite simple in this case. All we are doing is going to override flag 5 which controls whether or not a certain
            // hole area is going to be filled or not. All we do it return true if the point in question is in the Harpy village zone, which prevents the filling at that point
            c.ErrorOnFailedGotoNext(i => i.MatchStloc(flagFiveLocalNumber));

            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldloc_S, itemLocalNumber);
            c.EmitDelegate<Func<Point, bool>>(point => {
                //Checks if Harpy village zone is not null
                if (ModContent.GetInstance<WorldCreationSystem>().villageZones[(int)VillagerType.Harpy] is Rectangle rectangle) {
                    return rectangle.Contains(point);
                }

                return !WorldGen.InWorld(point.X, point.Y, 1);
            });

            //IL block in above edit ^:
            /*/* (4813,6)-(4813,38) tModLoader\src\tModLoader\Terraria\WorldGen.cs #1#
            /* 0x004807F2 1108         #1# IL_00A2: ldloc.s   item
            /* 0x004807F4 7B0E02000A   #1# IL_00A4: ldfld     int32 [FNA]Microsoft.Xna.Framework.Point::X
            /* 0x004807F9 1108         #1# IL_00A9: ldloc.s   item
            /* 0x004807FB 7B0F02000A   #1# IL_00AB: ldfld     int32 [FNA]Microsoft.Xna.Framework.Point::Y
            /* 0x00480800 17           #1# IL_00B0: ldc.i4.1
            /* 0x00480801 28D40D0006   #1# IL_00B1: call      bool Terraria.WorldGen::InWorld(int32, int32, int32)
            /* 0x00480806 16           #1# IL_00B6: ldc.i4.0
            /* 0x00480807 FE01         #1# IL_00B7: ceq
            /* 0x00480809 130D         #1# IL_00B9: stloc.s   V_13
            /* (hidden)-(hidden) tModLoader\src\tModLoader\Terraria\WorldGen.cs #1#
            /* 0x0048080B 110D         #1# IL_00BB: ldloc.s   V_13
            /* 0x0048080D 2C0F         #1# IL_00BD: brfalse.s IL_00CE*/
        }
    }
}