using System;
using LivingWorldMod.DataStatuctures.Classes;
using LivingWorldMod.Globals.Sets;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent.Drawing;
using Terraria.ObjectData;

namespace LivingWorldMod.Globals.Patches;

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

        int sizeXVarIndex = -1;
        c.GotoNext(i => i.MatchLdcI4(1));
        c.GotoNext(i => i.MatchStloc(out sizeXVarIndex));

        int sizeYVarIndex = -1;
        c.GotoNext(i => i.MatchLdcI4(1));
        c.GotoNext(i => i.MatchStloc(out sizeYVarIndex));

        int tileVarIndex = -1;
        c.GotoNext(i => i.MatchCall<Tilemap>("get_Item"));
        c.GotoNext(i => i.MatchStloc(out tileVarIndex));

        c.GotoNext(i => i.MatchCall<TileDrawing>("DrawMultiTileVinesInWind"));
        c.GotoPrev(MoveType.After, i => i.MatchLdarg0());
        // We do this so we don't have to mess with the branches
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Ldarg_0);
        c.Index--;
        //Size X
        c.Emit(OpCodes.Ldloc_S, (byte)sizeXVarIndex);
        c.Emit(OpCodes.Ldloc_S, (byte)tileVarIndex);
        c.EmitDelegate<Func<int, Tile, int>>((sizeX, tile) => TileSets.NeedsAdvancedWindSway[tile.TileType] ? TileObjectData.GetTileData(tile).Width : sizeX);
        c.Emit(OpCodes.Stloc_S, (byte)sizeXVarIndex);
        //Size Y
        c.Emit(OpCodes.Ldloc_S, (byte)sizeYVarIndex);
        c.Emit(OpCodes.Ldloc_S, (byte)tileVarIndex);
        c.EmitDelegate<Func<int, Tile, int>>((sizeY, tile) => TileSets.NeedsAdvancedWindSway[tile.TileType] ? TileObjectData.GetTileData(tile).Height : sizeY);
        c.Emit(OpCodes.Stloc_S, (byte)sizeYVarIndex);
    }
}