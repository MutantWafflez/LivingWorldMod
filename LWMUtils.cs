using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Hjson;
using LivingWorldMod.Content.Villages.DataStructures.Enums;
using LivingWorldMod.Content.Villages.DataStructures.Structs;
using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.Villages.Globals.Systems;
using LivingWorldMod.Content.Villages.HarpyVillage.Materials;
using LivingWorldMod.Content.Villages.HarpyVillage.NPCs;
using LivingWorldMod.DataStructures.Enums;
using LivingWorldMod.DataStructures.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace LivingWorldMod;

// Master Utilities class for LWM.
public static partial class LWMUtils {
    public enum CornerType : byte {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    ///     Ticks in a real world second.
    /// </summary>
    public const int RealLifeSecond = 60;

    /// <summary>
    ///     Ticks in a real world minute.
    /// </summary>
    public const int RealLifeMinute = RealLifeSecond * 60;

    /// <summary>
    ///     Ticks in a real world hour.
    /// </summary>
    public const int RealLifeHour = RealLifeMinute * 60;

    /// <summary>
    ///     Ticks in a real world day.
    /// </summary>
    public const int RealLifeDay = RealLifeHour * 24;

    /// <summary>
    ///     Ticks in the duration of an in-game day when the sun is up. Equivalent to 15 in-game hours.
    /// </summary>
    public const int InGameDaylight = (int)Main.dayLength;

    /// <summary>
    ///     Ticks in the duration of an in-game day when the moon is out. Equivalent to 9 in-game hours.
    /// </summary>
    public const int InGameMoonlight = (int)Main.nightLength;

    /// <summary>
    ///     Ticks in the full duration of an in-game day.
    /// </summary>
    public const int InGameFullDay = InGameDaylight + InGameMoonlight;

    /// <summary>
    ///     Ticks in an in-game minute. Equivalent to a real life second.
    /// </summary>
    public const int InGameMinute = RealLifeSecond;

    /// <summary>
    ///     Ticks in an in-game hour. Equivalent to a real life minute.
    /// </summary>
    public const int InGameHour = RealLifeMinute;

    /// <summary>
    ///     The side length of a tile, in pixels.
    /// </summary>
    public const int TilePixelsSideLength = 16;

    /// <summary>
    ///     The color of the chat text from Vanilla for various "errors" or "issues", namely for Pylon teleportation and housing requirement problems.
    /// </summary>
    public static readonly Color YellowErrorTextColor = new (255, 240, 20);

    /// <summary>
    ///     The color of the chat text from Vanilla when a naturally occurring parties happens.
    /// </summary>
    public static readonly Color DarkPinkPartyTextColor = new(255, 0, 160);

    /// <summary>
    ///     The color of the chat text from Vanilla when a Town NPC dies or "leaves."
    /// </summary>
    public static readonly Color RedTownNPCDeathTextColor = new(255, 25, 25);

    /// <summary>
    ///     The panel background color used with various UIs in LWM.
    /// </summary>
    public static readonly Color LWMCustomUIPanelBackgroundColor = new (59, 97, 203);

    /// <summary>
    ///     The background color of the sub-panels within pre-existing panels used with various UIs in LWM.
    /// </summary>
    public static readonly Color LWMCustomUISubPanelBackgroundColor = new (46, 46, 159);

    /// <summary>
    ///     The border color of the sub-panels within pre-existing panels used with various UIs in LWM.
    /// </summary>
    public static readonly Color LWMCustomUISubPanelBorderColor = new (22, 29, 107);

    /// <summary>
    ///     The default background color that vanilla uses for its UI Panels.
    /// </summary>
    public static readonly Color UIPanelBackgroundColor = new Color(63, 82, 151) * 0.7f;

    /// <summary>
    ///     Returns world size (on the X coordinate) of the current world being played.
    /// </summary>
    public static WorldSize CurrentWorldSizeX {
        get {
            // I'm in a bit shifting mood
            const int smallMediumDiffHalf = (WorldGen.WorldSizeMediumX - WorldGen.WorldSizeSmallX) >> 1;
            const int mediumLargeDiffHalf = (WorldGen.WorldSizeLargeX - WorldGen.WorldSizeMediumX) >> 1;

            const int halfwayToMediumSizeTilesX = WorldGen.WorldSizeSmallX + smallMediumDiffHalf;
            const int halfwayToLargeSizeTilesX = WorldGen.WorldSizeMediumX + mediumLargeDiffHalf;
            return Main.maxTilesX switch {
                < WorldGen.WorldSizeSmallX => WorldSize.SmallerThanSmall,
                <= halfwayToMediumSizeTilesX => WorldSize.Small,
                <= halfwayToLargeSizeTilesX => WorldSize.Medium,
                <= WorldGen.WorldSizeLargeX => WorldSize.Large,
                _ => WorldSize.LargerThanLarge
            };
        }
    }

    /// <summary>
    ///     Returns world size (on the Y coordinate) of the current world being played.
    /// </summary>
    public static WorldSize CurrentWorldSizeY {
        get {
            // I'm in a bit shifting mood
            const int smallMediumDiffHalf = (WorldGen.WorldSizeMediumY - WorldGen.WorldSizeSmallY) >> 1;
            const int mediumLargeDiffHalf = (WorldGen.WorldSizeLargeY - WorldGen.WorldSizeMediumY) >> 1;

            const int halfwayToMediumSizeTilesY = WorldGen.WorldSizeSmallY + smallMediumDiffHalf;
            const int halfwayToLargeSizeTilesY = WorldGen.WorldSizeMediumY + mediumLargeDiffHalf;
            return Main.maxTilesY switch {
                < WorldGen.WorldSizeSmallY => WorldSize.SmallerThanSmall,
                <= halfwayToMediumSizeTilesY => WorldSize.Small,
                <= halfwayToLargeSizeTilesY => WorldSize.Medium,
                <= WorldGen.WorldSizeLargeY => WorldSize.Large,
                _ => WorldSize.LargerThanLarge
            };
        }
    }

