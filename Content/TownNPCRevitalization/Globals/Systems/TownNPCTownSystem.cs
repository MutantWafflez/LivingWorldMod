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

    private readonly record struct TownData(Rectangle2D<int> TownZone, List<Rectangle2D<int>> RoomRects, TownNPCPathfinder TownPathfinder);

    private const int MaximumTileRangeForRoomLinking = 75;

    private List<TownData> _towns;

    private static Rectangle2D<int> CreateTownZoneFromRoomPositions(List<Rectangle2D<int>> roomRects) {
        Point2D<int> topLeftOfTown = new (int.MaxValue, int.MaxValue);
        Point2D<int> bottomRightOfTown = new (int.MinValue, int.MinValue);
        foreach (Rectangle2D<int> rect in roomRects) {
            if (rect.X < topLeftOfTown.X) {
                topLeftOfTown.X = rect.X;
            }

            if (rect.Y < topLeftOfTown.Y) {
                topLeftOfTown.Y = rect.Y;
            }

            if (rect.Right > bottomRightOfTown.X) {
                bottomRightOfTown.X = rect.X + 1;
            }

            if (rect.Y > bottomRightOfTown.Y) {
                bottomRightOfTown.Y = rect.Y + 1;
            }
        }

        return new Rectangle2D<int>(topLeftOfTown, bottomRightOfTown);
    }

    /// <summary>
    ///     Returns a copy of the passed-in town object with the zone replace with a correct one based on the town's room positions. Only really needs to be called if the internal room position list is
    ///     modified without copying the struct.
    /// </summary>
    private static TownData CopyTownWithNewZone(TownData town) => town with { TownZone = CreateTownZoneFromRoomPositions(town.RoomRects) };

    /// <summary>
    ///     Creates a new <see cref="TownNPCPathfinder" /> instance based on the rectangle passed in for use for NPCs within whatever Town has said rectangle as its zone.
    /// </summary>
    private static TownNPCPathfinder CreatePathfinderFromTownZone(Rectangle2D<int> townZone) {
        ushort gridSizeX = Math.Max(LWMUtils.CeilingToNearestPowerOfTwo((ushort)townZone.Width), (ushort)TownNPCPathfinderModule.PathfinderSize);
        ushort gridSizeY = Math.Max(LWMUtils.CeilingToNearestPowerOfTwo((ushort)townZone.Height), (ushort)(TownNPCPathfinderModule.PathfinderSize / 2));

        return new TownNPCPathfinder((townZone.Center - new Point2D<int>(gridSizeX / 2, gridSizeY / 2)).Convert<ushort>(), gridSizeX, gridSizeY);
    }

    /// <summary>
    ///     Confirms that the town in question is still "valid", meaning that all rooms are still within linkable distance (<see cref="MaximumTileRangeForRoomLinking" />) of each other. If valid, return
    ///     true. If invalid, returns false and passes out a list of room positions that were unlinked.
    /// </summary>
    private static bool IsTownValid(TownData townData, out List<Rectangle2D<int>> unlinkedRoomPositions) {
        unlinkedRoomPositions = [];
        if (townData.RoomRects.Count <= 0) {
            return false;
        }

        HashSet<Rectangle2D<int>> visitedSet = [];
        Queue<Rectangle2D<int>> frontier = [];
        frontier.Enqueue(townData.RoomRects[0]);
        while (frontier.Count > 0) {
            Rectangle2D<int> currentRoom = frontier.Dequeue();
            if (!visitedSet.Add(currentRoom)) {
                continue;
            }

            foreach (Rectangle2D<int> linkedRoom in townData.RoomRects) {
                if (currentRoom.Center.DistanceSquared(linkedRoom.Center) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                frontier.Enqueue(linkedRoom);
            }
        }

        if (visitedSet.Count == townData.RoomRects.Count) {
            return true;
        }

        unlinkedRoomPositions = townData.RoomRects.Except(visitedSet).ToList();
        return false;
    }

    public override void ClearWorld() {
        _towns = [];
    }

    public override void PostWorldLoad() {
        _towns = [];

        List<Rectangle2D<int>> roomRects = [];
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.TryGetGlobalNPC(out TownNPCHousingModule housingModule) || housingModule.UpdateRoomBoundingBox() is not { } roomBoundingBox || roomRects.Contains(roomBoundingBox)) {
                continue;
            }

            roomRects.Add(roomBoundingBox);
        }

        CalculateTowns(roomRects);
    }

    public override void PostDrawTiles() {
        if (!LWM.IsDebug) {
            return;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (int i = 0; i < _towns.Count; i++) {
            (Rectangle2D<int> townZone, List<Rectangle2D<int>> roomRects, TownNPCPathfinder pathfinder) = _towns[i];

            Utils.DrawRectForTilesInWorld(Main.spriteBatch, (Rectangle)townZone, new Color((byte)townZone.X, (byte)townZone.Y, (byte)(townZone.X + townZone.Y)));

            string townInfo = $"Town Index: {i}, Room Count: {roomRects.Count}";
            ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                townInfo,
                (Vector2)townZone.ToWorldCoordinates().TopLeft - Main.screenPosition,
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

            foreach (Rectangle2D<int> rect in roomRects) {
                Utils.DrawRectForTilesInWorld(Main.spriteBatch, (Rectangle)rect, Main.DiscoColor);
            }
        }

        Main.spriteBatch.End();
    }

    /// <summary>
    ///     Searches the current list of towns and attempts to add the passed-in room to its proper town. If no town is close enough to be grouped into any town, a new town will be created with the room as
    ///     the only position. This method also is capable of merging towns together if such an edge case occurs.
    /// </summary>
    public void AddRoomToTown(Rectangle2D<int> roomRect) {
        if (_towns.Count <= 0) {
            CreateTownObjectFromRooms([roomRect]);
        }

        List<int> linkableTownIndices = [];
        foreach ((int i, TownData townData) in _towns.Select((town, index) => (Index: index, Town: town)).OrderBy(pair => pair.Town.TownZone.Center.DistanceSquared(roomRect.Center))) {
            foreach (Rectangle2D<int> rect in townData.RoomRects) {
                if (rect.Center.DistanceSquared(roomRect.Center) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                linkableTownIndices.Add(i);

                break;
            }
        }

        switch (linkableTownIndices.Count) {
            case <= 0:
                CreateTownObjectFromRooms([roomRect]);

                return;
            case 1: {
                int firstIndex = linkableTownIndices[0];

                _towns[firstIndex].RoomRects.Add(roomRect);
                _towns[firstIndex] = CopyTownWithNewZone(_towns[firstIndex]);

                return;
            }
        }

        List<Rectangle2D<int>> finalTownRoomRectList = [];
        for (int i = 0; i < linkableTownIndices.Count; i++) {
            int townIndex = linkableTownIndices[i] - i;

            finalTownRoomRectList.AddRange(_towns[townIndex].RoomRects);
            _towns.RemoveAt(townIndex);
        }

        CreateTownObjectFromRooms(finalTownRoomRectList);
    }

    /// <summary>
    ///     Searches all current towns for the one that contains the passed-in room and removes it. If this room was the only one in the town, the town is also removed. If no town was found to contain this
    ///     room, nothing occurs. This method also handles splitting towns into two, if such an edge case occurs (unless disabled; enabled by default).
    /// </summary>
    public void RemoveRoomFromTown(Rectangle2D<int> roomRect, bool doSplitCheck = true) {
        if (_towns.Count <= 0) {
            return;
        }

        int ownedTownIndex = 0;
        int ownedTownRoomPosIndex;
        TownData ownedTown;
        for (int i = 0; i < _towns.Count; i++) {
            TownData town = _towns[i];

            for (int j = 0; j < town.RoomRects.Count; j++) {
                Rectangle2D<int> rect = town.RoomRects[j];
                if (roomRect != rect) {
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
        if (ownedTown.RoomRects.Count <= 1) {
            _towns.RemoveAt(ownedTownIndex);
            return;
        }

        ownedTown.RoomRects.RemoveAt(ownedTownRoomPosIndex);
        _towns[ownedTownIndex] = CopyTownWithNewZone(ownedTown);
        if (!doSplitCheck) {
            return;
        }

        // Verify that the town is still valid, and if not, create new towns based on all of the rooms that were unlinked as a result of the removal
        if (IsTownValid(ownedTown, out List<Rectangle2D<int>> unlinkedRoomPositions)) {
            return;
        }

        ownedTown.RoomRects.RemoveAll(rect => unlinkedRoomPositions.Contains(rect));
        _towns[ownedTownIndex] = CopyTownWithNewZone(ownedTown);

        CalculateTowns(unlinkedRoomPositions);
    }

    /// <summary>
    ///     From a list of room positions, follow a linking process by distance to determine all grouped together rooms, which will be conglomerated into individual <see cref="TownData" /> objects, added to
    ///     the internal <see cref="_towns" /> list.
    /// </summary>
    private void CalculateTowns(List<Rectangle2D<int>> allRoomRects) {
        if (allRoomRects.Count <= 0) {
            return;
        }

        Dictionary<Rectangle2D<int>, List<Rectangle2D<int>>> allLinkedRooms = [];
        // First pass; link all rooms that are directly connected via the maximum distance
        for (int i = 0; i < allRoomRects.Count; i++) {
            Rectangle2D<int> rectOne = allRoomRects[i];
            allLinkedRooms[rectOne] = [];

            for (int j = 0; j < allRoomRects.Count; j++) {
                if (i == j) {
                    continue;
                }

                Rectangle2D<int> rectTwo = allRoomRects[j];
                if (rectOne.Center.DistanceSquared(rectTwo.Center) > MaximumTileRangeForRoomLinking * MaximumTileRangeForRoomLinking) {
                    continue;
                }

                allLinkedRooms[rectOne].Add(rectTwo);
            }
        }

        // Second pass; link all rooms that are "chained" together via multiple BFS (my goat), i.e. transitive (if A -> B and B -> C, then A -> C)
        HashSet<Rectangle2D<int>> visitedSet = [];
        foreach (Rectangle2D<int> rootRect in allRoomRects) {
            if (visitedSet.Contains(rootRect)) {
                continue;
            }

            List<Rectangle2D<int>> linkedRooms = [];
            Queue<Rectangle2D<int>> frontier = [];
            frontier.Enqueue(rootRect);
            while (frontier.Count > 0) {
                Rectangle2D<int> currentVertex = frontier.Dequeue();
                linkedRooms.Add(currentVertex);
                visitedSet.Add(currentVertex);

                foreach (Rectangle2D<int> linkedRect in allLinkedRooms[currentVertex]) {
                    if (!visitedSet.Add(linkedRect)) {
                        continue;
                    }

                    frontier.Enqueue(linkedRect);
                }
            }

            CreateTownObjectFromRooms(linkedRooms);
        }
    }

    /// <summary>
    ///     Given a list of room positions, add a new <see cref="TownData" /> object to the internal <see cref="_towns" /> list. This method assumes that the room positions have been determined to be linked
    ///     together by distance.
    /// </summary>
    private void CreateTownObjectFromRooms(List<Rectangle2D<int>> roomPositions) {
        Rectangle2D<int> townZone = CreateTownZoneFromRoomPositions(roomPositions);

        _towns.Add(new TownData(townZone, roomPositions, CreatePathfinderFromTownZone(townZone)));
    }
}