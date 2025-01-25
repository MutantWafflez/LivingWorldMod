using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.DataStructures.Structs;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Utilities;

/// Utilities class that has helper methods for specifically NPC housing.
public static partial class LWMUtils {
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
        return housePositions.All(
            housePosition =>
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
}