    /// <summary>
    ///     Calculates and returns the current time in game.
    /// </summary>
    public static TimeOnly CurrentInGameTime {
        get {
            // Adapted vanilla code
            double currentTime = Main.time;
            if (!Main.dayTime) {
                currentTime += InGameDaylight;
            }

            double preciseHour = currentTime / InGameFullDay * 24f - 19.5f;
            // "Day time" starts at 04:30 and must be shifted accordingly
            if (preciseHour < 0) {
                preciseHour += 24f;
            }

            int hour = (int)preciseHour;
            return new TimeOnly(hour, (int)((preciseHour - hour) * 60), (int)(Main.time % InGameMinute));
        }
    }

    /// <summary>
    ///     Whether or not, during the ScoreRoom process, to ignore if there is currently an NPC within the specified house.
    ///     Be careful with this, making sure to set it to FALSE once you're done with the score room process, or an infinite
    ///     amount of NPCs can theoretically move into the house!
    /// </summary>
    public static bool IgnoreHouseOccupancy {
        get;
        private set;
    }

    /// <summary>
    ///     The current localization error key related to villager housing, if there is one. Used to provide context to the player that their house is either in a village (when trying to house
    ///     a non-villager NPC) or NOT in a village (when trying to house a villager NPC). Is null when no such error has occurred.
    /// </summary>
    public static string VillagerHousingErrorKey {
        get;
        set;
    }

    private static Exception ILInstructionNotFoundException => new ("Could not find specified IL instruction.");

    /// <summary>
    ///     Extension for strings that will turn the provided string into its <see cref="LocalizedText" /> equivalent, assuming
    ///     the provided string is the key for a <see cref="LWM" /> localization file text value.
    /// </summary>
    public static LocalizedText Localized(this string key) => LWM.Instance.GetLocalization(key);

    /// <summary>
    ///     Converts this <see cref="LocalizedText" /> instance into its <see cref="NetworkText" /> equivalent.
    /// </summary>
    public static NetworkText ToNetworkText(this LocalizedText text) => NetworkText.FromKey(text.Key);

    /// <summary>
    ///     Extension for strings that will prepend "Mods.LivingWorldMod." to the string, for use with generating localization keys
    ///     instead of <see cref="LocalizedText" /> objects directly.
    /// </summary>
    public static string PrependModKey(this string suffix) => $"Mods.{nameof(LivingWorldMod)}.{suffix}";

    /// <summary>
    ///     Takes the input string and places a space in-between each english capital letter, returning the result.
    /// </summary>
    public static string SplitBetweenCapitalLetters(string input) => SpaceBetweenCapitalsRegex().Replace(input, " $1");

    /// <summary>
    ///     Returns whether this localized text actually has a proper localization entry.
    /// </summary>
    public static bool HasValidLocalizationValue(this LocalizedText text) => text.Key != text.Value;

    /// <summary>
    ///     Returns the price multiplier that will affect shop prices depending on the status of a
    ///     villager's relationship with the players.
    /// </summary>
    public static float GetPriceMultiplierFromRep(Villager villager) {
        if (villager.RelationshipStatus == VillagerRelationship.Neutral) {
            return 1f;
        }

        ReputationThresholdData thresholds = ReputationSystem.Instance.villageThresholdData[villager.VillagerType];

        float reputationValue = ReputationSystem.Instance.GetNumericVillageReputation(villager.VillagerType);
        float centerPoint = (thresholds.likeThreshold - thresholds.dislikeThreshold) / 2f;

        return MathHelper.Clamp(1 - reputationValue / (ReputationSystem.VillageReputationConstraint - centerPoint) / 2f, 0.67f, 1.67f);
    }

    /// <summary>
    ///     Returns the count of all defined villager types as defined by the <seealso cref="VillagerType" /> enum.
    /// </summary>
    /// <returns></returns>
    public static int GetTotalVillagerTypeCount() => Enum.GetValues<VillagerType>().Length;

    /// <summary>
    ///     Gets &amp; returns the respective NPC for the given villager type passed in.
    /// </summary>
    /// <param name="type"> The type of villager you want to NPC equivalent of. </param>
    public static int VillagerTypeToNPCType(VillagerType type) {
        return type switch {
            VillagerType.Harpy => ModContent.NPCType<HarpyVillager>(),
            _ => -1
        };
    }

    /// <summary>
    ///     Gets &amp; returns the respective Respawn Item for the given villager type passed in.
    /// </summary>
    /// <param name="type"> The type of villager you want to Respawn Item equivalent of. </param>
    public static int VillagerTypeToRespawnItemType(VillagerType type) {
        return type switch {
            VillagerType.Harpy => ModContent.ItemType<HarpyEgg>(),
            _ => -1
        };
    }

