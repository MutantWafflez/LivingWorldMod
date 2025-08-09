using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Items;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class DogFetchAIState : TownNPCAIState {
    private readonly record struct DogStandPoints(Point LeftPoint, Point RightPoint);

    private const int SubStateNone = 0;

    private const int StateNavigatingToPlayerPreThrow = 0;

    private const int SubStateAttemptingNavigationToPlayer = 0;
    private const int SubStateWalkingToPlayer = 1;

    private const int StateWaitingForPlayerToThrow = 1;
    private const int StateWaitingForProjectileToSettle = 2;
    private const int StateFetching = 3;
    private const int StateReachedProjectile = 4;
    private const int StateWalkingToPlayerPostFetch = 5;
    private const int StateWaitingForPlayerToThrowAgainBuffer = 6;

    private static DogStandPoints GetDogStandPoints(Player targetPlayer) {
        Point playerBottomLeft = (targetPlayer.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates();

        return new DogStandPoints(playerBottomLeft + new Point(-2, 0), playerBottomLeft + new Point((int)Math.Ceiling(targetPlayer.width / 16f) + 1, 0));
    }

    public override void DoState(NPC npc) {
        ref float stateValue = ref npc.ai[1];

        TownDogModule dogModule = npc.GetGlobalNPC<TownDogModule>();
        Player targetPlayer = dogModule.fetchPlayer;
        Projectile targetProjectile = dogModule.fetchProj;

        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        switch ((int)stateValue) {
            case StateNavigatingToPlayerPreThrow: {
                ref float subStateNavigating = ref npc.ai[2];

                DogStandPoints standPoints = GetDogStandPoints(targetPlayer);
                switch ((int)subStateNavigating) {
                    case SubStateAttemptingNavigationToPlayer: {
                        ref float retryTimer = ref npc.ai[3];

                        if (retryTimer-- > 0) {
                            break;
                        }

                        retryTimer = 0f;

                        bool isFacingLeft = targetPlayer.IsFacingLeft();
                        bool hasLeftPath = pathfinderModule.HasPath(standPoints.LeftPoint);
                        bool hasRightPath = pathfinderModule.HasPath(standPoints.RightPoint);

                        if ((isFacingLeft && hasLeftPath) || (!hasRightPath && hasLeftPath)) {
                            pathfinderModule.RequestPathfind(standPoints.LeftPoint);

                            subStateNavigating = SubStateWalkingToPlayer;
                            break;
                        }

                        if ((!isFacingLeft && hasRightPath) || hasRightPath) {
                            pathfinderModule.RequestPathfind(standPoints.RightPoint);

                            subStateNavigating = SubStateWalkingToPlayer;
                            break;
                        }

                        retryTimer = LWMUtils.RealLifeSecond * 4;

                        break;
                    }
                    case SubStateWalkingToPlayer: {
                        if (pathfinderModule.IsPathfinding) {
                            break;
                        }

                        Point bottomLeftTileOfNPC = pathfinderModule.BottomLeftTileOfNPC;
                        if (bottomLeftTileOfNPC != standPoints.LeftPoint && bottomLeftTileOfNPC != standPoints.RightPoint) {
                            subStateNavigating = SubStateAttemptingNavigationToPlayer;

                            break;
                        }


                        npc.direction = Math.Sign(npc.DirectionTo(targetPlayer.Center).X);

                        stateValue = StateWaitingForPlayerToThrow;
                        subStateNavigating = SubStateNone;
                        break;
                    }
                }

                break;
            }
            case StateWaitingForPlayerToThrow: {
                Projectile foundProjectile = null;
                foreach (Projectile proj in Main.ActiveProjectiles) {
                    if (proj.type != ModContent.ProjectileType<FetchingStickProj>() || proj.owner != targetPlayer.whoAmI) {
                        continue;
                    }

                    foundProjectile = proj;
                }

                if (foundProjectile is null) {
                    break;
                }

                dogModule.fetchProj = foundProjectile;
                stateValue = StateWaitingForProjectileToSettle;

                break;
            }
            case StateWaitingForProjectileToSettle: {
                npc.direction = Math.Sign(npc.DirectionTo(targetProjectile.Center).X);
                if (targetProjectile.ai[0] != FetchingStickProj.AtRestState) {
                    break;
                }

                stateValue = StateFetching;

                break;
            }
            case StateFetching: {
                Point fetchPoint = (targetProjectile.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates();

                pathfinderModule.HorizontalSpeed = 3f;
                pathfinderModule.RequestPathfind(fetchPoint);

                if (pathfinderModule.BottomLeftTileOfNPC == fetchPoint) {
                    stateValue = StateReachedProjectile;
                }

                break;
            }
            case StateReachedProjectile: {
                if (npc.ai[2]++ >= LWMUtils.RealLifeSecond) {
                    stateValue = StateWalkingToPlayerPostFetch;
                    npc.ai[2] = 0f;

                    targetProjectile.ai[0] = FetchingStickProj.PickedUpByDog;
                }

                break;
            }
            case StateWalkingToPlayerPostFetch: {
                Point dogStandPoint = (targetPlayer.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates()
                    + new Point(targetPlayer.direction * 2 + (targetPlayer.direction == 1 ? (int)Math.Ceiling(targetPlayer.width / 16f) - 1 : 0), 0);

                pathfinderModule.HorizontalSpeed = 1.5f;
                pathfinderModule.RequestPathfind(dogStandPoint);

                targetProjectile.direction = npc.direction;
                targetProjectile.Center = npc.Top + new Vector2(npc.Size.X * npc.direction, 4f);

                if (pathfinderModule.BottomLeftTileOfNPC == dogStandPoint) {
                    npc.ai[1] = StateWaitingForPlayerToThrowAgainBuffer;

                    dogModule.fetchProj = null;
                    targetProjectile.Kill();

                    targetPlayer.QuickSpawnItem(new EntitySource_Gift(npc), ModContent.ItemType<FetchingStick>());
                }

                break;
            }
            case StateWaitingForPlayerToThrowAgainBuffer: {
                if (npc.ai[2]++ < LWMUtils.RealLifeSecond * 3f) {
                    break;
                }

                if (targetPlayer.HeldItem.type != ModContent.ItemType<FetchingStick>()) {
                    TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
                    break;
                }

                stateValue = StateWaitingForPlayerToThrow;
                npc.ai[2] = 0f;

                break;
            }
        }
    }
}