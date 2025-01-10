﻿using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.Utilities;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class WalkToRandomPosState : TownNPCAIState {
    public override int ReservedStateInteger => 1;

    public override void DoState( NPC npc) {
        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        if (npc.ai[2] == 0f) {
            int maxTileThreshold = TownNPCPathfinderModule.PathfinderSize / 4;
            int minTileThreshold = Math.ILogB(TownNPCPathfinderModule.PathfinderSize);

            WeightedRandom<Point> wanderPoints = new();
            Vector2 homePos = new(npc.homeTileX, npc.homeTileY);
            for (int i = 0; i < 360; i += 15) {
                Point displacement = new Vector2(0, -Main.rand.Next(minTileThreshold, maxTileThreshold)).RotatedBy(MathHelper.ToRadians(i)).ToPoint();
                if (LWMUtils.DropUntilCondition(
                        ValidWanderPoint,
                        pathfinderModule.BottomLeftTileOfNPC(npc) + displacement,
                        maxTileThreshold + 1
                    ) is not { } point
                    || !pathfinderModule.HasPath(npc, point + new Point(0, -1))
                ) {
                    continue;
                }

                Point wanderPoint = point + new Point(0, -1);
                float distanceFromHome = homePos.Distance(wanderPoint.ToVector2());
                wanderPoints.Add(wanderPoint, distanceFromHome == 0f ? 1f : 1 / distanceFromHome);
            }

            if (wanderPoints.elements.Count == 0) {
                TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
                npc.ai[1] = Main.rand.Next(LWMUtils.RealLifeSecond * 2, LWMUtils.RealLifeSecond * 5);
                return;
            }

            pathfinderModule.RequestPathfind(npc, wanderPoints);
            npc.ai[2] = 1f;
            npc.netUpdate = true;
        }
        else if (npc.ai[2] == 1f && !pathfinderModule.IsPathfinding) {
            TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
            npc.ai[1] = LWMUtils.RealLifeSecond * 3;
        }

        return;

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