    /// <summary>
    ///     Returns whether or not the specified entity is under a roof or not.
    /// </summary>
    /// <param name="entity"> The entity in question. </param>
    /// <param name="maxRoofHeight"> The maximum height from the top of the entity that can be considered to be a "roof".  </param>
    /// <returns></returns>
    public static bool IsEntityUnderRoof(Entity entity, int maxRoofHeight = 32) {
        for (int i = 0; i < maxRoofHeight; i++) {
            if (!WorldGen.InWorld((int)((entity.Center.X + entity.direction) / 16), (int)(entity.Center.Y / 16) - i)) {
                return false;
            }

            Tile tile = Framing.GetTileSafely(entity.Center.ToTileCoordinates16() + new Point16(0, -i));
            if (tile.HasTile && Main.tileSolid[tile.TileType]) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Returns a precise version of the rectangle bounding box of this npc.
    /// </summary>
    public static PreciseRectangle GetPreciseRectangle(this NPC npc) => new(npc.position, npc.Size);

    /// <summary>
    ///     Returns the first NPC that meets the passed in function requirement.
    /// </summary>
    /// <remarks>
    ///     Note that <see cref="NPC.active" /> is checked by default, along-side the predicate.
    /// </remarks>
    public static NPC GetFirstNPC(Predicate<NPC> npcPredicate) {
        foreach (NPC npc in Main.ActiveNPCs) {
            if (npcPredicate.Invoke(npc)) {
                return npc;
            }
        }

        return null;
    }

    /// <summary>
    ///     Returns all NPCs that meet the passed in function requirement.
    /// </summary>
    /// <remarks>
    ///     Note that <see cref="NPC.active" /> is checked by default, along-side the predicate.
    /// </remarks>
    public static List<NPC> GetAllNPCs(Predicate<NPC> npcPredicate) {
        List<NPC> npcList = [];
        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc.active && npcPredicate.Invoke(npc)) {
                npcList.Add(npc);
            }
        }

        return npcList;
    }

    /// <summary>
    ///     Gets and returns either the type name (if a modded NPC) or <see cref="NPCID" /> name (if a vanilla NPC), based on the passed in npc type.
    /// </summary>
    public static string GetNPCTypeNameOrIDName(int npcType) => npcType >= NPCID.Count ? NPCLoader.GetNPC(npcType).Name : NPCID.Search.GetName(npcType);

    /// <summary>
    ///     Whether this entity is facing left, i.e. <see cref="Entity.direction" /> is -1.
    /// </summary>
    public static bool IsFacingLeft(this Entity entity) => entity.direction == -1;

    /// <summary>
    ///     Generates a given Structure into the world using a StructureData struct.
    /// </summary>
    /// <param name="data"> The struct containing data for the structure. </param>
    /// <param name="startingX"> Far left location of where the structure will begin to generate. </param>
    /// <param name="startingY"> Top-most location of where the structure will begin to generate. </param>
    /// <param name="autoFrame">
    ///     Whether or not the entire structure should be framed, in terms of both walls and tiles,
    ///     when finished being generated.
    /// </param>
    public static void GenerateStructure(StructureData data, int startingX, int startingY, bool autoFrame = true) {
        for (int x = 0; x < data.structureWidth; x++) {
            for (int y = 0; y < data.structureHeight; y++) {
                Tile selectedTile = Framing.GetTileSafely(startingX + x + data.structureDisplacement.X, startingY + y + data.structureDisplacement.Y);
                TileData tileData = data.structureTileData[x][y];

                switch (tileData.type) {
                    case > 0: {
                        if (ModContent.TryFind(tileData.modTileOwner, tileData.modTileName, out ModTile modTile)) {
                            selectedTile.TileType = modTile.Type;
                        }
                        else {
                            selectedTile.TileType = (ushort)tileData.type;
                        }

                        selectedTile.HasTile = true;

                        selectedTile.IsHalfBlock = tileData.isHalfBlock;
                        selectedTile.TileFrameNumber = (byte)tileData.frameNumber;
                        selectedTile.TileFrameX = (short)tileData.frameX;
                        selectedTile.TileFrameY = (short)tileData.frameY;
                        selectedTile.Slope = (SlopeType)tileData.slopeType;
                        selectedTile.TileColor = (byte)tileData.color;
                        selectedTile.IsActuated = tileData.isActuated;
                        selectedTile.HasActuator = tileData.hasActuator;
                        selectedTile.RedWire = tileData.hasRedWire;
                        selectedTile.BlueWire = tileData.hasBlueWire;
                        selectedTile.GreenWire = tileData.hasGreenWire;
                        selectedTile.YellowWire = tileData.hasYellowWire;
                        selectedTile.LiquidType = tileData.liquidType;
                        selectedTile.LiquidAmount = (byte)tileData.liquidAmount;
                        break;
                    }
                    case -1:
                        selectedTile.TileType = TileID.Dirt;
                        selectedTile.HasTile = false;
                        selectedTile.HasActuator = tileData.hasActuator;
                        selectedTile.RedWire = tileData.hasRedWire;
                        selectedTile.BlueWire = tileData.hasBlueWire;
                        selectedTile.GreenWire = tileData.hasGreenWire;
                        selectedTile.YellowWire = tileData.hasYellowWire;
                        selectedTile.LiquidType = tileData.liquidType;
                        selectedTile.LiquidAmount = (byte)tileData.liquidAmount;
                        break;
                }

                if (tileData.wallType == -1) {
                    continue;
                }

                if (ModContent.TryFind(tileData.modWallOwner, tileData.modWallName, out ModWall modWall)) {
                    selectedTile.WallType = modWall.Type;
                }
                else {
                    selectedTile.WallType = (ushort)tileData.wallType;
                }

                selectedTile.WallColor = (byte)tileData.wallColor;
                selectedTile.WallFrameNumber = (byte)tileData.wallFrame;
                selectedTile.WallFrameX = tileData.wallFrameX;
                selectedTile.WallFrameY = tileData.wallFrameY;
            }
        }

        if (!autoFrame) {
            return;
        }

        for (int y = 0; y < data.structureHeight; y++) {
            for (int x = 0; x < data.structureWidth; x++) {
                WorldGen.TileFrame(startingX + x + data.structureDisplacement.X, startingY + y + data.structureDisplacement.Y, true, true);
                Framing.WallFrame(startingX + x + data.structureDisplacement.X, startingY + y + data.structureDisplacement.Y, true);
            }
        }
    }

