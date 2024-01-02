using System;
using LivingWorldMod.Common.Sets;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Utilities;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ObjectData;

namespace LivingWorldMod.Core.Patches;

/// <summary>
/// Handles all patches that relate to tile drawing.
/// </summary>
public class TileDrawingPatches : LoadablePatch {
    public override void LoadPatches() {
        IL_TileDrawing.DrawMultiTileVines += WindDrawingEdit;
    }


    private void WindDrawingEdit(ILContext il) {
        currentContext = il;

        // This edit allows for this mod to properly incorporate wind-sway for larger tiles
        ILCursor c = new(il);

        byte sizeXVarIndex = 7;
        byte sizeYVarIndex = 8;
        byte tileVarIndex = 9;

        c.ErrorOnFailedGotoNext(i => i.MatchCall<TileDrawing>("DrawMultiTileVinesInWind"));
        c.Index -= 6;
        // We do this so we don't have to mess with the branches
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Ldarg_0);
        c.Index--;
        //Size X
        c.Emit(OpCodes.Ldloc_S, sizeXVarIndex);
        c.Emit(OpCodes.Ldloc_S, tileVarIndex);
        c.EmitDelegate<Func<int, Tile, int>>((sizeX, tile) => TileSets.NeedsAdvancedWindSway[tile.TileType] ? TileObjectData.GetTileData(tile).Width : sizeX);
        c.Emit(OpCodes.Stloc_S, sizeXVarIndex);
        //Size Y
        c.Emit(OpCodes.Ldloc_S, sizeYVarIndex);
        c.Emit(OpCodes.Ldloc_S, tileVarIndex);
        c.EmitDelegate<Func<int, Tile, int>>((sizeY, tile) => TileSets.NeedsAdvancedWindSway[tile.TileType] ? TileObjectData.GetTileData(tile).Height : sizeY);
        c.Emit(OpCodes.Stloc_S, sizeYVarIndex);
    }
}