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

    /// <summary>
    ///     Generic handler for an AI state that has the two "navigation" substates. Returns false when the navigation is not completed yet, and returns true when navigation is complete and is going
    ///     to transition to the next STATE (not sub-state).
    /// </summary>
    private static bool HandleNavigationToPlayerSubStates(NPC npc, Player targetPlayer, TownNPCPathfinderModule pathfinderModule, int nextSuccessfulState, float dogMoveSpeed) {
        ref float stateValue = ref npc.ai[1];
        ref float subStateNavigating = ref npc.ai[2];
        ref float retryTimer = ref npc.ai[3];

        DogStandPoints standPoints = GetDogStandPoints(targetPlayer);
        switch ((int)subStateNavigating) {
            case SubStateAttemptingNavigationToPlayer: {
                if (retryTimer-- > 0) {
                    return false;
                }

                retryTimer = 0f;

                bool isFacingLeft = targetPlayer.IsFacingLeft();
                bool hasLeftPath = pathfinderModule.HasPath(standPoints.LeftPoint);
                bool hasRightPath = pathfinderModule.HasPath(standPoints.RightPoint);

                if ((isFacingLeft && hasLeftPath) || (!hasRightPath && hasLeftPath)) {
                    pathfinderModule.RequestPathfind(standPoints.LeftPoint);

                    subStateNavigating = SubStateWalkingToPlayer;
                    return false;
                }

                if ((!isFacingLeft && hasRightPath) || hasRightPath) {
                    pathfinderModule.RequestPathfind(standPoints.RightPoint);

                    subStateNavigating = SubStateWalkingToPlayer;
                    return false;
                }

                retryTimer = LWMUtils.RealLifeSecond * 4;

                break;
            }
            case SubStateWalkingToPlayer: {
                pathfinderModule.HorizontalSpeed = dogMoveSpeed;
                if (pathfinderModule.IsPathfinding) {
                    return false;
                }

                Point bottomLeftTileOfNPC = pathfinderModule.BottomLeftTileOfNPC;
                if (bottomLeftTileOfNPC != standPoints.LeftPoint && bottomLeftTileOfNPC != standPoints.RightPoint) {
                    subStateNavigating = SubStateAttemptingNavigationToPlayer;

                    return false;
                }

                npc.direction = Math.Sign(npc.DirectionTo(targetPlayer.Center).X);

                stateValue = nextSuccessfulState;
                subStateNavigating = SubStateNone;
                return true;
            }
        }

        return false;
    }

    public override void DoState(NPC npc) {
        ref float stateValue = ref npc.ai[1];
        ref float subStateValue = ref npc.ai[2];
        ref float genericTimer = ref npc.ai[3];

        TownDogModule dogModule = npc.GetGlobalNPC<TownDogModule>();
        Player targetPlayer = dogModule.fetchPlayer;
        Projectile targetProjectile = dogModule.fetchProj;

        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        switch ((int)stateValue) {
            case StateNavigatingToPlayerPreThrow: {
                HandleNavigationToPlayerSubStates(npc, targetPlayer, pathfinderModule, StateWaitingForPlayerToThrow, 2f);

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
                if (genericTimer++ >= LWMUtils.RealLifeSecond) {
                    stateValue = StateWalkingToPlayerPostFetch;
                    genericTimer = 0f;

                    targetProjectile.ai[0] = FetchingStickProj.PickedUpByDog;
                }

                break;
            }
            case StateWalkingToPlayerPostFetch: {
                targetProjectile.direction = npc.direction;
                targetProjectile.Center = npc.Top + new Vector2(npc.Size.X * npc.direction, 4f);

                if (!HandleNavigationToPlayerSubStates(npc, targetPlayer, pathfinderModule, StateWaitingForPlayerToThrowAgainBuffer, 1.5f)) {
                    break;
                }

                dogModule.fetchProj = null;
                targetProjectile.Kill();

                targetPlayer.QuickSpawnItem(new EntitySource_Gift(npc), ModContent.ItemType<FetchingStick>());

                break;
            }
            case StateWaitingForPlayerToThrowAgainBuffer: {
                if (genericTimer++ < LWMUtils.RealLifeSecond * 3f) {
                    break;
                }

                if (targetPlayer.HeldItem.type != ModContent.ItemType<FetchingStick>()) {
                    TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
                    break;
                }

                stateValue = StateWaitingForPlayerToThrow;
                genericTimer = 0f;

                break;
            }
        }
    }
}