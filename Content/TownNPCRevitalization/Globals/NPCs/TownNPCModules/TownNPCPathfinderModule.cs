using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;
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

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;

public sealed class TownNPCPathfinderModule : TownNPCModule {
    private sealed class PathfinderResult(Point topLeftOfGrid, Point endPoint, PathNode lastConsumedNode, List<PathNode> path) {
        public readonly Point topLeftOfGrid = topLeftOfGrid;
        public readonly Point endPoint = endPoint;
        public readonly List<PathNode> path = path;
        public PathNode lastConsumedNode = lastConsumedNode;
    }

    private record struct OpenedDoorData(Point16 TilePos, Rectangle DoorHitBox, bool IsGate = false);

    private const int MaxPathRecyclesBeforeFailure = 5;

    private readonly List<PathfinderResult> _cachedResults = [];

    /// <summary>
    ///     The position of doors that this NPC has opened and need to be closed.
    /// </summary>
    private readonly List<OpenedDoorData> _openDoors = [];

    private float _prevDistanceToNextNode;
    private int _notMakingProgressCounter;
    private int _pathRestartCount;

    private bool _isPaused;
    private bool _wasPaused;

    private TownNPCPathfinder _cachedPathfinder;
    private PathfinderResult _currentPathfinderResult;

    public static int PathfinderSize => ModContent.GetInstance<TownNPCConfig>().pathfinderSize;

    public override int UpdatePriority => 3;

    public bool IsPathfinding => _currentPathfinderResult is not null;

    public Point BottomLeftTileOfNPC => (NPC.BottomLeft + new Vector2(0f, -2f)).ToTileCoordinates();

    public Point TopLeftOfPathfinderZone => BottomLeftTileOfNPC - new Point(PathfinderSize / 2, PathfinderSize / 2);

