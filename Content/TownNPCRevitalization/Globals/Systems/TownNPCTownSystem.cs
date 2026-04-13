using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.DataStructures.Records;
using LivingWorldMod.Globals.BaseTypes.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;

/// <summary>
///     System that handles the conglomeration of Town NPCs in the same area, internally referred to as a "Town". Manages the shared usage of a pathfinder grid for performance, "Points of Interest"
///     for selecting places for Town NPCs to pathfind to, and more to-be implemented.
/// </summary>
public class TownNPCTownSystem : BaseModSystem<TownNPCTownSystem> {
    private enum PointOfInterestType : byte {
        None
    }

    private readonly record struct PointOfInterest(PointOfInterestType Type, Point2D<int> Position);

    private readonly record struct TownData(Rectangle TownZone, List<Point2D<int>> RoomPositions, TownNPCPathfinder TownPathfinder);

    private const int MaximumTileRangeForRoomLinking = 75;

    private List<TownData> _towns;

    private static Rectangle CreateTownZoneFromRoomPositions(List<Point2D<int>> roomPositions) {
        Point2D<int> topLeftOfTown = new (int.MaxValue, int.MaxValue);
        Point2D<int> bottomRightOfTown = new (int.MinValue, int.MinValue);
        foreach (Point2D<int> point in roomPositions) {
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

        return LWMUtils.NewRectFromCorners((Point)topLeftOfTown, (Point)bottomRightOfTown);
    }

    /// <summary>
    ///     Returns a copy of the passed-in town object with the zone replace with a correct one based on the town's room positions. Only really needs to be called if the internal room position list is
    ///     modified without copying the struct.
    /// </summary>
    private static TownData CopyTownWithNewZone(TownData town) => town with { TownZone = CreateTownZoneFromRoomPositions(town.RoomPositions) };

    /// <summary>
    ///     Creates a new <see cref="TownNPCPathfinder" /> instance based on the rectangle passed in for use for NPCs within whatever Town has said rectangle as its zone.
    /// </summary>
    private static TownNPCPathfinder CreatePathfinderFromTownZone(Rectangle townZone) {
        ushort gridSize = Math.Max(LWMUtils.CeilingToNearestPowerOfTwo((ushort)Math.Max(townZone.Width, townZone.Height)), (ushort)TownNPCPathfinderModule.PathfinderSize);

        return new TownNPCPathfinder((Point2D<ushort>)(townZone.Center - new Point(gridSize / 2, gridSize / 2)), gridSize);
    }

    public override void ClearWorld() {
        _towns = [];
    }

    public override void PostWorldLoad() {
        _towns = [];

        List<Point2D<int>> roomPoints = [];
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.TryGetGlobalNPC(out TownNPCHousingModule housingModule) || housingModule.UpdateRoomBoundingBox() is not { } roomBoundingBox) {
                continue;
            }

            Point2D<int> point = new(roomBoundingBox.X, roomBoundingBox.Y);
            if (roomPoints.Contains(point)) {
                continue;
            }

            roomPoints.Add(point);
        }

