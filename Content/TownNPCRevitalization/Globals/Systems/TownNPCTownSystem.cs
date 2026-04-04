using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Globals.BaseTypes.Systems;
using Microsoft.Xna.Framework;

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
        List<Vector2> allRoomPositions = WorldGen.TownManager._roomLocationPairs.Select(pair => pair.Item2.ToVector2()).Distinct().ToList();
        if (allRoomPositions.Count <= 0) {
            return;
        }

        Dictionary<Vector2, List<Vector2>> allLinkedRooms = [];

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

        // Second pass; link all rooms that are "chained" together via multiple BFS (my goat), i.e. transitive (if A -> B and B -> C, then A -> C)
        HashSet<Vector2> visitedSet = [];
        foreach (Vector2 rootVertex in allRoomPositions) {
            if (visitedSet.Contains(rootVertex)) {
                continue;
            }

            List<Vector2> linkedVertices = [];
            Queue<Vector2> frontier = [];
            frontier.Enqueue(rootVertex);
            while (frontier.Count > 0) {
                Vector2 currentVertex = frontier.Dequeue();
                linkedVertices.Add(currentVertex);
                visitedSet.Add(currentVertex);

                foreach (Vector2 linkedVertex in allLinkedRooms[currentVertex]) {
                    if (!visitedSet.Add(linkedVertex)) {
                        continue;
                    }

                    frontier.Enqueue(linkedVertex);
                }
            }


            Vector2 centroid = linkedVertices.Aggregate(Vector2.Zero, (current, roomPos) => current + roomPos) / linkedVertices.Count;
            _towns.Add(new TownData(linkedVertices, centroid.ToPoint(), []));
        }
    }
}