    private static void PrunePath(List<PathNode> path) {
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

    public override void UpdateModule() {
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
            NPC.velocity.X = 0f;
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

        NPC.GravityIgnoresLiquid = lastConsumedNode.MovementType == NodeMovementType.Jump || nextNode.MovementType == NodeMovementType.Jump;
        _prevDistanceToNextNode = curDistanceToNextNode;

        Vector2 nextNodeCenter = (topLeftOfGrid + new Point(nextNode.NodePos.x, nextNode.NodePos.y)).ToWorldCoordinates();
        Vector2 nextNodeBottom = nextNodeCenter + new Vector2(0f, 8f);

        Rectangle nodeRectangle = new((int)(nextNodeCenter.X - 8f), (int)(nextNodeCenter.Y - 8f), 16, 16);
        Rectangle npcNodeCollisionRectangle = new((int)NPC.BottomLeft.X, (int)NPC.BottomLeft.Y - 16, 16, 16);

        bool leftHasBreachedNode = NPC.direction == 1 ? NPC.Left.X >= nextNodeCenter.X : NPC.Left.X <= nextNodeCenter.X;

        TownNPCCollisionModule collisionModule = NPC.GetGlobalNPC<TownNPCCollisionModule>();
        if (leftHasBreachedNode && nodeRectangle.Intersects(npcNodeCollisionRectangle) && (lastConsumedNode.MovementType is not NodeMovementType.Jump || NPC.velocity.Y == 0f)) {
            lastConsumedNode = nextNode;
            path.RemoveAt(path.Count - 1);

            if (path.Count == 0) {
                EndPathfinding();
                return;
            }

            nextNode = path.Last();
            nextNodeCenter = (topLeftOfGrid + new Point(nextNode.NodePos.x, nextNode.NodePos.y)).ToWorldCoordinates();

            // TODO: Figure out solution for 5+ block jumps from the left not clearing (sometimes??? can't reproduce)
            if (lastConsumedNode.MovementType is NodeMovementType.Jump) {
                collisionModule.ignoreLiquidVelocityModifications = true;
                collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = false;
                NPC.BottomLeft = nextNodeBottom;

                //Reset velocity & calculate jump vector required to reach jump destination
                NPC.velocity *= 0f;

                float npcGravity = NPC.gravity;
                float jumpHeight = NPC.Bottom.Y - (topLeftOfGrid + new Point(nextNode.NodePos.x, nextNode.NodePos.y)).ToWorldCoordinates(0f).Y;
                float yDisplacement = jumpHeight - NPC.height;

                // Horizontal Velocity = X Displacement / (sqrt(-2 * h / g) + sqrt(2(Y Displacement - h) / g))
                // npc ^ formula assumes gravity is negative; it is not because positive Y is down in Terraria. Thus, removed some of the negatives
                NPC.velocity.X = (float)((nextNodeCenter.X - NPC.Left.X) / (Math.Sqrt(2 * jumpHeight / npcGravity) + Math.Sqrt(2 * (jumpHeight - yDisplacement) / npcGravity)));

                // Vertical velocity = sqrt(-2gh)
                // Entire formula negated since, again, positive Y is down
                NPC.velocity.Y = (float)-Math.Sqrt(2 * npcGravity * jumpHeight);
            }

            leftHasBreachedNode = false;
            NPC.direction = NPC.Left.X > nextNodeCenter.X ? -1 : 1;
            collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = collisionModule.walkThroughStairs = false;
        }

        switch (lastConsumedNode.MovementType) {
            //Step movements or horizontal movements
            case NodeMovementType.StepUp:
                NPC.velocity.X = NPC.direction;
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

                collisionModule.ignoreLiquidVelocityModifications = collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = collisionModule.walkThroughStairs = false;
                CheckForDoors();
                break;
            case NodeMovementType.StepDown:
                NPC.velocity.X = NPC.direction;

                collisionModule.ignoreLiquidVelocityModifications = collisionModule.fallThroughStairs = collisionModule.walkThroughStairs = collisionModule.fallThroughPlatforms = false;
                CheckForDoors();
                break;
            default:
            case NodeMovementType.PureHorizontal:
                NPC.velocity.X = NPC.direction;
                CheckForDoors();

                collisionModule.ignoreLiquidVelocityModifications = collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = false;
                collisionModule.walkThroughStairs = true;
                break;
            case NodeMovementType.Fall:
                collisionModule.ignoreLiquidVelocityModifications = collisionModule.walkThroughStairs = false;
                if (NPC.velocity.Y == 0f) {
                    NPC.velocity.X = NPC.direction;
                }

                if (!leftHasBreachedNode) {
                    return;
                }

                NPC.velocity.X = 0f;
                if (NPC.velocity.Y != 0f) {
                    return;
                }

                collisionModule.fallThroughPlatforms = collisionModule.fallThroughStairs = true;
                break;
            case NodeMovementType.Jump:
                if (NPC.velocity.Y >= 0f) {
                    NPC.velocity.X = NPC.direction;
                }

                break;
        }
    }

    public override void PostDraw(NPC _, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!IsPathfinding || !LWM.IsDebug) {
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

            spriteBatch.Draw(
                TextureAssets.Extra[66].Value,
                _currentPathfinderResult.topLeftOfGrid.ToWorldCoordinates(2f, 2.5f) + new Vector2(node.NodePos.x, node.NodePos.y).ToWorldCoordinates(0f, 0f) - screenPos,
                nodeColor
            );
        }
    }

    public override void SendExtraAI(NPC _, BitWriter bitWriter, BinaryWriter binaryWriter) {
        Point16 endPoint = new(_currentPathfinderResult?.endPoint ?? new Point(-1, -1));
        binaryWriter.Write(endPoint.X);
        binaryWriter.Write(endPoint.Y);
    }

