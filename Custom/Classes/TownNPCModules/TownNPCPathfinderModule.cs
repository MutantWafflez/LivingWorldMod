using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Custom.Utilities;
using LivingWorldMod.Library.AStarPathfinding;
using LivingWorldMod.Library.AStarPathfinding.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace LivingWorldMod.Custom.Classes.TownNPCModules;

public sealed class TownNPCPathfinderModule : TownNPCModule {
    private sealed class PathfinderResult {
        public readonly Point topLeftOfGrid;
        public readonly Point endPoint;
        public readonly List<PathFinderNode> path;
        public PathFinderNode lastConsumedNode;

        public PathfinderResult(Point topLeftOfGrid, Point endPoint, PathFinderNode lastConsumedNode, List<PathFinderNode> path) {
            this.topLeftOfGrid = topLeftOfGrid;
            this.endPoint = endPoint;
            this.lastConsumedNode = lastConsumedNode;
            this.path = path;
        }
    }

    public delegate void PathfindEndedCallback(bool reachedDestination);

    /// <summary>
    /// The maximum amount of tiles Town NPCs can jump from their bottom left. For example, a value of 6 means that a Town NPC
    /// can jump on top of a 6 tile tall pillar.
    /// </summary>
    public const int MaxTileJumpHeight = 6;

    /// <summary>
    /// The side length of the square that represents the zone in which the PathFinder will search for a path.
    /// </summary>
    public const int PathFinderZoneSideLength = 128;

    public bool IsPathfinding => _currentPathfinderResult is not null;

    public Point BottomLeftTileOfNPC => (npc.BottomLeft + new Vector2(0, -2)).ToTileCoordinates();

    public Point TopLeftOfPathfinderZone => BottomLeftTileOfNPC - new Point(PathFinderZoneSideLength / 2, PathFinderZoneSideLength / 2);

    public Rectangle PathfinderZone {
        get {
            Point tempTopLeftOfGrid = TopLeftOfPathfinderZone;
            return new Rectangle(tempTopLeftOfGrid.X, tempTopLeftOfGrid.Y, PathFinderZoneSideLength, PathFinderZoneSideLength);
        }
    }

    public bool canFallThroughPlatforms;
    private readonly List<PathfinderResult> _cachedResults;

    private float _prevDistanceToNextNode;
    private int _notMakingProgressCounter;
    private GroundedPathFinder _cachedPathfinder;
    private PathfinderResult _currentPathfinderResult;
    private PathfindEndedCallback _currentCallback;

    public TownNPCPathfinderModule(NPC npc) : base(npc) {
        _cachedResults = new List<PathfinderResult>();
    }

