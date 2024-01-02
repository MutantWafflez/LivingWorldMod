using System;
using System.Linq;
using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Custom.Classes;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;

namespace LivingWorldMod.Content.TownNPCAIStates {
    public class WanderAIState : TownNPCAIState {
        public override int ReservedStateInteger => 1;

        public override void DoState(TownAIGlobalNPC globalNPC, NPC npc) {
            TownNPCPathfinderModule pathfinderModule = globalNPC.PathfinderModule;
            if (npc.ai[2] == 0f) {
                const int maxTileThreshold = TownNPCPathfinderModule.PathFinderZoneSideLength / 4;
                const int minTileThreshold = 8;

                WeightedRandom<Point> wanderPoints = new();
                Vector2 homePos = new(npc.homeTileX, npc.homeTileY);
                for (int i = 0; i < 360; i += 15) {
                    Vector2 displacement = new Vector2(0, -Main.rand.Next(minTileThreshold, maxTileThreshold) * 16).RotatedBy(MathHelper.ToRadians(i));
                    if (TileUtils.DropUntilCondition(
                            ValidWanderPoint,
                            pathfinderModule.BottomLeftTileOfNPC + displacement.ToTileCoordinates(),
                            maxTileThreshold + 1) is not { } point
                        || !pathfinderModule.HasPath(point + new Point(0, -1))
                       ) {
                        continue;
                    }

                    Point wanderPoint = point + new Point(0, -1);
                    float distanceFromHome = homePos.Distance(wanderPoint.ToVector2());
                    wanderPoints.Add(wanderPoint, distanceFromHome == 0f ? 1f : 1 / distanceFromHome);
                }

                if (!wanderPoints.elements.Any()) {
                    TownAIGlobalNPC.RefreshToState<DefaultAIState>(npc);
                    npc.ai[1] = UnitUtils.RealLifeSecond * 3;
                    return;
                }

                pathfinderModule.RequestPathfind(wanderPoints, null);
                npc.ai[2] = 1f;
                npc.netUpdate = true;
            }
            else if (npc.ai[2] == 1f && !pathfinderModule.IsPathfinding) {
                TownAIGlobalNPC.RefreshToState<DefaultAIState>(npc);
                npc.ai[1] = UnitUtils.RealLifeSecond * 3;
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
                    if (upTile.HasUnactuatedTile && Main.tileSolid[upTile.TileType] || i == 0 && upTile is { LiquidType: LiquidID.Water, LiquidAmount: > 0 }) {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}