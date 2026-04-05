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

    /// <summary>
    ///     Returns a copy of the passed-in town object with the zone replace with a correct one based on the town's room positions. Only really needs to be called if the internal room position list is
    ///     modified without copying the struct.
    /// </summary>
    private static TownData CopyTownWithNewZone(TownData town) => town with { TownZone = CreateTownZoneFromRoomPositions(town.RoomPositions) };

    public override void ClearWorld() {
        _towns = [];
    }

    public override void PostWorldLoad() {
        _towns = [];

        CalculateTowns(WorldGen.TownManager._roomLocationPairs.Select(pair => pair.Item2).Distinct().ToList());
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
            CreateTownObjectFromRooms([roomPos]);
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
            CreateTownObjectFromRooms([roomPos]);
            return;
        }

        closestTown.RoomPositions.Add(roomPos);
        _towns[closestTownIndex] = CopyTownWithNewZone(closestTown);
    }

    /// <summary>
    ///     Searches all current towns for the one that contains the passed-in room and removes it. If this room was the only one in the town, the town is also removed. If no town was found to contain this
    ///     room, nothing occurs. This method also handles splitting towns into two, if such an edge case occurs (unless disabled; enabled by default).
    /// </summary>
    public void RemoveRoomFromTown(Point roomPos, bool doSplitCheck = true) {
        if (_towns.Count <= 0) {
            return;
        }

        int ownedTownIndex = 0;
        int ownedTownRoomPosIndex = 0;
        TownData ownedTown = _towns[ownedTownIndex];
        for (int i = 0; i < _towns.Count; i++) {
            TownData town = _towns[i];

            for (int j = 0; j < town.RoomPositions.Count; j++) {
                Point point = town.RoomPositions[j];
                if (roomPos != point) {
                    continue;
                }

                ownedTownIndex = i;
                ownedTown = town;

                ownedTownRoomPosIndex = j;
                goto OutsideLoop;
            }
        }

        return;

        OutsideLoop:
        if (ownedTown.RoomPositions.Count <= 1) {
            _towns.RemoveAt(ownedTownIndex);
            return;
        }

        ownedTown.RoomPositions.RemoveAt(ownedTownRoomPosIndex);
        _towns[ownedTownIndex] = CopyTownWithNewZone(ownedTown);
        if (!doSplitCheck) {
            return;
        }

        // Verify that the town is still valid, and if not, create new towns based on all of the rooms that were unlinked as a result of the removal
        if (IsTownValid(ownedTown, out List<Point> unlinkedRoomPositions)) {
            return;
        }

        ownedTown.RoomPositions.RemoveAll(pos => unlinkedRoomPositions.Contains(pos));
        _towns[ownedTownIndex] = CopyTownWithNewZone(ownedTown);

        CalculateTowns(unlinkedRoomPositions);
    }

    /// <summary>
    ///     Confirms that the town in question is still "valid", meaning that all rooms are still within linkable distance (<see cref="MaximumTileRangeForRoomLinking" />) of each other. If valid, return
    ///     true. If invalid, returns false and passes out a list of room positions that were unlinked.
    /// </summary>
    private bool IsTownValid(TownData townData, out List<Point> unlinkedRoomPositions) {
        unlinkedRoomPositions = [];
        if (townData.RoomPositions.Count <= 0) {
            return false;
        }

        HashSet<Vector2> visitedSet = [];
        Queue<Vector2> frontier = [];
        frontier.Enqueue(townData.RoomPositions[0].ToVector2());
        while (frontier.Count > 0) {
            Vector2 currentVertex = frontier.Dequeue();
            if (!visitedSet.Add(currentVertex)) {
                continue;
            }

            foreach (Point linkedRoomPoint in townData.RoomPositions) {
                Vector2 linkedVertex = linkedRoomPoint.ToVector2();
                if (currentVertex.DistanceSQ(linkedVertex) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                frontier.Enqueue(linkedVertex);
            }
        }

        if (visitedSet.Count == townData.RoomPositions.Count) {
            return true;
        }

        unlinkedRoomPositions = townData.RoomPositions.Except(visitedSet.Select(vector => vector.ToPoint())).ToList();
        return false;
    }

    /// <summary>
    ///     From a list of room positions, follow a linking process by distance to determine all grouped together rooms, which will be conglomerated into individual <see cref="TownData" /> objects, added to
    ///     the internal <see cref="_towns" /> list.
    /// </summary>
    private void CalculateTowns(List<Point> allRoomPositions) {
        if (allRoomPositions.Count <= 0) {
            return;
        }

        // Using Vector2 for its better hash function in comparison to Point
        List<Vector2> allRoomVertices = allRoomPositions.Select(point => point.ToVector2()).ToList();
        Dictionary<Vector2, List<Vector2>> allLinkedRooms = [];

        // First pass; link all rooms that are directly connected via the maximum distance
        for (int i = 0; i < allRoomVertices.Count; i++) {
            Vector2 posOne = allRoomVertices[i];
            allLinkedRooms[posOne] = [];

            for (int j = 0; j < allRoomVertices.Count; j++) {
                if (i == j) {
                    continue;
                }

                Vector2 posTwo = allRoomVertices[j];
                if (posOne.DistanceSQ(posTwo) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                allLinkedRooms[posOne].Add(posTwo);
            }
        }

        // Second pass; link all rooms that are "chained" together via multiple BFS (my goat), i.e. transitive (if A -> B and B -> C, then A -> C)
        HashSet<Vector2> visitedSet = [];
        foreach (Vector2 rootVertex in allRoomVertices) {
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
            CreateTownObjectFromRooms(finalRoomPositions);
        }
    }

    /// <summary>
    ///     Given a list of room positions, add a new <see cref="TownData" /> object to the internal <see cref="_towns" /> list. This method assumes that the room positions have been determined to be linked
    ///     together by distance.
    /// </summary>
    private void CreateTownObjectFromRooms(List<Point> roomPositions) {
        _towns.Add(new TownData(CreateTownZoneFromRoomPositions(roomPositions), roomPositions, null));
    }
}