        CalculateTowns(roomPoints);
    }

    public override void PostDrawTiles() {
        if (!LWM.IsDebug) {
            return;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (int i = 0; i < _towns.Count; i++) {
            (Rectangle townZone, List<Point2D<int>> roomPositions, TownNPCPathfinder pathfinder) = _towns[i];

            Utils.DrawRectForTilesInWorld(Main.spriteBatch, townZone, new Color((byte)townZone.X, (byte)townZone.Y, (byte)(townZone.X + townZone.Y)));

            string townInfo = $"Town Index: {i}, Room Count: {roomPositions.Count}";
            ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                townInfo,
                townZone.ToWorldCoordinates().TopLeft() - Main.screenPosition,
                Color.White,
                0f,
                Vector2.Zero,
                Vector2.One
            );

            Utils.DrawRectForTilesInWorld(
                Main.spriteBatch,
                new Rectangle(pathfinder.topLeftOfGrid.X, pathfinder.topLeftOfGrid.Y, pathfinder.gridSizeX, pathfinder.gridSizeY),
                Color.White
            );

            string pathfinderInfo = $"Pathfinder Grid Size: {pathfinder.gridSizeX}, {pathfinder.gridSizeY}";
            ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                pathfinderInfo,
                pathfinder.topLeftOfGrid.ToWorldCoordinates() - Main.screenPosition,
                Color.White,
                0f,
                Vector2.Zero,
                Vector2.One
            );

            foreach (Point2D<int> point in roomPositions) {
                Utils.DrawRectForTilesInWorld(Main.spriteBatch, new Rectangle(point.X, point.Y, 1, 1), Main.DiscoColor);
            }
        }

        Main.spriteBatch.End();
    }

    /// <summary>
    ///     Searches the current list of towns and attempts to add the passed-in room to its proper town. If no town is close enough to be grouped into any town, a new town will be created with the room as
    ///     the only position. This method also is capable of merging towns together if such an edge case occurs.
    /// </summary>
    public void AddRoomToTown(Point2D<int> roomPos) {
        if (_towns.Count <= 0) {
            CreateTownObjectFromRooms([roomPos]);
        }

        List<int> linkableTownIndices = [];
        foreach ((int i, TownData townData) in _towns.Select((town, index) => (Index: index, Town: town)).OrderBy(pair => pair.Town.TownZone.Center.ToVector2().DistanceSQ((Vector2)roomPos))) {
            foreach (Point2D<int> point in townData.RoomPositions) {
                if (point.DistanceSquared(roomPos) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                linkableTownIndices.Add(i);

                break;
            }
        }

        switch (linkableTownIndices.Count) {
            case <= 0:
                CreateTownObjectFromRooms([roomPos]);

                return;
            case 1: {
                int firstIndex = linkableTownIndices[0];

                _towns[firstIndex].RoomPositions.Add(roomPos);
                _towns[firstIndex] = CopyTownWithNewZone(_towns[firstIndex]);

                return;
            }
        }

        List<Point2D<int>> finalTownRoomPosList = [];
        for (int i = 0; i < linkableTownIndices.Count; i++) {
            int townIndex = linkableTownIndices[i] - i;

            finalTownRoomPosList.AddRange(_towns[townIndex].RoomPositions);
            _towns.RemoveAt(townIndex);
        }

        CreateTownObjectFromRooms(finalTownRoomPosList);
    }

    /// <summary>
    ///     Searches all current towns for the one that contains the passed-in room and removes it. If this room was the only one in the town, the town is also removed. If no town was found to contain this
    ///     room, nothing occurs. This method also handles splitting towns into two, if such an edge case occurs (unless disabled; enabled by default).
    /// </summary>
    public void RemoveRoomFromTown(Point2D<int> roomPos, bool doSplitCheck = true) {
        if (_towns.Count <= 0) {
            return;
        }

        int ownedTownIndex = 0;
        int ownedTownRoomPosIndex;
        TownData ownedTown = _towns[ownedTownIndex];
        for (int i = 0; i < _towns.Count; i++) {
            TownData town = _towns[i];

            for (int j = 0; j < town.RoomPositions.Count; j++) {
                Point2D<int> point = town.RoomPositions[j];
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
        if (IsTownValid(ownedTown, out List<Point2D<int>> unlinkedRoomPositions)) {
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
    private bool IsTownValid(TownData townData, out List<Point2D<int>> unlinkedRoomPositions) {
        unlinkedRoomPositions = [];
        if (townData.RoomPositions.Count <= 0) {
            return false;
        }

        HashSet<Point2D<int>> visitedSet = [];
        Queue<Point2D<int>> frontier = [];
        frontier.Enqueue(townData.RoomPositions[0]);
        while (frontier.Count > 0) {
            Point2D<int> currentVertex = frontier.Dequeue();
            if (!visitedSet.Add(currentVertex)) {
                continue;
            }

            foreach (Point2D<int> linkedVertex in townData.RoomPositions) {
                if (currentVertex.DistanceSquared(linkedVertex) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                frontier.Enqueue(linkedVertex);
            }
        }

        if (visitedSet.Count == townData.RoomPositions.Count) {
            return true;
        }

        unlinkedRoomPositions = townData.RoomPositions.Except(visitedSet).ToList();
        return false;
    }

    /// <summary>
    ///     From a list of room positions, follow a linking process by distance to determine all grouped together rooms, which will be conglomerated into individual <see cref="TownData" /> objects, added to
    ///     the internal <see cref="_towns" /> list.
    /// </summary>
    private void CalculateTowns(List<Point2D<int>> allRoomPositions) {
        if (allRoomPositions.Count <= 0) {
            return;
        }

        Dictionary<Point2D<int>, List<Point2D<int>>> allLinkedRooms = [];
        // First pass; link all rooms that are directly connected via the maximum distance
        for (int i = 0; i < allRoomPositions.Count; i++) {
            Point2D<int> posOne = allRoomPositions[i];
            allLinkedRooms[posOne] = [];

            for (int j = 0; j < allRoomPositions.Count; j++) {
                if (i == j) {
                    continue;
                }

                Point2D<int> posTwo = allRoomPositions[j];
                if (posOne.DistanceSquared(posTwo) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                allLinkedRooms[posOne].Add(posTwo);
            }
        }

        // Second pass; link all rooms that are "chained" together via multiple BFS (my goat), i.e. transitive (if A -> B and B -> C, then A -> C)
        HashSet<Point2D<int>> visitedSet = [];
        foreach (Point2D<int> rootVertex in allRoomPositions) {
            if (visitedSet.Contains(rootVertex)) {
                continue;
            }

            List<Point2D<int>> linkedVertices = [];
            Queue<Point2D<int>> frontier = [];
            frontier.Enqueue(rootVertex);
            while (frontier.Count > 0) {
                Point2D<int> currentVertex = frontier.Dequeue();
                linkedVertices.Add(currentVertex);
                visitedSet.Add(currentVertex);

                foreach (Point2D<int> linkedVertex in allLinkedRooms[currentVertex]) {
                    if (!visitedSet.Add(linkedVertex)) {
                        continue;
                    }

                    frontier.Enqueue(linkedVertex);
                }
            }

            CreateTownObjectFromRooms(linkedVertices);
        }
    }

    /// <summary>
    ///     Given a list of room positions, add a new <see cref="TownData" /> object to the internal <see cref="_towns" /> list. This method assumes that the room positions have been determined to be linked
    ///     together by distance.
    /// </summary>
    private void CreateTownObjectFromRooms(List<Point2D<int>> roomPositions) {
        Rectangle townZone = CreateTownZoneFromRoomPositions(roomPositions);

        _towns.Add(new TownData(townZone, roomPositions, CreatePathfinderFromTownZone(townZone)));
    }
}