    /// <summary>
    ///     Starting at the specified position, attempts to purge all walls and tiles connected to said tile in any way. This
    ///     includes diagonal tiles.
    /// </summary>
    /// <param name="x"> The beginning tile X position to begin purging. </param>
    /// <param name="y"> The beginning tile Y position to begin purging. </param>
    /// <param name="ignoredTileTypes">
    ///     An array of tile types to ignore for purging. Defaults to null, or no ignored tile
    ///     types.
    /// </param>
    /// <param name="maxRepetitions"> How many times the purge loops is allowed run, at maximum. Defaults to 500. </param>
    public static void PurgeStructure(int x, int y, int[] ignoredTileTypes = null, uint maxRepetitions = 500) {
        Queue<Point> tiles = new();
        Tile firstTile = Main.tile[x, y];
        if ((firstTile.HasTile && !(ignoredTileTypes?.Contains(firstTile.TileType) ?? false)) || firstTile.WallType > WallID.None) {
            tiles.Enqueue(new Point(x, y));
        }

        uint repetitions = 0;
        while (tiles.Count != 0 && repetitions < maxRepetitions) {
            repetitions++;

            Point tilePos = tiles.Dequeue();

            SearchAroundTile(tilePos.X, tilePos.Y);

            Main.tile[tilePos].ClearTile();
            Main.tile[tilePos].WallType = WallID.None;
        }

        return;

        void SearchAroundTile(int i, int j) {
            for (int k = -1; k <= 1; k++) {
                for (int l = -1; l <= 1; l += k == 0 ? 2 : 1) {
                    Tile tile = Main.tile[i + k, j + l];

                    if (((tile.HasTile && !(ignoredTileTypes?.Contains(tile.TileType) ?? false)) || tile.WallType > WallID.None) && !tiles.Any(point => point.X == i + k && point.Y == j + l)) {
                        tiles.Enqueue(new Point(i + k, j + l));
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Checks and returns whether or not the given tile type at the given position can merge
    ///     with the other tile at the position offset by the given offset. For example, passing the
    ///     dirt tile type and its position and an offset of 0, 1 would check if dirt can merge with
    ///     the tile right below it.
    /// </summary>
    /// <param name="tileType"> The tile type at the given position. </param>
    /// <param name="tilePosition"> The position of the tile in question. </param>
    /// <param name="otherTileOffset">
    ///     The offset of the other tile in regards to the initial tile's position.
    /// </param>
    /// <returns> </returns>
    public static bool CanMergeWithTile(int tileType, Point tilePosition, Point otherTileOffset) {
        Tile otherTile = Framing.GetTileSafely(tilePosition + otherTileOffset);

        if (otherTile is not { HasTile: true }) {
            return false;
        }

        return otherTile.TileType == tileType
            || Main.tileMerge[tileType][otherTile.TileType]
            || (otherTile.TileType == TileID.Dirt && (Main.tileMergeDirt[tileType] || TileID.Sets.ForcedDirtMerging[tileType]))
            || (TileID.Sets.MergesWithClouds[tileType] && TileID.Sets.Clouds[otherTile.TileType])
            || (TileID.Sets.OreMergesWithMud[tileType] && TileID.Sets.Mud[otherTile.TileType]);
    }

    public static Point GetCornerOfMultiTile(Tile tile, int x, int y, CornerType corner) {
        TileObjectData data = TileObjectData.GetTileData(tile);
        Point topLeft = new(x - tile.TileFrameX % data.CoordinateFullWidth / 18, y - tile.TileFrameY % data.CoordinateFullHeight / 18);

        return corner switch {
            CornerType.TopLeft => topLeft,
            CornerType.TopRight => topLeft + new Point(data.Width - 1, 0),
            CornerType.BottomLeft => topLeft + new Point(0, data.Height - 1),
            CornerType.BottomRight => topLeft + new Point(data.Width - 1, data.Height - 1),
            _ => topLeft
        };
    }

    /// <summary>
    ///     Calculates the hitbox AKA bounding box of the specified tile at the provided position.
    /// </summary>
    public static Rectangle GetTileHitBox(Tile tile, Point16 tilePos) =>
        TileObjectData.GetTileData(tile) is not { } tileData ? Rectangle.Empty : new Rectangle(tilePos.X, tilePos.Y, tileData.Width, tileData.Height);

    /// <summary>
    ///     Method that starts at a specified initial tile position, and moves down until the passed in conditional is
    ///     satisfied. Returns the first point that meets the conditions, or if one isn't found (or if the maximum drop is
    ///     reached), returns null.
    /// </summary>
    /// <param name="condition"> The condition function that will determine if a tile position is valid. </param>
    /// <param name="initialPoint"> The initial tile point to start searching from. </param>
    /// <param name="maximumDrop">
    ///     The maximum distance from the initial Y point. Exceeding will forcefully trigger
    ///     failure.
    /// </param>
    /// <returns></returns>
    public static Point? DropUntilCondition(Func<Point, bool> condition, Point initialPoint, int maximumDrop) {
        Point point = new(initialPoint.X, initialPoint.Y);

        for (int i = 0; i <= maximumDrop; i++) {
            if (condition(point)) {
                return point;
            }

            point.Y++;
        }

        return null;
    }

    /// <summary>
    ///     A more "auto-magic" version of <see cref="AddMapEntry" />, where assumptions are automatically made about what kind of map entry is intended to be added, and where.
    ///     <para></para>
    ///     If the <see cref="instance" /> parameter is a <see cref="ModTile" /> instance, it will be assumed that <see cref="MapLoader.tileEntries" /> is going to be added to. The same applies for
    ///     <see cref="ModWall" /> and <see cref="MapLoader.wallEntries" />. This method will fail and do nothing if the <see cref="instance" /> parameter is neither tile nor wall.
    ///     <para></para>
    ///     The "hover text" functionality on the map will only be added if an existing localization key exists, following the format that <see cref="ILocalizedModTypeExtensions.GetLocalizationKey" /> uses,
    ///     with the suffix of "MapEntry". For example: Mods.LivingWorldMod.Tiles.ExampleTile.MapEntry. If the given key does not exist, no hover text will be added.
    ///     <para></para>
    ///     If the provided <see cref="color" /> parameter is null, this function will fail.
    /// </summary>
    /// <returns>Whether a map entry was successfully added.</returns>
    public static bool TryAddMapEntry(ILocalizedModType instance, ushort type, Color? color) {
        if (MapLoader.initialized || color is not { } mapColor) {
            return false;
        }

        IDictionary<ushort, IList<MapEntry>> entryDict;
        switch (instance) {
            case ModTile:
                entryDict = MapLoader.tileEntries;
                break;
            case ModWall:
                entryDict = MapLoader.wallEntries;
                break;
            default:
                return false;
        }

        string mapEntryKey = instance.GetLocalizationKey("MapEntry");
        //AKA check if the localization for this tile exists, and only add it if it does
        //Translations will return the key if you try to get the translation value for a translation that doesn't exist.
        AddMapEntry(entryDict, type,  mapColor, mapEntryKey == Language.GetTextValue(mapEntryKey) ? null : Language.GetText(mapEntryKey));
        return true;
    }

    /// <summary>
    ///     Method that allow for adding a map entry to a dictionary of <see cref="MapEntry" /> lists. Applicable for <see cref="MapLoader.tileEntries" /> and <see cref="MapLoader.wallEntries" /> for
    ///     adding map entries for tiles and walls, respectively.
    /// </summary>
    /// <remarks>
    ///     I created this because <see cref="ModTile.AddMapEntry" /> and <see cref="ModWall.AddMapEntry" /> could easily be coalesced into the same method, where the passed in dictionary is the only thing
    ///     that changes between them. Also exists because of all the red tape in terms of access levels (everything is either internal or private).
    /// </remarks>
    public static void AddMapEntry(IDictionary<ushort, IList<MapEntry>> entryDict, ushort type, Color color, LocalizedText hoverText = null) {
        MapEntry mapEntry = new(color, hoverText);
        if (!entryDict.TryGetValue(type, out IList<MapEntry> list)) {
            list = new List<MapEntry>();
            entryDict[type] = list;
        }

        list.Add(mapEntry);
    }

    /// <summary>
    ///     Constructs a new <see cref="Rectangle" /> from two points representing opposite corners.
    ///     These corners can be any of the corners - but make sure that they are OPPOSITE
    ///     corners, such as top-left/bottom-right or bottom-left/top-right.
    /// </summary>
    public static Rectangle NewRectFromCorners(Point cornerOne, Point cornerTwo) {
        int minX = Math.Min(cornerOne.X, cornerTwo.X);
        int maxX = Math.Max(cornerOne.X, cornerTwo.X);
        int minY = Math.Min(cornerOne.Y, cornerTwo.Y);
        int maxY = Math.Max(cornerOne.Y, cornerTwo.Y);

        return new Rectangle(
            minX,
            minY,
            maxX - minX,
            maxY - minY
        );
    }

    /// <summary>
    ///     Returns a copy of this rectangle (which should be in Tile coordinates) but in World coordinates.
    /// </summary>
    public static Rectangle ToWorldCoordinates(this Rectangle rectangle) => new (
        rectangle.X * 16,
        rectangle.Y * 16,
        rectangle.Width * 16,
        rectangle.Height * 16
    );

    /// <summary>
    ///     Calls <seealso cref="ILCursor.TryGotoNext" /> normally, but will throw an exception if
    ///     the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoNext(this ILCursor cursor, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoNext(predicates)) {
            return;
        }

        throw new ILPatchFailureException(LWM.Instance, cursor.Context, ILInstructionNotFoundException);
    }

    /// <summary>
    ///     Calls <seealso cref="ILCursor.TryGotoNext" /> normally, but will throw an exception if
    ///     the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoNext(this ILCursor cursor, MoveType moveType, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoNext(moveType, predicates)) {
            return;
        }

        throw new ILPatchFailureException(LWM.Instance, cursor.Context, ILInstructionNotFoundException);
    }

    /// <summary>
    ///     Calls <seealso cref="ILCursor.TryGotoPrev" /> normally, but will throw an exception if
    ///     the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoPrev(this ILCursor cursor, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoPrev(predicates)) {
            return;
        }