    public override void Update() {
        _cachedResults.Clear();
        _cachedPathfinder = null;
        if (!IsPathfinding) {
            return;
        }

        Point topLeftOfGrid = _currentPathfinderResult.topLeftOfGrid;
        List<PathFinderNode> path = _currentPathfinderResult.path;
        ref PathFinderNode lastConsumedNode = ref _currentPathfinderResult.lastConsumedNode;

        PathFinderNode nextNode = path.Last();

        // If the NPC does not make meaningful progress to the next node, regenerate the path
        float curDistanceToNextNode = GetDistanceToNode(nextNode);
        if (curDistanceToNextNode >= _prevDistanceToNextNode) {
            if (++_notMakingProgressCounter >= LWMUtils.RealLifeSecond / 2) {
                GenerateAndUseNewPath(_currentPathfinderResult.endPoint);

                return;
            }
        }
        else {
            _notMakingProgressCounter = 0;
        }
        _prevDistanceToNextNode = curDistanceToNextNode;

        Vector2 nextNodeCenter = (topLeftOfGrid + new Point(nextNode.X, nextNode.Y)).ToWorldCoordinates();
        Vector2 nextNodeBottom = nextNodeCenter + new Vector2(0f, 8f);

        Rectangle nodeRectangle = new((int)(nextNodeCenter.X - 8f), (int)(nextNodeCenter.Y - 8f), 16, 16);
        Rectangle npcNodeCollisionRectangle = new((int)npc.BottomLeft.X, (int)npc.BottomLeft.Y - 16, 16, 16);

        bool leftHasBreachedNode = npc.direction == 1 ? npc.Left.X >= nextNodeCenter.X : npc.Left.X <= nextNodeCenter.X;

        canFallThroughPlatforms = false;
        if (leftHasBreachedNode && nodeRectangle.Intersects(npcNodeCollisionRectangle) && (lastConsumedNode.Y - nextNode.Y < GroundedPathFinder.MinimumHeightToBeConsideredJump || npc.velocity.Y == 0f)) {
            lastConsumedNode = nextNode;
            path.RemoveAt(path.Count - 1);

            if (!path.Any()) {
                EndPathfinding(true);
                return;
            }

            nextNode = path.Last();
            nextNodeCenter = (topLeftOfGrid + new Point(nextNode.X, nextNode.Y)).ToWorldCoordinates();

            if (lastConsumedNode.Y - nextNode.Y >= GroundedPathFinder.MinimumHeightToBeConsideredJump) {
                npc.BottomLeft = nextNodeBottom;

                //Reset velocity & calculate jump vector required to reach jump destination
                npc.velocity *= 0f;

                float npcGravity = npc.gravity;
                float jumpHeight = npc.BottomLeft.Y - (topLeftOfGrid + new Point(nextNode.X, nextNode.Y)).ToWorldCoordinates(8f, 0f).Y;
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
        }

        //Step movements or horizontal movements
        if (Math.Abs(lastConsumedNode.Y - nextNode.Y) == GroundedPathFinder.ExactHeightToBeConsideredStep || lastConsumedNode.Y == nextNode.Y) {
            npc.velocity.X = npc.direction;
            CheckForDoors();

            if (lastConsumedNode.Y < nextNode.Y) {
                Collision.StepDown(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
            }
            else if (lastConsumedNode.Y > nextNode.Y) {
                Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
            }
        }
        //Fall movements
        else if (lastConsumedNode.Y - nextNode.Y < 0) {
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

            npc.velocity.Y = 0.1f;
            canFallThroughPlatforms = true;
        }
    }

    public bool RequestPathfind(Point location, PathfindEndedCallback endCallback) {
        if (IsPathfinding) {
            return false;
        }

        _currentCallback = endCallback;
        GenerateAndUseNewPath(location);
        return true;
    }

    public bool HasPath(Point location) => GetPathfinderResult(location) is not null;

    public void CancelPathfind() => EndPathfinding(false);

    public bool EntityWithinPathfinderZone(Entity entity) {
        Point npcTopLeft = npc.position.ToTileCoordinates();

        return RectangleWithinPathfinderZone(new Rectangle(npcTopLeft.X, npcTopLeft.Y, (int)Math.Ceiling((double)entity.width), (int)Math.Ceiling((double)entity.height)));
    }

    public bool RectangleWithinPathfinderZone(Rectangle rectangle) => PathfinderZone.Contains(rectangle);

    public void DebugDrawPath(SpriteBatch spriteBatch, Vector2 screenPos) {
        if (!IsPathfinding) {
            return;
        }

        foreach (PathFinderNode node in _currentPathfinderResult.path) {
            spriteBatch.Draw(TextureAssets.Extra[66].Value, _currentPathfinderResult.topLeftOfGrid.ToWorldCoordinates(2f, 2.5f) + new Vector2(node.X, node.Y).ToWorldCoordinates(0f, 0f) - screenPos, Color.White);
        }
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
        GroundedPathFinder pathFinder;
        if (_cachedPathfinder is not null) {
            pathFinder = _cachedPathfinder;
        }
        else {
            PathFinder.GridObject[,] grid = new PathFinder.GridObject[PathFinderZoneSideLength, PathFinderZoneSideLength];
            for (int i = 0; i < PathFinderZoneSideLength; i++) {
                for (int j = 0; j < PathFinderZoneSideLength; j++) {
                    Tile tile = Main.tile[topLeftOfGrid + new Point(i, j)];

                    bool hasTile = tile.HasTile;
                    bool isActuated = tile.IsActuated;
                    bool isSolid = Main.tileSolid[tile.TileType];
                    bool isPlatform = TileID.Sets.Platforms[tile.TileType];
                    bool isDoor = TileLoader.OpenDoorID(tile) > 0 || TileLoader.CloseDoorID(tile) > 0;
                    bool hasLava = tile is { LiquidAmount: > 0, LiquidType: LiquidID.Lava };
                    bool hasShimmer = tile is { LiquidAmount: >= 200, LiquidType: LiquidID.Shimmer };

                    grid[i, j] = hasTile switch {
                        true when !isActuated && isSolid && !isDoor && !isPlatform => new PathFinder.GridObject(GridNodeType.Solid, 0),
                        true when !isActuated && isPlatform => new PathFinder.GridObject(GridNodeType.SolidTop, 0),
                        false when hasLava || hasShimmer => new PathFinder.GridObject(GridNodeType.Impassable, 0),
                        _ => new PathFinder.GridObject(GridNodeType.NonSolid, GetTileMovementCost(tile))
                    };
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC otherNPC = Main.npc[i];
                if (!otherNPC.active || otherNPC.friendly || otherNPC.damage <= 0 || !EntityWithinPathfinderZone(otherNPC)) {
                    continue;
                }

                Point npcPathfinderPos = otherNPC.position.ToTileCoordinates() - topLeftOfGrid;
                for (int j = npcPathfinderPos.X; j < npcPathfinderPos.X + (int)Math.Ceiling(npc.width / 16f); j++) {
                    for (int k = npcPathfinderPos.Y; k < npcPathfinderPos.Y + (int)Math.Ceiling(npc.height / 16f); k++) {
                        if (grid[j, k].ObjectType == GridNodeType.NonSolid) {
                            grid[j, k] = new PathFinder.GridObject(GridNodeType.Impassable, 0);
                        }
                    }
                }
            }

            pathFinder = new GroundedPathFinder(grid);
        }

        List<PathFinderNode> path = pathFinder.FindPath(
            BottomLeftTileOfNPC - topLeftOfGrid,
            endPoint - topLeftOfGrid,
            MaxTileJumpHeight,
            (int)Math.Ceiling(npc.width / 16f),
            (int)Math.Ceiling(npc.height / 16f)
        );

        if (path is null) {
            return null;
        }

        PrunePath(path);
        PathfinderResult result = new(topLeftOfGrid, endPoint, default(PathFinderNode), path);
        _cachedResults.Add(result);
        return result;
    }

    private byte GetTileMovementCost(Tile tile) {
        byte tileCost = 1;

        if (TileID.Sets.AvoidedByNPCs[tile.TileType]) {
            tileCost += 8;
        }

        if (tile.LiquidAmount <= 0) {
            return tileCost;
        }

        switch (tile.LiquidType) {
            case LiquidID.Water:
                tileCost += 8;
                break;
            case LiquidID.Honey:
                tileCost += 12;
                break;
        }

        return tileCost;
    }

    private void GenerateAndUseNewPath(Point endPoint) {
        if (_cachedResults.FirstOrDefault(result => result.endPoint == endPoint) is { } cachedResult) {
            _currentPathfinderResult = cachedResult;
            return;
        }

        _currentPathfinderResult = GetPathfinderResult(endPoint);

        if (_currentPathfinderResult is null) {
            EndPathfinding(false);
        }
        else {
            List<PathFinderNode> path = _currentPathfinderResult.path;
            PrunePath(path);

            _prevDistanceToNextNode = GetDistanceToNode(path.Last());
            _notMakingProgressCounter = 0;
            npc.direction = npc.Center.X > (path.Last().X + _currentPathfinderResult.topLeftOfGrid.X) * 16 + 8 ? -1 : 1;
            npc.velocity.X = npc.direction;

            npc.netUpdate = true;
        }
    }

    private float GetDistanceToNode(PathFinderNode nextNode) => npc.BottomLeft.Distance((_currentPathfinderResult.topLeftOfGrid + new Point(nextNode.X, nextNode.Y)).ToWorldCoordinates());

    private static void PrunePath(IList<PathFinderNode> path) {
        List<int> indicesToBeRemoved = new();

        for (int i = path.Count - 2; i > 0; i--) {
            PathFinderNode prevNode = path[i + 1];
            PathFinderNode curNode = path[i];
            PathFinderNode nextNode = path[i - 1];

            if (curNode.Y == prevNode.Y && curNode.Y == nextNode.Y) {
                indicesToBeRemoved.Add(i);
            }
        }

        int removalCount = 0;
        for (int i = indicesToBeRemoved.Count - 1; i >= 0; i--) {
            path.RemoveAt(indicesToBeRemoved[i] - removalCount++);
        }
    }

    private void EndPathfinding(bool reachedDestination) {
        _prevDistanceToNextNode = float.MaxValue;
        _notMakingProgressCounter = 0;

        npc.velocity.X = 0f;
        _currentPathfinderResult = null;
        _currentCallback?.Invoke(reachedDestination);
        _currentCallback = null;
    }
}