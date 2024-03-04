using System;
using System.Collections.Generic;
using LivingWorldMod.DataStructures.Structs;
using LivingWorldMod.Utilities;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.DataStructures.Classes.DebugModules;

/// <summary>
/// Saves the selected region to a structure file.
/// </summary>
public class StructureModule : RegionModule {
    protected override void ApplyEffectOnRegion() {
        List<List<TileData>> tileData = new();

        for (int x = 0; x <= bottomRight.X - topLeft.X; x++) {
            tileData.Add(new List<TileData>());
            for (int y = 0; y <= bottomRight.Y - topLeft.Y; y++) {
                Tile requestedTile = Framing.GetTileSafely(x + topLeft.X, y + topLeft.Y);
                tileData[x].Add(new TileData(requestedTile));
            }
        }

        StructureData structData = new(tileData.Count, tileData[0].Count, tileData);

        string outputPath = LWMUtils.GetLWMFilePath() + $"/StructureOutput_{DateTime.Now.ToShortTimeString().Replace(':', '_').Replace(' ', '_')}.struct";

        TagIO.ToFile(new TagCompound { { "structureData", structData } }, outputPath);

        Main.NewText("Structure Copied to File!");
    }
}