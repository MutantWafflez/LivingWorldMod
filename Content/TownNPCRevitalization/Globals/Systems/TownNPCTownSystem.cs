using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Globals.BaseTypes.Systems;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     System that handles the conglomeration of Town NPCs in the same area, internally referred to as a "Town". Manages the shared usage of a pathfinder grid for performance, "Points of Interest" for
///     selecting places for Town NPCs to pathfind to, and more to-be implemented.
/// </summary>
public class TownNPCTownSystem : BaseModSystem<TownNPCTownSystem> {
    private enum PointOfInterestType : byte {
        None
    }

    private readonly record struct PointOfInterest(PointOfInterestType Type, Point Position);

    private readonly record struct TownData(List<Vector2> Rooms, Point Centroid, List<PointOfInterest> PointsOfInterest);

    private const int MaximumTileRangeForRoomLinking = 75;

    private List<TownData> _towns;

    public override void PostWorldLoad() {
        GenerateTownGraphs();
    }

    /// <summary>
    ///     Generate all internal graphs for each stored home location of the NPCs in the world. This is the most expensive mapping, as it clears all current data and re-instatiates all of it.
    /// </summary>
    private void GenerateTownGraphs() {
        _towns = [];

        // Using Vector2 for its better hash function in comparison to Point
        List<Vector2> allRoomPositions = WorldGen.TownManager._roomLocationPairs.Select(pair => pair.Item2.ToVector2()).ToList();
        Dictionary<Vector2, HashSet<Vector2>> allLinkedRooms = [];

        // First pass; link all rooms that are directly connected via the maximum distance
        for (int i = 0; i < allRoomPositions.Count; i++) {
            Vector2 posOne = allRoomPositions[i];
            allLinkedRooms[posOne] = [];

            for (int j = 0; j < allRoomPositions.Count; j++) {
                if (i == j) {
                    continue;
                }

                Vector2 posTwo = allRoomPositions[j];
                if (posOne.DistanceSQ(posTwo) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                allLinkedRooms[posOne].Add(posTwo);
            }
        }
        
        // Second pass; link all rooms that are "chained" together, i.e. transitive (if A -> B and B -> C, then A -> C)
        foreach (Vector2 pos in allRoomPositions) {
            if (!allLinkedRooms.TryGetValue(pos, out HashSet<Vector2> initialLinkedRooms)) {
                continue;
            }

            HashSet<Vector2> linkedRoomsUnion = [];
            linkedRoomsUnion.UnionWith(initialLinkedRooms);
            foreach (Vector2 linkedRoom in initialLinkedRooms) {
                linkedRoomsUnion.UnionWith(allLinkedRooms[linkedRoom]);
                allLinkedRooms.Remove(linkedRoom);
            }

            List<Vector2> roomsList = linkedRoomsUnion.ToList();
            Vector2 centroid = roomsList.Aggregate(Vector2.Zero, (current, roomPos) => current + roomPos) / roomsList.Count;
            
            _towns.Add(new TownData(linkedRoomsUnion.ToList(), centroid.ToPoint(), []));
        }
    }
}