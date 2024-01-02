using System;
using System.Collections.Generic;
using LivingWorldMod.Custom.Structs;
using LivingWorldMod.Custom.Utilities;
using Terraria;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Classes.DebugModules;

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

        string outputPath = IOUtils.GetLWMFilePath() + $"/StructureOutput_{DateTime.Now.ToShortTimeString().Replace(':', '_').Replace(' ', '_')}.struct";

        TagIO.ToFile(new TagCompound { { "structureData", structData } }, outputPath);

        Main.NewText("Structure Copied to File!");
    }
}