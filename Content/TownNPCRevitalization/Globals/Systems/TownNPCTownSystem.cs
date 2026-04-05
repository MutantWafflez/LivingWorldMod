using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;
using LivingWorldMod.Globals.BaseTypes.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

    private readonly record struct TownData(Rectangle TownZone, List<Point> RoomPositions, TownNPCPathfinder TownPathfinder);

    private const int MaximumTileRangeForRoomLinking = 75;

    private List<TownData> _towns;

    private static Rectangle CreateTownZoneFromRoomPositions(List<Point> roomPositions) {
        Point topLeftOfTown = new (int.MaxValue, int.MaxValue);
        Point bottomRightOfTown = new (int.MinValue, int.MinValue);
        foreach (Point point in roomPositions) {
            if (point.X < topLeftOfTown.X) {
                topLeftOfTown.X = point.X;
            }

            if (point.Y < topLeftOfTown.Y) {
                topLeftOfTown.Y = point.Y;
            }

            if (point.X > bottomRightOfTown.X) {
                bottomRightOfTown.X = point.X + 1;
            }

            if (point.Y > bottomRightOfTown.Y) {
                bottomRightOfTown.Y = point.Y + 1;
            }
        }

        return LWMUtils.NewRectFromCorners(topLeftOfTown, bottomRightOfTown);
    }

    public override void PostWorldLoad() {
        CalculateTowns();
    }

    public override void PostDrawTiles() {
        if (!LWM.IsDebug) {
            return;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        foreach (TownData town in _towns) {
            Utils.DrawRect(Main.spriteBatch, town.TownZone.ToWorldCoordinates(), new Color((byte)town.TownZone.X, (byte)town.TownZone.Y, (byte)(town.TownZone.X + town.TownZone.Y)));
        }

        Main.spriteBatch.End();
    }

    /// <summary>
    ///     Searches the current list of towns and attempts to add the passed-in room to its proper town. If no town is close enough to be grouped into any town, a new town will be created with the room as
    ///     the only position.
    /// </summary>
    public void AddRoomToTown(Point roomPos) {
        if (_towns.Count <= 0) {
            AddTownFromRooms([roomPos]);
            return;
        }

        float closestTownDistanceSquared = float.MaxValue;
        int closestTownIndex = 0;
        TownData closestTown = _towns[closestTownIndex];
        for (int i = 0; i < _towns.Count; i++) {
            TownData town = _towns[i];

            float distanceSquaredToTown = town.TownZone.Center.ToVector2().DistanceSQ(roomPos.ToVector2());
            if (distanceSquaredToTown > closestTownDistanceSquared) {
                continue;
            }

            closestTownDistanceSquared = distanceSquaredToTown;
            closestTownIndex = i;
            closestTown = town;
        }

        bool canLinkToTown = closestTown.RoomPositions.Any(point => !(point.ToVector2().DistanceSQ(roomPos.ToVector2()) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking));
        if (!canLinkToTown) {
            AddTownFromRooms([roomPos]);
            return;
        }

        closestTown.RoomPositions.Add(roomPos);
        _towns[closestTownIndex] = closestTown with { TownZone = CreateTownZoneFromRoomPositions(closestTown.RoomPositions) };
    }

    /// <summary>
    ///     Generate all internal towns for each stored home location of the NPCs in the world.
    /// </summary>
    private void CalculateTowns() {
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


            List<Point> finalRoomPositions = linkedVertices.Select(vertex => vertex.ToPoint()).ToList();
            AddTownFromRooms(finalRoomPositions);
        }
    }

    private void AddTownFromRooms(List<Point> roomPositions) {
        _towns.Add(new TownData(CreateTownZoneFromRoomPositions(roomPositions), roomPositions, null));
    }
}