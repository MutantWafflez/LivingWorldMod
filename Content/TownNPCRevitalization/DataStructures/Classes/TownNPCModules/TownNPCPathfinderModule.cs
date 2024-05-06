using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Configs;
using LivingWorldMod.Globals.Configs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader.IO;
using PathNode = LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCPathfinder.PathNode;
using NodeMovementType = LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCPathfinder.NodeMovementType;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

public sealed class TownNPCPathfinderModule : TownNPCModule {
    private sealed class PathfinderResult(Point topLeftOfGrid, Point endPoint, PathNode lastConsumedNode, List<PathNode> path) {
        public readonly Point topLeftOfGrid = topLeftOfGrid;
        public readonly Point endPoint = endPoint;
        public readonly List<PathNode> path = path;
        public PathNode lastConsumedNode = lastConsumedNode;
    }

    private const int MaxPathRecyclesBeforeFailure = 5;

    public static int PathfinderSize => ModContent.GetInstance<TownNPCConfig>().pathfinderSize;

    public bool IsPathfinding => _currentPathfinderResult is not null;

    public Point BottomLeftTileOfNPC => npc.BottomLeft.ToTileCoordinates() + new Point(0, -1);

    public Point TopLeftOfPathfinderZone => BottomLeftTileOfNPC - new Point(PathfinderSize / 2, PathfinderSize / 2);

    private readonly List<PathfinderResult> _cachedResults;

    private float _prevDistanceToNextNode;
    private int _notMakingProgressCounter;
    private int _pathRestartCount;

    private bool _isPaused;
    private bool _wasPaused;

    private TownNPCPathfinder _cachedPathfinder;
    private PathfinderResult _currentPathfinderResult;

    public TownNPCPathfinderModule(NPC npc) : base(npc) {
        _cachedResults = [];
    }

