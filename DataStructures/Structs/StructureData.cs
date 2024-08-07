﻿using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.DataStructures.Structs;

/// <summary>
///     Struct that holds the data for a specified structure, used for loading and saving structures
///     from files to generate in the world.
/// </summary>
public readonly struct StructureData (int structureWidth, int structureHeight, List<List<TileData>> structureTileData, Point16 structureDisplacement)
    : TagSerializable {
    public static readonly Func<TagCompound, StructureData> DESERIALIZER = Deserialize;

    public readonly int structureWidth = structureWidth;

    public readonly int structureHeight = structureHeight;

    public readonly List<List<TileData>> structureTileData = structureTileData;

    public readonly Point16 structureDisplacement = structureDisplacement;

    public StructureData(int structureWidth, int structureHeight, List<List<TileData>> structureTileData) : this(structureWidth, structureHeight, structureTileData, Point16.Zero) { }

    public static StructureData Deserialize(TagCompound tag) => new(
        tag.GetInt(nameof(structureWidth)),
        tag.GetInt(nameof(structureHeight)),
        tag.Get<List<List<TileData>>>(nameof(structureTileData)),
        tag.Get<Point16>(nameof(structureDisplacement))
    );

    public TagCompound SerializeData() => new() {
        { nameof(structureWidth), structureWidth },
        { nameof(structureHeight), structureHeight },
        { nameof(structureTileData), structureTileData },
        { nameof(structureDisplacement), structureDisplacement }
    };
}