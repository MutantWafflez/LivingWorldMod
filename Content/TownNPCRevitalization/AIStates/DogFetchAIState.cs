using System;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs.Animations;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Items;

using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class DogFetchAIState : TownNPCAIState {
    private readonly record struct DogStandPoints(Point LeftPoint, Point RightPoint);

    private const int MaxTileDistanceToPlayFetch = LWMUtils.TilePixelsSideLength * 40;
    private const int MaxDogPatienceBeforeThrow = LWMUtils.RealLifeSecond * 15;

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

    // Animation info
    private const int DefaultFrame = 0;

    private const int GoingDownHalfKneeledFrame = 17;
    private const int GoingUpHalfKneeledFrame = 22;
    private const int HalfKneeledFrameDuration = 12;

    private const int KneeledTailWagStartFrame = 18;
    private const int KneeledTailWagEndFrame = 21;
    private const int KneeledTailWagFrameDuration = 4;

    private const int PickUpStickFromGroundAnimationFrameCount = 4;

    private static readonly LinearAnimation KneelingDownAnimation = new([GoingDownHalfKneeledFrame], [HalfKneeledFrameDuration]);
    private static readonly LinearAnimation StandingUpFromKneelAnimation = new([GoingUpHalfKneeledFrame], [HalfKneeledFrameDuration]);

    /// <summary>
    ///     Represents linear animation where a Town Dog will kneel down and immediately stand back up.
    /// </summary>
    private static readonly LinearAnimation PickUpStickFromGroundAnimation = new(
        [DefaultFrame, GoingDownHalfKneeledFrame, KneeledTailWagStartFrame, GoingUpHalfKneeledFrame],
        Enumerable.Repeat(HalfKneeledFrameDuration, PickUpStickFromGroundAnimationFrameCount).ToArray(),
        -2
    );

    /// <summary>
    ///     Represents animation where a Town Dog will kneel down on their front legs and wag their tail on loop until <see cref="LoopingKneeledTailWagAnimationFinished" /> returns true, where
    ///     the dog will then stand back up.
    /// </summary>
    private static readonly ChainedAnimation EasedKneeledTailWaggingAnimation = new (
        0,
        KneelingDownAnimation,
        new LoopingAnimation(KneeledTailWagStartFrame, KneeledTailWagEndFrame, KneeledTailWagFrameDuration, LoopingKneeledTailWagAnimationFinished),
        StandingUpFromKneelAnimation
    );

    public static bool PlayerIsValidToPlayFetchWith(Player player, NPC npc) => player.HeldItem.type == ModContent.ItemType<FetchingStick>()
        && PlayerWithinValidFetchingDistance(player, npc)
        && Collision.CanHitLine(npc.Center, 2, 2, player.Center, 2, 2);

    private static bool LoopingKneeledTailWagAnimationFinished(in NPC npc) => (int)npc.ai[1] != StateWaitingForPlayerToThrow;

    private static bool PlayerWithinValidFetchingDistance(Player player, NPC npc) => npc.Distance(player.Center) <= MaxTileDistanceToPlayFetch * MaxTileDistanceToPlayFetch;

    private static DogStandPoints GetDogStandPoints(Player targetPlayer) {
        Point playerBottomLeft = (targetPlayer.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates();

        return new DogStandPoints(playerBottomLeft + new Point(-2, 0), playerBottomLeft + new Point((int)Math.Ceiling(targetPlayer.width / 16f) + 1, 0));
    }

    /// <summary>
    ///     Generic handler for an AI state that has the two "navigation" substates. Returns false when the navigation is not completed yet, and returns true when navigation is complete and is going
    ///     to transition to the next STATE (not sub-state).
    /// </summary>
    private static bool HandleNavigationToPlayerSubStates(
        NPC npc,
        Player targetPlayer,
        TownNPCPathfinderModule pathfinderModule,
        int nextSuccessfulState,
        float dogMoveSpeed,
        Func<Player, NPC, bool> canContinueFetchFunc
    ) {
        ref float stateValue = ref npc.ai[1];
        ref float subStateNavigating = ref npc.ai[2];
        ref float retryTimer = ref npc.ai[3];

        DogStandPoints standPoints = GetDogStandPoints(targetPlayer);
        switch ((int)subStateNavigating) {
            case SubStateAttemptingNavigationToPlayer: {
                if (retryTimer-- > 0) {
                    return false;
                }

                if (!canContinueFetchFunc(targetPlayer, npc)) {
                    CancelState(npc, pathfinderModule, npc.GetGlobalNPC<TownDogModule>());

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

    /// <summary>
    ///     Cancels this entire state back to <see cref="DefaultAIState" />, canceling pathfinding, and setting <see cref="TownDogModule" /> related fields to their defaults.
    /// </summary>
    private static void CancelState(NPC npc, TownNPCPathfinderModule pathfinderModule, TownDogModule dogModule) {
        TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
        pathfinderModule.CancelPathfind();

        if (dogModule.fetchProj is not null && dogModule.fetchProj.active) {
            dogModule.fetchProj.timeLeft = LWMUtils.RealLifeSecond;
        }

        dogModule.fetchPlayer = null;
        dogModule.fetchProj = null;
    }

    public override void DoState(NPC npc) {
        ref float stateValue = ref npc.ai[1];
        ref float genericTimer = ref npc.ai[3];

        TownDogModule dogModule = npc.GetGlobalNPC<TownDogModule>();
        Player targetPlayer = dogModule.fetchPlayer;
        Projectile targetProjectile = dogModule.fetchProj;

        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        TownNPCAnimationModule animationModule = npc.GetGlobalNPC<TownNPCAnimationModule>();
        switch ((int)stateValue) {
            case StateNavigatingToPlayerPreThrow: {
                if (!HandleNavigationToPlayerSubStates(npc, targetPlayer, pathfinderModule, StateWaitingForPlayerToThrow, 2f, PlayerIsValidToPlayFetchWith)) {
                    break;
                }

                animationModule.RequestAnimation(EasedKneeledTailWaggingAnimation);
                break;
            }
            case StateWaitingForPlayerToThrow: {
                if (!PlayerIsValidToPlayFetchWith(targetPlayer, npc) || ++genericTimer >= MaxDogPatienceBeforeThrow) {
                    CancelState(npc, pathfinderModule, dogModule);

                    break;
                }

                Projectile foundProjectile = null;
                foreach (Projectile proj in Main.ActiveProjectiles) {
                    if (proj.type != ModContent.ProjectileType<FetchingStickProj>() || proj.owner != targetPlayer.whoAmI || (int)proj.ai[1] == FetchingStickProj.StickClaimedByDog) {
                        continue;
                    }

                    foundProjectile = proj;
                }

                if (foundProjectile is null) {
                    break;
                }

                dogModule.fetchProj = foundProjectile;
                foundProjectile.ai[1] = FetchingStickProj.StickClaimedByDog;

                stateValue = StateWaitingForProjectileToSettle;
                genericTimer = 0;

                break;
            }
            case StateWaitingForProjectileToSettle: {
                ref float targetProjectileState = ref targetProjectile.ai[0];

                if (targetProjectile is null || !targetProjectile.active) {
                    CancelState(npc, pathfinderModule, dogModule);

                    break;
                }

                npc.direction = Math.Sign(npc.DirectionTo(targetProjectile.Center).X);

                if ((int)targetProjectileState != FetchingStickProj.AtRestState) {
                    break;
                }

                stateValue = StateFetching;

                break;
            }
            case StateFetching: {
                if (targetProjectile is null || !targetProjectile.active) {
                    CancelState(npc, pathfinderModule, dogModule);

                    break;
                }

                Point fetchPoint = (targetProjectile.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates();
                if (!pathfinderModule.IsPathfinding) {
                    bool hasPathToProj = pathfinderModule.HasPath(fetchPoint);
                    if (!hasPathToProj) {
                        CancelState(npc, pathfinderModule, dogModule);

                        break;
                    }

                    pathfinderModule.RequestPathfind(fetchPoint);
                }

                pathfinderModule.HorizontalSpeed = 3f;

                if (pathfinderModule.BottomLeftTileOfNPC == fetchPoint) {
                    animationModule.RequestAnimation(PickUpStickFromGroundAnimation);

                    stateValue = StateReachedProjectile;
                }

                break;
            }
            case StateReachedProjectile: {
                ref float targetProjectileState = ref targetProjectile.ai[0];

                if (genericTimer++ >= HalfKneeledFrameDuration * PickUpStickFromGroundAnimationFrameCount) {
                    stateValue = StateWalkingToPlayerPostFetch;
                    genericTimer = 0f;

                    targetProjectileState = FetchingStickProj.PickedUpByDog;
                }

                break;
            }
            case StateWalkingToPlayerPostFetch: {
                targetProjectile.direction = npc.direction;
                targetProjectile.Center = npc.Top + new Vector2(npc.Size.X * npc.direction, 4f);

                if (!HandleNavigationToPlayerSubStates(npc, targetPlayer, pathfinderModule, StateWaitingForPlayerToThrowAgainBuffer, 1.5f, PlayerWithinValidFetchingDistance)) {
                    break;
                }

                dogModule.fetchProj = null;
                targetProjectile.Kill();

                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    targetPlayer.QuickSpawnItem(new EntitySource_Gift(npc), ModContent.ItemType<FetchingStick>());
                }

                break;
            }
            case StateWaitingForPlayerToThrowAgainBuffer: {
                if (genericTimer++ < LWMUtils.RealLifeSecond) {
                    break;
                }

                if (!PlayerIsValidToPlayFetchWith(targetPlayer, npc)) {
                    CancelState(npc, pathfinderModule, dogModule);

                    break;
                }

                stateValue = StateWaitingForPlayerToThrow;
                genericTimer = 0f;

                animationModule.RequestAnimation(EasedKneeledTailWaggingAnimation);

                break;
            }
        }
    }
}