    public override void Update() {
        _cachedResults.Clear();
        _cachedPathfinder = null;

        if (!IsPathfinding) {
            return;
        }

        Point topLeftOfGrid = _currentPathfinderResult.topLeftOfGrid;
        List<PathNode> path = _currentPathfinderResult.path;
        ref PathNode lastConsumedNode = ref _currentPathfinderResult.lastConsumedNode;

        PathNode nextNode = path.Last();

        if (_isPaused) {
            _isPaused = false;
            _wasPaused = true;
            npc.velocity.X = 0f;
            return;
        }

        if (_wasPaused) {
            _wasPaused = false;
            GenerateAndUseNewPath(_currentPathfinderResult.endPoint);
            return;
        }

        // If the NPC does not make meaningful progress to the next node, regenerate the path
        float curDistanceToNextNode = GetDistanceToNode(nextNode);
        if (curDistanceToNextNode >= _prevDistanceToNextNode) {
            if (++_notMakingProgressCounter >= LWMUtils.RealLifeSecond / 2) {
                if (_pathRestartCount++ > MaxPathRecyclesBeforeFailure) {
                    EndPathfinding();
                    return;
                }

                GenerateAndUseNewPath(_currentPathfinderResult.endPoint);

                return;
            }
        }
        else {
            _notMakingProgressCounter = 0;
        }
        _prevDistanceToNextNode = curDistanceToNextNode;

        Vector2 nextNodeCenter = (topLeftOfGrid + new Point(nextNode.NodePos.x, nextNode.NodePos.y)).ToWorldCoordinates();
        Vector2 nextNodeBottom = nextNodeCenter + new Vector2(0f, 8f);

        Rectangle nodeRectangle = new((int)(nextNodeCenter.X - 8f), (int)(nextNodeCenter.Y - 8f), 16, 16);
        Rectangle npcNodeCollisionRectangle = new((int)npc.BottomLeft.X, (int)npc.BottomLeft.Y - 16, 16, 16);

        bool leftHasBreachedNode = npc.direction == 1 ? npc.Left.X >= nextNodeCenter.X : npc.Left.X <= nextNodeCenter.X;

        TownNPCCollisionModule collisionModule = GlobalNPC.CollisionModule;
        if (leftHasBreachedNode && nodeRectangle.Intersects(npcNodeCollisionRectangle) && (lastConsumedNode.MovementType is not NodeMovementType.Jump || npc.velocity.Y == 0f)) {
            lastConsumedNode = nextNode;
            path.RemoveAt(path.Count - 1);

            if (path.Count == 0) {
                EndPathfinding();
                return;
            }

            nextNode = path.Last();
            nextNodeCenter = (topLeftOfGrid + new Point(nextNode.NodePos.x, nextNode.NodePos.y)).ToWorldCoordinates();

            if (lastConsumedNode.MovementType is NodeMovementType.Jump) {
                collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = false;
                npc.BottomLeft = nextNodeBottom;

                //Reset velocity & calculate jump vector required to reach jump destination
                npc.velocity *= 0f;

                float npcGravity = npc.gravity;
                float jumpHeight = npc.BottomLeft.Y - (topLeftOfGrid + new Point(nextNode.NodePos.x, nextNode.NodePos.y)).ToWorldCoordinates(8f, 0f).Y;
                float yDisplacement = jumpHeight - npc.height;

                // Horizontal Velocity = X Displacement / (sqrt(-2 * h / g) + sqrt(2(Y Displacement - h) / g))
                // npc ^ formula assumes gravity is negative; it is not because positive Y is down in Terraria. Thus, removed some of the negatives
                npc.velocity.X = (float)((nextNodeCenter.X - npc.Left.X) / (Math.Sqrt(2 * jumpHeight / npcGravity) + Math.Sqrt(2 * (jumpHeight - yDisplacement) / npcGravity)));

                // Vertical velocity = sqrt(-2gh)
                // Entire formula negated since, again, positive Y is down
                npc.velocity.Y = (float)-Math.Sqrt(2 * npcGravity * jumpHeight);
            }

            leftHasBreachedNode = false;
            npc.direction = npc.Left.X > nextNodeCenter.X ? -1 : 1;
            collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = collisionModule.walkThroughStairs = false;
        }

        switch (lastConsumedNode.MovementType) {
            //Step movements or horizontal movements
            case NodeMovementType.StepUp:
                npc.velocity.X = npc.direction;
                Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

                collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = collisionModule.walkThroughStairs = false;
                CheckForDoors();
                break;
            case NodeMovementType.StepDown:
                npc.velocity.X = npc.direction;

                collisionModule.fallThroughStairs = collisionModule.walkThroughStairs = collisionModule.fallThroughPlatforms = false;
                CheckForDoors();
                break;
            case NodeMovementType.PureHorizontal: {
                npc.velocity.X = npc.direction;
                CheckForDoors();

                collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = false;
                collisionModule.walkThroughStairs = true;
                break;
            }
            //Fall movements
            case NodeMovementType.Fall: {
                collisionModule.walkThroughStairs = false;
                if (npc.velocity.Y == 0f) {
                    npc.velocity.X = npc.direction;
                }

                if (!leftHasBreachedNode) {
                    return;
                }

                npc.velocity.X = 0f;
                if (npc.velocity.Y != 0f) {
                    return;
                }

                collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = true;
                break;
            }
        }
    }

    public bool RequestPathfind(Point location) {
        if (IsPathfinding) {
            return false;
        }

        GenerateAndUseNewPath(location);
        return true;
    }

    public bool HasPath(Point location) => GetPathfinderResult(location) is not null;

    public void PausePathfind() => _isPaused = true;

    public void CancelPathfind() => EndPathfinding();

    public void DebugDrawPath(SpriteBatch spriteBatch, Vector2 screenPos) {
        if (!IsPathfinding) {
            return;
        }

        foreach (PathNode node in _currentPathfinderResult.path) {
            Color nodeColor = node.MovementType switch {
                NodeMovementType.StepUp => Color.Green,
                NodeMovementType.StepDown => Color.OrangeRed,
                NodeMovementType.Jump => Color.Purple,
                NodeMovementType.Fall => Color.Red,
                _ => Color.White
            };

            spriteBatch.Draw(TextureAssets.Extra[66].Value, _currentPathfinderResult.topLeftOfGrid.ToWorldCoordinates(2f, 2.5f) + new Vector2(node.NodePos.x, node.NodePos.y).ToWorldCoordinates(0f, 0f) - screenPos, nodeColor);
        }
    }

