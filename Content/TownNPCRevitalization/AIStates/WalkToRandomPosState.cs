using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.DataStructures.Records;
using Microsoft.Xna.Framework;
using Terraria.Utilities;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class WalkToRandomPosState : TownNPCAIState {
    public override int ReservedStateInteger => 1;

    public override void DoState(NPC npc) {
        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        if (npc.ai[2] == 0f) {
            Point2D<int> wanderPoint = GetWanderPointFallback(npc);

            if (wanderPoint == Point2D<int>.NegativeOne) {
                TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
                npc.ai[1] = Main.rand.Next(LWMUtils.RealLifeSecond * 2, LWMUtils.RealLifeSecond * 5);
                return;
            }

            pathfinderModule.RequestPathfind((Point)wanderPoint);
            npc.ai[2] = 1f;
            npc.netUpdate = true;
        }
        else if (npc.ai[2] == 1f && !pathfinderModule.IsPathfinding) {
            TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
            npc.ai[1] = LWMUtils.RealLifeSecond * 3;
        }
    }

    /// <summary>
    ///     The "old" or fallback method of acquiring a point for this NPC to wander to. This is only done in the scenario where there is no PoI in the pathfinder grid.
    /// </summary>
    private Point2D<int> GetWanderPointFallback(NPC npc) {
        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        int maxTileThreshold = TownNPCPathfinderModule.DefaultPathfinderSize / 4;
        int minTileThreshold = Math.ILogB(TownNPCPathfinderModule.DefaultPathfinderSize);

        WeightedRandom<Point2D<int>> wanderPoints = new();
        Vector2 homePos = new(npc.homeTileX, npc.homeTileY);
        for (int i = 0; i < 360; i += 15) {
            Point displacement = new Vector2(0, -Main.rand.Next(minTileThreshold, maxTileThreshold)).RotatedBy(MathHelper.ToRadians(i)).ToPoint();
            if (LWMUtils.DropUntilCondition(
                    ValidWanderPoint,
                    pathfinderModule.BottomLeftTileOfNPC + displacement,
                    maxTileThreshold + 1
                ) is not { } point
                || !pathfinderModule.HasPath(point + new Point(0, -1))
            ) {
                continue;
            }

            Point wanderPoint = point + new Point(0, -1);
            float distanceFromHome = homePos.Distance(wanderPoint.ToVector2());
            wanderPoints.Add((Point2D<int>)wanderPoint, distanceFromHome == 0f ? 1f : 1 / distanceFromHome);
        }

        if (wanderPoints.elements.Count == 0) {
            return Point2D<int>.NegativeOne;
        }

        return wanderPoints;

        bool ValidWanderPoint(Point point) {
            Tile tile = Main.tile[point];
            if (!(tile.HasUnactuatedTile && (Main.tileSolidTop[tile.TileType] || Main.tileSolid[tile.TileType]))) {
                return false;
            }

            int npcTileHeight = (int)Math.Ceiling(npc.height / 16f);
            for (int i = 0; i < npcTileHeight; i++) {
                point.Y--;
                Tile upTile = Main.tile[point];
                if ((upTile.HasUnactuatedTile && Main.tileSolid[upTile.TileType]) || (i == 0 && upTile is { LiquidType: LiquidID.Water, LiquidAmount: > 127 })) {
                    return false;
                }
            }

            return true;
        }
    }
}