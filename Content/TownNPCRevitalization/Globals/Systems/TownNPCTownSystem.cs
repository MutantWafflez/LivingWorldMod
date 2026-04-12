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

    private readonly record struct PointOfInterest(PointOfInterestType Type, HashPoint<int> Position);

    private readonly record struct TownData(Rectangle TownZone, List<HashPoint<int>> RoomPositions, TownNPCPathfinder TownPathfinder);

    private const int MaximumTileRangeForRoomLinking = 75;

    private List<TownData> _towns;

    private static Rectangle CreateTownZoneFromRoomPositions(List<HashPoint<int>> roomPositions) {
        HashPoint<int> topLeftOfTown = new (int.MaxValue, int.MaxValue);
        HashPoint<int> bottomRightOfTown = new (int.MinValue, int.MinValue);
        foreach (HashPoint<int> point in roomPositions) {
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

        List<HashPoint<int>> roomPoints = [];
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.TryGetGlobalNPC(out TownNPCHousingModule housingModule) || housingModule.UpdateRoomBoundingBox() is not { } roomBoundingBox) {
                continue;
            }

            HashPoint<int> point = new(roomBoundingBox.X, roomBoundingBox.Y);
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
            TownData town = _towns[i];
            Utils.DrawRectForTilesInWorld(Main.spriteBatch, town.TownZone, new Color((byte)town.TownZone.X, (byte)town.TownZone.Y, (byte)(town.TownZone.X + town.TownZone.Y)));

            string townInfo = $"Town Index: {i}, Room Count: {town.RoomPositions.Count}";
            ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                townInfo,
                town.TownZone.ToWorldCoordinates().TopLeft() - Main.screenPosition,
                Color.White,
                0f,
                Vector2.Zero,
                Vector2.One
            );

            foreach (HashPoint<int> point in town.RoomPositions) {
                Utils.DrawRectForTilesInWorld(Main.spriteBatch, new Rectangle(point.X, point.Y, 1, 1), Main.DiscoColor);
            }
        }

        Main.spriteBatch.End();
    }

    /// <summary>
    ///     Searches the current list of towns and attempts to add the passed-in room to its proper town. If no town is close enough to be grouped into any town, a new town will be created with the room as
    ///     the only position. This method also is capable of merging towns together if such an edge case occurs.
    /// </summary>
    public void AddRoomToTown(HashPoint<int> roomPos) {
        if (_towns.Count <= 0) {
            CreateTownObjectFromRooms([roomPos]);
        }

        List<TownData> townsByDistance = _towns.OrderBy(town => town.TownZone.Center.ToVector2().DistanceSQ((Vector2)roomPos)).ToList();
        List<int> linkableTownIndices = [];
        for (int i = 0; i < townsByDistance.Count; i++) {
            TownData townData = townsByDistance[i];

            foreach (HashPoint<int> point in townData.RoomPositions) {
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

        List<HashPoint<int>> finalTownRoomPosList = [];
        for (int i = 0; i < linkableTownIndices.Count; i++) {
            int townIndex = linkableTownIndices[i];

            finalTownRoomPosList.AddRange(_towns[townIndex].RoomPositions);
            _towns.RemoveAt(townIndex - i);
        }

        CreateTownObjectFromRooms(finalTownRoomPosList);
    }

    /// <summary>
    ///     Searches all current towns for the one that contains the passed-in room and removes it. If this room was the only one in the town, the town is also removed. If no town was found to contain this
    ///     room, nothing occurs. This method also handles splitting towns into two, if such an edge case occurs (unless disabled; enabled by default).
    /// </summary>
    public void RemoveRoomFromTown(HashPoint<int> roomPos, bool doSplitCheck = true) {
        if (_towns.Count <= 0) {
            return;
        }

        int ownedTownIndex = 0;
        int ownedTownRoomPosIndex = 0;
        TownData ownedTown = _towns[ownedTownIndex];
        for (int i = 0; i < _towns.Count; i++) {
            TownData town = _towns[i];

            for (int j = 0; j < town.RoomPositions.Count; j++) {
                HashPoint<int> point = town.RoomPositions[j];
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
        if (IsTownValid(ownedTown, out List<HashPoint<int>> unlinkedRoomPositions)) {
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
    private bool IsTownValid(TownData townData, out List<HashPoint<int>> unlinkedRoomPositions) {
        unlinkedRoomPositions = [];
        if (townData.RoomPositions.Count <= 0) {
            return false;
        }

        HashSet<HashPoint<int>> visitedSet = [];
        Queue<HashPoint<int>> frontier = [];
        frontier.Enqueue(townData.RoomPositions[0]);
        while (frontier.Count > 0) {
            HashPoint<int> currentVertex = frontier.Dequeue();
            if (!visitedSet.Add(currentVertex)) {
                continue;
            }

            foreach (HashPoint<int> linkedVertex in townData.RoomPositions) {
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
    private void CalculateTowns(List<HashPoint<int>> allRoomPositions) {
        if (allRoomPositions.Count <= 0) {
            return;
        }

        Dictionary<HashPoint<int>, List<HashPoint<int>>> allLinkedRooms = [];
        // First pass; link all rooms that are directly connected via the maximum distance
        for (int i = 0; i < allRoomPositions.Count; i++) {
            HashPoint<int> posOne = allRoomPositions[i];
            allLinkedRooms[posOne] = [];

            for (int j = 0; j < allRoomPositions.Count; j++) {
                if (i == j) {
                    continue;
                }

                HashPoint<int> posTwo = allRoomPositions[j];
                if (posOne.DistanceSquared(posTwo) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                allLinkedRooms[posOne].Add(posTwo);
            }
        }

        // Second pass; link all rooms that are "chained" together via multiple BFS (my goat), i.e. transitive (if A -> B and B -> C, then A -> C)
        HashSet<HashPoint<int>> visitedSet = [];
        foreach (HashPoint<int> rootVertex in allRoomPositions) {
            if (visitedSet.Contains(rootVertex)) {
                continue;
            }

            List<HashPoint<int>> linkedVertices = [];
            Queue<HashPoint<int>> frontier = [];
            frontier.Enqueue(rootVertex);
            while (frontier.Count > 0) {
                HashPoint<int> currentVertex = frontier.Dequeue();
                linkedVertices.Add(currentVertex);
                visitedSet.Add(currentVertex);

                foreach (HashPoint<int> linkedVertex in allLinkedRooms[currentVertex]) {
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
    private void CreateTownObjectFromRooms(List<HashPoint<int>> roomPositions) {
        _towns.Add(new TownData(CreateTownZoneFromRoomPositions(roomPositions), roomPositions, null));
    }
}