    public void SendNetworkData(BitWriter bitWriter, BinaryWriter binaryWriter) {
        Point16 endPoint = new(_currentPathfinderResult?.endPoint ?? new Point(-1, -1));
        binaryWriter.Write(endPoint.X);
        binaryWriter.Write(endPoint.Y);
    }

    public void ReceiveNetworkData(BitReader bitReader, BinaryReader binaryReader) {
        Point endPoint = new(binaryReader.ReadInt16(), binaryReader.ReadInt16());
        EndPathfinding();
        if (endPoint is { X: -1, Y: -1 }) {
            return;
        }

        GenerateAndUseNewPath(endPoint);
    }

    private void CheckForDoors() {
        // Direct vanilla code (sorta disgusting)
        if (npc.closeDoor && ((npc.position.X + npc.width / 2f) / 16f > npc.doorX + 2 || (npc.position.X + npc.width / 2f) / 16f < npc.doorX - 2)) {
            Tile doorPos = Framing.GetTileSafely(npc.doorX, npc.doorY);

            if (TileLoader.CloseDoorID(doorPos) >= 0) {
                if (WorldGen.CloseDoor(npc.doorX, npc.doorY)) {
                    npc.closeDoor = false;
                    NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 1, npc.doorX, npc.doorY, npc.direction);
                }

                if ((npc.position.X + npc.width / 2f) / 16f > npc.doorX + 4 || (npc.position.X + npc.width / 2f) / 16f < npc.doorX - 4 || (npc.position.Y + npc.height / 2f) / 16f > npc.doorY + 4 || (npc.position.Y + npc.height / 2f) / 16f < npc.doorY - 4) {
                    npc.closeDoor = false;
                }
            }
            else if (doorPos.TileType == TileID.TallGateOpen) {
                if (WorldGen.ShiftTallGate(npc.doorX, npc.doorY, true)) {
                    npc.closeDoor = false;
                    NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 5, npc.doorX, npc.doorY);
                }

                if ((npc.position.X + npc.width / 2f) / 16f > npc.doorX + 4 || (npc.position.X + npc.width / 2f) / 16f < npc.doorX - 4 || (npc.position.Y + npc.height / 2f) / 16f > npc.doorY + 4 || (npc.position.Y + npc.height / 2f) / 16f < npc.doorY - 4) {
                    npc.closeDoor = false;
                }
            }
            else {
                npc.closeDoor = false;
            }
        }

        // How vanilla does it; keeping it the same lest some weird edge case happens
        Point headTilePos = new((int)((npc.position.X + npc.width / 2f + 15 * npc.direction) / 16f), (int)((npc.position.Y + npc.height - 16f) / 16f) - 2);
        Tile topTile = Framing.GetTileSafely(headTilePos.X, headTilePos.Y);

        if (!topTile.HasUnactuatedTile || !TileLoader.IsClosedDoor(topTile) && topTile.TileType != TileID.TallGateClosed || Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        if (WorldGen.OpenDoor(headTilePos.X, headTilePos.Y, npc.direction)) {
            npc.closeDoor = true;
            npc.doorX = headTilePos.X;
            npc.doorY = headTilePos.Y;
            NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, headTilePos.X, headTilePos.Y, npc.direction);
            npc.netUpdate = true;
        }
        else if (WorldGen.OpenDoor(headTilePos.X, headTilePos.Y, -npc.direction)) {
            npc.closeDoor = true;
            npc.doorX = headTilePos.X;
            npc.doorY = headTilePos.Y;
            NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, headTilePos.X, headTilePos.Y, -npc.direction);
            npc.netUpdate = true;
        }
        else if (WorldGen.ShiftTallGate(headTilePos.X, headTilePos.Y, false)) {
            npc.closeDoor = true;
            npc.doorX = headTilePos.X;
            npc.doorY = headTilePos.Y;
            NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 4, headTilePos.X, headTilePos.Y);
            npc.netUpdate = true;
        }
        /*else {
            // TODO: Fix scenario where NPC can't open door

            npc.direction *= -1;
            npc.netUpdate = true;
        }*/
    }

    private PathfinderResult GetPathfinderResult(Point endPoint) {
        if (_cachedResults.FirstOrDefault(result => result.endPoint == endPoint) is { } cachedResult) {
            _currentPathfinderResult = cachedResult;
            return _currentPathfinderResult;
        }

        Point topLeftOfGrid = TopLeftOfPathfinderZone;

        const int worldFluff = 10;
        topLeftOfGrid.X = Utils.Clamp(topLeftOfGrid.X, worldFluff, Main.maxTilesX - PathfinderSize - worldFluff - 1);
        topLeftOfGrid.Y = Utils.Clamp(topLeftOfGrid.Y, 0, Main.maxTilesY - PathfinderSize - 1);

        TownNPCPathfinder pathFinder =
            _cachedPathfinder
            ?? new TownNPCPathfinder(new UPoint16(topLeftOfGrid.X, topLeftOfGrid.Y), (ushort)PathfinderSize, (ushort)Math.Ceiling(npc.width / 16f), (ushort)Math.Ceiling(npc.height / 16f));

        List<PathNode> path = pathFinder.FindPath(new UPoint16(BottomLeftTileOfNPC - topLeftOfGrid), new UPoint16(endPoint - topLeftOfGrid));

        if (path is null) {
            return null;
        }

        PrunePath(path);
        PathfinderResult result = new(topLeftOfGrid, endPoint, default(PathNode), path);
        _cachedResults.Add(result);
        return result;
    }

    private void GenerateAndUseNewPath(Point endPoint) {
        if (_cachedResults.FirstOrDefault(result => result.endPoint == endPoint) is { } cachedResult) {
            _currentPathfinderResult = cachedResult;
            return;
        }

        _currentPathfinderResult = GetPathfinderResult(endPoint);

        if (_currentPathfinderResult is null) {
            EndPathfinding();
        }
        else {
            List<PathNode> path = _currentPathfinderResult.path;
            PrunePath(path);

            _prevDistanceToNextNode = GetDistanceToNode(path.Last());
            _notMakingProgressCounter = 0;
            npc.direction = npc.Center.X > (path.Last().NodePos.x + _currentPathfinderResult.topLeftOfGrid.X) * 16 + 8 ? -1 : 1;
            npc.velocity.X = npc.direction;
        }
    }

    private float GetDistanceToNode(PathNode nextNode) => npc.BottomLeft.Distance((_currentPathfinderResult.topLeftOfGrid + new Point(nextNode.NodePos.x, nextNode.NodePos.y)).ToWorldCoordinates());

    private static void PrunePath(IList<PathNode> path) {
        if (LWM.IsDebug && ModContent.GetInstance<DebugConfig>().disablePathPruning) {
            return;
        }

        List<int> indicesToBeRemoved = [];
        for (int i = path.Count - 2; i > 0; i--) {
            PathNode prevNode = path[i + 1];
            PathNode curNode = path[i];

            if (curNode.MovementType == NodeMovementType.PureHorizontal && prevNode.MovementType == NodeMovementType.PureHorizontal) {
                indicesToBeRemoved.Add(i);
            }
        }

        int removalCount = 0;
        for (int i = indicesToBeRemoved.Count - 1; i >= 0; i--) {
            path.RemoveAt(indicesToBeRemoved[i] - removalCount++);
        }
    }

    private void EndPathfinding() {
        _prevDistanceToNextNode = float.MaxValue;
        _pathRestartCount = _notMakingProgressCounter = 0;

        npc.velocity.X = 0f;
        _currentPathfinderResult = null;
    }
}