    public override void ReceiveExtraAI(NPC _, BitReader bitReader, BinaryReader binaryReader) {
        Point endPoint = new(binaryReader.ReadInt16(), binaryReader.ReadInt16());
        EndPathfinding();
        if (endPoint is { X: -1, Y: -1 }) {
            return;
        }

        GenerateAndUseNewPath(endPoint);
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

    private void CheckForDoors() {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        Point16 doorCheckPos = NPC.position.ToTileCoordinates16() + new Point16(NPC.direction == -1 ? -1 : (int)Math.Ceiling(NPC.width / 16f), 0);

        // Go through all doors that are currently open, and if the NPC isn't currently colliding with their hitbox, close them
        for (int i = 0; i < _openDoors.Count; i++) {
            OpenedDoorData openDoor = _openDoors[i];
            if ((NPC.Hitbox with { X = NPC.Hitbox.X + 16 * NPC.direction }).Intersects(openDoor.DoorHitBox)) {
                continue;
            }

            if (openDoor.IsGate && WorldGen.ShiftTallGate(openDoor.TilePos.X, openDoor.TilePos.Y, true)) {
                NetMessage.SendData(MessageID.ToggleDoorState, number: 5, number2: openDoor.TilePos.X, number3: openDoor.DoorHitBox.Y);
            }
            else if (WorldGen.CloseDoor(openDoor.TilePos.X, openDoor.TilePos.Y)) {
                NetMessage.SendData(MessageID.ToggleDoorState, number: 1, number2: openDoor.TilePos.X, number3: openDoor.DoorHitBox.Y, number4: NPC.direction);
            }

            _openDoors.RemoveAt(i--);
        }

        Tile doorTile = Main.tile[doorCheckPos];
        if (!doorTile.HasUnactuatedTile || (!TileLoader.IsClosedDoor(doorTile) && doorTile.TileType != TileID.TallGateClosed)) {
            return;
        }

        for (int i = NPC.direction; i != -NPC.direction; i = -i) {
            if (!WorldGen.OpenDoor(doorCheckPos.X, doorCheckPos.Y, i)) {
                continue;
            }

            AddOpenDoor(false);
            NetMessage.SendData(MessageID.ToggleDoorState, number2: doorCheckPos.X, number3: doorCheckPos.Y, number4: i);
            break;
        }

        if (!WorldGen.ShiftTallGate(doorCheckPos.X, doorCheckPos.Y, false)) {
            return;
        }

        AddOpenDoor(true);
        NetMessage.SendData(MessageID.ToggleDoorState, number: 4, number2: doorCheckPos.X, number3: doorCheckPos.Y);

        return;

        void AddOpenDoor(bool isGate) {
            Rectangle doorHitBox = LWMUtils.GetTileHitBox(
                    doorTile,
                    LWMUtils.GetCornerOfMultiTile(doorTile, doorCheckPos.X, doorCheckPos.Y, LWMUtils.CornerType.TopLeft).ToPoint16()
                )
                .ToWorldCoordinates();

            // Add additional "fluff" around edges of the doors so NPCs don't try to close the door while still inside it
            _openDoors.Add(new OpenedDoorData(doorCheckPos, doorHitBox, isGate));
        }
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
            ?? new TownNPCPathfinder(new UPoint16(topLeftOfGrid.X, topLeftOfGrid.Y), (ushort)PathfinderSize, (ushort)Math.Ceiling(NPC.width / 16f), (ushort)Math.Ceiling(NPC.height / 16f));

        Point adjustedBottomLeftOfNPC = BottomLeftTileOfNPC;
        Tile bottomLeftTileOfNPC = Main.tile[adjustedBottomLeftOfNPC];
        if (bottomLeftTileOfNPC.HasTile && (bottomLeftTileOfNPC.IsHalfBlock || bottomLeftTileOfNPC.Slope > SlopeType.Solid)) {
            adjustedBottomLeftOfNPC.Y--;
        }

        List<PathNode> path = pathFinder.FindPath(new UPoint16(adjustedBottomLeftOfNPC - topLeftOfGrid), new UPoint16(endPoint - topLeftOfGrid));

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
            NPC.direction = NPC.Center.X > (path.Last().NodePos.x + _currentPathfinderResult.topLeftOfGrid.X) * 16 + 8 ? -1 : 1;
            NPC.velocity.X = NPC.direction;
        }
    }

    private float GetDistanceToNode(PathNode nextNode) => NPC.BottomLeft.Distance((_currentPathfinderResult.topLeftOfGrid + new Point(nextNode.NodePos.x, nextNode.NodePos.y)).ToWorldCoordinates());

    private void EndPathfinding() {
        _prevDistanceToNextNode = float.MaxValue;
        _pathRestartCount = _notMakingProgressCounter = 0;
        NPC.GravityIgnoresLiquid = false;

        NPC.velocity.X = 0f;
        _currentPathfinderResult = null;
    }
}