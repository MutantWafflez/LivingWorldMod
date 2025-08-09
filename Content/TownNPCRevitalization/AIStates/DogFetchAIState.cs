using System;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;
using LivingWorldMod.Content.TownNPCRevitalization.Items;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.TownNPCRevitalization.AIStates;

public class DogFetchAIState : TownNPCAIState {
    private const int WalkingToPlayerPreThrowState = 0;
    private const int WaitingForPlayerToThrowState = 1;
    private const int WaitingForProjectileToSettleState = 2;
    private const int FetchingState = 3;
    private const int ReachedProjectileState = 4;
    private const int WalkingToPlayerPostFetchState = 5;
    private const int WaitingForPlayerToThrowAgainBufferState = 6;

    public override void DoState(NPC npc) {
        TownDogModule dogModule = npc.GetGlobalNPC<TownDogModule>();
        Player targetPlayer = dogModule.fetchPlayer;
        Projectile targetProjectile = dogModule.fetchProj;

        TownNPCPathfinderModule pathfinderModule = npc.GetGlobalNPC<TownNPCPathfinderModule>();
        switch ((int)npc.ai[1]) {
            case WalkingToPlayerPreThrowState: {
                Point dogStandPoint = (targetPlayer.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates()
                    + new Point(targetPlayer.direction * 2 + (targetPlayer.direction == 1 ? (int)Math.Ceiling(targetPlayer.width / 16f) - 1 : 0), 0);

                pathfinderModule.HorizontalSpeed = 2f;
                pathfinderModule.RequestPathfind(dogStandPoint);

                if (pathfinderModule.BottomLeftTileOfNPC == dogStandPoint) {
                    npc.direction = Math.Sign(npc.DirectionTo(targetPlayer.Center).X);
                    npc.ai[1] = WaitingForPlayerToThrowState;
                }

                break;
            }
            case WaitingForPlayerToThrowState: {
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
                npc.ai[1] = WaitingForProjectileToSettleState;

                break;
            }
            case WaitingForProjectileToSettleState: {
                npc.direction = Math.Sign(npc.DirectionTo(targetProjectile.Center).X);
                if (targetProjectile.ai[0] != FetchingStickProj.AtRestState) {
                    break;
                }

                npc.ai[1] = FetchingState;

                break;
            }
            case FetchingState: {
                Point fetchPoint = (targetProjectile.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates();

                pathfinderModule.HorizontalSpeed = 3f;
                pathfinderModule.RequestPathfind(fetchPoint);

                if (pathfinderModule.BottomLeftTileOfNPC == fetchPoint) {
                    npc.ai[1] = ReachedProjectileState;
                }

                break;
            }
            case ReachedProjectileState: {
                if (npc.ai[2]++ >= LWMUtils.RealLifeSecond) {
                    npc.ai[1] = WalkingToPlayerPostFetchState;
                    npc.ai[2] = 0f;

                    targetProjectile.ai[0] = FetchingStickProj.PickedUpByDog;
                }

                break;
            }
            case WalkingToPlayerPostFetchState: {
                Point dogStandPoint = (targetPlayer.BottomLeft + new Vector2(0, -2f)).ToTileCoordinates()
                    + new Point(targetPlayer.direction * 2 + (targetPlayer.direction == 1 ? (int)Math.Ceiling(targetPlayer.width / 16f) - 1 : 0), 0);

                pathfinderModule.HorizontalSpeed = 1.5f;
                pathfinderModule.RequestPathfind(dogStandPoint);

                targetProjectile.direction = npc.direction;
                targetProjectile.Center = npc.Top + new Vector2(npc.Size.X * npc.direction, 4f);

                if (pathfinderModule.BottomLeftTileOfNPC == dogStandPoint) {
                    npc.ai[1] = WaitingForPlayerToThrowAgainBufferState;

                    dogModule.fetchProj = null;
                    targetProjectile.Kill();

                    targetPlayer.QuickSpawnItem(new EntitySource_Gift(npc), ModContent.ItemType<FetchingStick>());
                }

                break;
            }
            case WaitingForPlayerToThrowAgainBufferState: {
                if (npc.ai[2]++ < LWMUtils.RealLifeSecond * 3f) {
                    break;
                }

                if (targetPlayer.HeldItem.type != ModContent.ItemType<FetchingStick>()) {
                    TownNPCStateModule.RefreshToState<DefaultAIState>(npc);
                    break;
                }

                npc.ai[1] = WaitingForPlayerToThrowState;
                npc.ai[2] = 0f;

                break;
            }
        }
    }
}