        throw new ILPatchFailureException(LWM.Instance, cursor.Context, ILInstructionNotFoundException);
    }

    /// <summary>
    ///     Calls <seealso cref="ILCursor.TryGotoPrev" /> normally, but will throw an exception if
    ///     the goto does not work or does not find the proper instruction.
    /// </summary>
    public static void ErrorOnFailedGotoPrev(this ILCursor cursor, MoveType moveType, params Func<Instruction, bool>[] predicates) {
        if (cursor.TryGotoPrev(moveType, predicates)) {
            return;
        }

        throw new ILPatchFailureException(LWM.Instance, cursor.Context, ILInstructionNotFoundException);
    }

    /// <summary>
    ///     Clones the given method body to the cursor, wholesale copying all
    ///     relevant contents (instructions, parameters, variables, etc.).
    /// </summary>
    /// <param name="body">The body to copy from.</param>
    /// <param name="c">The cursor to copy to.</param>
    /// <remarks>
    ///     This method is provided courtesy of the lovely developers behind Nitrate:
    ///     https://github.com/terraria-catalyst/nitrate-mod
    ///     <para></para>
    ///     Slightly tweaked for LWM's formatting, but no underlying logic changed.
    /// </remarks>
    public static void CloneMethodBodyToCursor(MethodBody body, ILCursor c) {
        c.Index = 0;

        c.Body.MaxStackSize = body.MaxStackSize;
        c.Body.InitLocals = body.InitLocals;
        c.Body.LocalVarToken = body.LocalVarToken;

        foreach (Instruction instr in body.Instructions) {
            c.Emit(instr.OpCode, instr.Operand);
        }

        for (int i = 0; i < body.Instructions.Count; i++) {
            c.Instrs[i].Offset = body.Instructions[i].Offset;
        }

        foreach (Instruction instr in c.Body.Instructions) {
            instr.Operand = instr.Operand switch {
                Instruction target => c.Body.Instructions[body.Instructions.IndexOf(target)],
                Instruction[] targets => targets.Select(x => c.Body.Instructions[body.Instructions.IndexOf(x)]).ToArray(),
                _ => instr.Operand
            };
        }

        c.Body.ExceptionHandlers.AddRange(
            body.ExceptionHandlers.Select(x => new ExceptionHandler(x.HandlerType) {
                    TryStart = x.TryStart is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.TryStart)],
                    TryEnd = x.TryEnd is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.TryEnd)],
                    FilterStart = x.FilterStart is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.FilterStart)],
                    HandlerStart = x.HandlerStart is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.HandlerStart)],
                    HandlerEnd = x.HandlerEnd is null ? null : c.Body.Instructions[body.Instructions.IndexOf(x.HandlerEnd)],
                    CatchType = x.CatchType is null ? null : c.Body.Method.Module.ImportReference(x.CatchType)
                }
            )
        );

        c.Body.Variables.AddRange(body.Variables.Select(x => new VariableDefinition(x.VariableType)));

        c.Method.CustomDebugInformations.AddRange(
            body.Method.CustomDebugInformations.Select(x => {
                    switch (x) {
                        case AsyncMethodBodyDebugInformation asyncInfo: {
                            AsyncMethodBodyDebugInformation info = new();

                            if (asyncInfo.CatchHandler.Offset >= 0) {
                                info.CatchHandler = asyncInfo.CatchHandler.IsEndOfMethod ? new InstructionOffset() : new InstructionOffset(ResolveInstrOff(info.CatchHandler.Offset));
                            }

                            info.Yields.AddRange(asyncInfo.Yields.Select(y => y.IsEndOfMethod ? new InstructionOffset() : new InstructionOffset(ResolveInstrOff(y.Offset))));
                            info.Resumes.AddRange(asyncInfo.Resumes.Select(y => y.IsEndOfMethod ? new InstructionOffset() : new InstructionOffset(ResolveInstrOff(y.Offset))));

                            return info;
                        }

                        case StateMachineScopeDebugInformation stateInfo: {
                            StateMachineScopeDebugInformation info = new();
                            info.Scopes.AddRange(stateInfo.Scopes.Select(y => new StateMachineScope(ResolveInstrOff(y.Start.Offset), y.End.IsEndOfMethod ? null : ResolveInstrOff(y.End.Offset))));

                            return info;
                        }

                        default:
                            return x;
                    }
                }
            )
        );

        c.Method.DebugInformation.SequencePoints.AddRange(
            body.Method.DebugInformation.SequencePoints.Select(x =>
                new SequencePoint(ResolveInstrOff(x.Offset), x.Document) { StartLine = x.StartLine, StartColumn = x.StartColumn, EndLine = x.EndLine, EndColumn = x.EndColumn }
            )
        );

        c.Index = 0;

        return;

        Instruction ResolveInstrOff(int off) {
            for (int i = 0; i < body.Instructions.Count; i++) {
                if (body.Instructions[i].Offset == off) {
                    return c.Body.Instructions[i];
                }
            }

            throw new Exception("Could not resolve instruction offset");
        }
    }

    /// <summary>
    ///     Tries to find an entity of the specified Type. Returns whether or not it found the
    ///     entity or not.
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="x"> The x coordinate of the potential entity. </param>
    /// <param name="y"> The y coordinate of the potential entity. </param>
    /// <param name="entity"> The potential entity. </param>
    public static bool TryFindModEntity<T>(int x, int y, out T entity) where T : ModTileEntity {
        TileEntity.ByPosition.TryGetValue(new Point16(x, y), out TileEntity retrievedEntity);

        if (retrievedEntity is T castEntity) {
            entity = castEntity;
            return true;
        }

        entity = null;
        return false;
    }

    /// <summary>
    ///     Tries to find an entity of the specified Type. Returns whether or not it found the
    ///     entity or not.
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="ID"> The ID of the potential entity. </param>
    /// <param name="entity"> The potential entity. </param>
    public static bool TryFindModEntity<T>(int ID, out T entity)
        where T : ModTileEntity {
        TileEntity retrievedEntity = TileEntity.ByID[ID];

        if (retrievedEntity is T castEntity) {
            entity = castEntity;
            return true;
        }

        entity = null;
        return false;
    }

    /// <summary>
    ///     Gets and returns all currently living tile entities of the specified type.
    /// </summary>
    /// <typeparam name="T"> The type of ModTileEntity you want to find all living entities of. </typeparam>
    public static IEnumerable<T> GetAllEntityOfType<T>() where T : ModTileEntity => TileEntity.ByID.Values.OfType<T>();

    /// <summary>
    ///     Calculates and returns the entirety of the savings of the player in all applicable inventories.
    /// </summary>
    /// <param name="player"> </param>
    /// <returns> </returns>
    public static long CalculateTotalSavings(this Player player) {
        long playerInvCashCount = Utils.CoinsCount(out bool _, player.inventory);
        long piggyCashCount = Utils.CoinsCount(out bool _, player.bank.item);
        long safeCashCount = Utils.CoinsCount(out bool _, player.bank2.item);
        long defForgeCashCount = Utils.CoinsCount(out bool _, player.bank3.item);
        long voidVaultCashCount = Utils.CoinsCount(out bool _, player.bank4.item);

        return Utils.CoinsCombineStacks(out bool _, playerInvCashCount, piggyCashCount, safeCashCount, defForgeCashCount, voidVaultCashCount);
    }

    public static double NextDouble(this UnifiedRandom self, double maxValue) => self.NextDouble() * maxValue;

    /// <summary>
    ///     Ensures that this asset is loaded by forcefully loading it if it's yet to be loaded, and no-ops if the asset has already been loaded, returning itself.
    ///     Uses LWM's Asset Repo by default (i.e. assetRepo param = <see langword="null" />).
    /// </summary>
    public static Asset<T> EnsureAssetLoaded<T>(this Asset<T> asset, AssetRepository assetRepo = null) where T : class {
        if (asset.IsLoaded) {
            return asset;
        }

        assetRepo ??= LWM.Instance.Assets;

        lock (assetRepo._requestLock) {
            assetRepo.EnsureReloadActionExistsForType<T>();
            assetRepo.LoadAsset(asset, AssetRequestMode.ImmediateLoad);
        }

        asset.Wait();

        return asset;
    }

    /// <summary>
    ///     Converts a <see cref="Point" /> type to its <see cref="Point16" /> equivalent.
    /// </summary>
    public static Point16 ToPoint16(this Point point) => new (point.X, point.Y);

    /// <summary>
    ///     Gets &amp; returns the positions of all houses that are valid housing within the passed in zone with the passed in
    ///     NPC type. Make sure the passed in zone is in tile coordinates.
    /// </summary>
    /// <param name="zone"> The zone to search in. This should be tile coordinates. </param>
    /// <param name="npcType">
    ///     The type of NPC to check the housing with. If you want "Normal" checking, pass in the Guide
    ///     type.
    /// </param>
    public static List<Point16> GetValidHousesInZone(Circle zone, int npcType) {
        List<Point16> foundHouses = [];
        Rectangle rectangle = zone.ToRectangle();

        for (int i = 0; i < rectangle.Width; i += 2) {
            for (int j = 0; j < rectangle.Height; j += 2) {
                Point position = new(rectangle.X + i, rectangle.Y + j);
                if (!zone.ContainsPoint(position.ToVector2())) {
                    continue;
                }

                if (!WorldGen.InWorld(position.X, position.Y) || !WorldGen.StartRoomCheck(position.X, position.Y) || !WorldGen.RoomNeeds(npcType)) {
                    continue;
                }

                ScoreRoomIgnoringOccupancy(npcTypeAskingToScoreRoom: npcType);
                Point16 bestPoint = new(WorldGen.bestX, WorldGen.bestY);
                if (WorldGen.hiScore <= 0
                    || foundHouses.Contains(bestPoint)
                    || !zone.ContainsPoint(new Vector2(WorldGen.bestX, WorldGen.bestY))
                ) {
                    continue;
                }

                foundHouses.Add(bestPoint);
            }
        }

        return foundHouses;
    }

    /// <summary>
    ///     Returns whether or not all of the passed in positions are all within regions
    ///     considered to be valid housing.
    /// </summary>
    /// <param name="housePositions"> Every position to check for valid housing. </param>
    /// <param name="npcType"> The NPC type to be testing against for house validity. </param>
    /// <param name="adjustForBest">
    ///     Whether or not to move the position up 1 tile when checking if the position is valid.
    ///     This is necessary if positions calculated with WorldGen's bestX and bestY values are passed in,
    ///     since the Y value is typically on a floor tile.
    /// </param>
    public static bool LocationsValidForHousing(List<Point16> housePositions, int npcType, bool adjustForBest = true) {
        return housePositions.All(housePosition =>
            WorldGen.InWorld(housePosition.X, housePosition.Y - (adjustForBest ? 1 : 0))
            && WorldGen.StartRoomCheck(housePosition.X, housePosition.Y - (adjustForBest ? 1 : 0))
            && WorldGen.RoomNeeds(npcType)
        );
    }

    /// <summary>
    ///     Gets the count of the specified NPC type that currently has a house within the zone.
    /// </summary>
    /// <param name="zone"> The zone to search in. This should be tile coordinates. </param>
    /// <param name="npcType"> The type of NPC to check the housing with. </param>
    public static int NPCCountHousedInZone(Circle zone, int npcType) =>
        Main.npc.Where(npc => npc.active && !npc.homeless && npc.type == npcType).Sum(npc => zone.ContainsPoint(new Vector2(npc.homeTileX, npc.homeTileY)) ? 1 : 0);

    /// <summary>
    ///     Sets WorldGen.bestX and WorldGen.bestY for the current house being checked. This exists purely in order
    ///     for the values to be set even if the house is occupied!
    /// </summary>
    public static void ScoreRoomIgnoringOccupancy(int ignoreNPC = -1, int npcTypeAskingToScoreRoom = -1) {
        IgnoreHouseOccupancy = true;
        WorldGen.ScoreRoom(ignoreNPC, npcTypeAskingToScoreRoom);
        IgnoreHouseOccupancy = false;
    }

    /// <summary>
    ///     Returns the LWM File path in the ModLoader folder, and creates the directory if it does
    ///     not exist.
    /// </summary>
    /// <returns> </returns>
    public static string GetLWMFilePath() {
        string lwmPath = ModLoader.ModPath.Replace("Mods", "LivingWorldMod");

        if (!Directory.Exists(lwmPath)) {
            Directory.CreateDirectory(lwmPath);
        }

        return lwmPath;
    }

    /// <summary>
    ///     Shorthand method for getting a StructureData instance from a file. The path parameter
    ///     should not include the mod folder. For example, in the path
    ///     "LivingWorldMod/Content/Structures/ExampeStructure.struct" it should not include the
    ///     "LivngWorldMod" part, it just needs the "Content/Structures/ExampleStructure.struct" part.
    /// </summary>
    /// <param name="path"> The path in the LivingWorldMod folder to go to. </param>
    /// <returns> </returns>
    public static StructureData GetStructureFromFile(string path) {
        LWM modInstance = LWM.Instance;

        Stream fileStream = modInstance.GetFileStream(path);

        StructureData structureData = TagIO.FromStream(fileStream).Get<StructureData>("structureData");

        fileStream.Close();

        return structureData;
    }

    /// <summary>
    ///     Draws a given texture with the specified armor shader id.
    /// </summary>
    /// <remarks>
    ///     Note: Make sure the given sprite-batch is already started when this method is called; this method end and restarts it.
    /// </remarks>
    /// <param name="spriteBatch">
    ///     The sprite-batch that will be used to draw with. Make sure it is already before this method
    ///     is called; this method ends and restarts it.
    /// </param>
    /// <param name="texture"> The texture that will be drawn. </param>
    /// <param name="shaderID"> The given id for the armor shader that will be applied to the texture. </param>
    /// <param name="drawColor"> The underlying color that the texture will be drawn with. </param>
    /// <param name="destinationRectangle"> The rectangle where the texture to be drawn to. </param>
    /// <param name="sourceRectangle"> The region of the texture/s sprite that will actually be drawn. </param>
    /// <param name="origin"> The offset from the draw position. </param>
    /// <param name="rotation"> The rotation that the texture will be drawn with. </param>
    /// <param name="drawMatrix">
    ///     The matrix that will be used to draw with; make sure to use the right one for UI and world
    ///     drawing, where applicable.
    /// </param>
    public static void DrawTextureWithArmorShader(
        SpriteBatch spriteBatch,
        Texture2D texture,
        int shaderID,
        Color drawColor,
        Rectangle destinationRectangle,
        Rectangle sourceRectangle,
        Vector2 origin,
        float rotation,
        Matrix drawMatrix
    ) {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, drawMatrix);

        DrawData itemDrawData = new(texture, destinationRectangle, sourceRectangle, drawColor, rotation, origin, SpriteEffects.None);
        GameShaders.Armor.Apply(shaderID, null, itemDrawData);

        spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, drawColor, rotation, origin, SpriteEffects.None, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, drawMatrix);
    }

    /// <summary>
    ///     Creates a circle of dust at the specified origin with the passed in radius, dust, and
    ///     optional angle change. Radius is in terms of pixels.
    /// </summary>
    /// <param name="origin"> The origin of the dust circle in world coordinates. </param>
    /// <param name="radius"> The radius of the dust circle. </param>
    /// <param name="dust"> The dust to duplicate and use. </param>
    /// <param name="angleChange">
    ///     The angle change between each dust particle in the circle. Defaults to 5 degrees.
    /// </param>
    public static void CreateCircle(Vector2 origin, float radius, Dust dust, float angleChange = 5) {
        for (float i = 0; i < 360f; i += angleChange) {
            Vector2 newPos = origin - new Vector2(0, radius).RotatedBy(MathHelper.ToRadians(i));

            Dust newDust = Dust.NewDustPerfect(newPos, dust.type, dust.velocity, dust.alpha, dust.color, dust.scale);

            newDust.fadeIn = dust.fadeIn;
            newDust.noGravity = dust.noGravity;
            newDust.rotation = dust.rotation;
            newDust.noLight = dust.noLight;
            newDust.frame = dust.frame;
            newDust.shader = dust.shader;
            newDust.customData = dust.customData;
        }
    }

    /// <summary>
    ///     Creates a circle of dust at the specified origin with the passed in radius, dust, and
    ///     optional angle change. Radius is in terms of pixels.
    /// </summary>
    /// <param name="origin"> The origin of the dust circle in world coordinates. </param>
    /// <param name="radius"> The radius of the dust circle. </param>
    /// <param name="dustID"> The ID of the dust to compose the circle of. </param>
    /// <param name="angleChange">
    ///     The angle change between each dust particle in the circle. Defaults to 5 degrees.
    /// </param>
    public static void CreateCircle(Vector2 origin, float radius, int dustID, Vector2? velocity = null, int alpha = 0, Color newColor = default, float scale = 1f, float angleChange = 5) {
        for (float i = 0; i < 360f; i += angleChange) {
            Dust.NewDustPerfect(origin - new Vector2(0, radius).RotatedBy(MathHelper.ToRadians(i)), dustID, velocity, alpha, newColor, scale);
        }
    }

    /// <summary>
    ///     Gets and returns the json data from the specified file path. This is for specifically
    ///     LWM, so the file path does not need to include "LivingWorldMod".
    /// </summary>
    public static JsonValue GetJSONFromFile(string filePath) {
        Stream jsonStream = LWM.Instance.GetFileStream(filePath);
        JsonValue jsonData = JsonValue.Load(jsonStream);
        jsonStream.Close();

        return jsonData;
    }

    /// <summary>
    ///     Regex used to
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("([A-Z])")]
    private static partial Regex SpaceBetweenCapitalsRegex();
}