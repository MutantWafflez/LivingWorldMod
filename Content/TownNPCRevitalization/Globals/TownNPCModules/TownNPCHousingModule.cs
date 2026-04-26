using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Enums;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.DataStructures.Records;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.ObjectData;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

public class TownNPCHousingModule : TownNPCModule, IOnTownNPCAttack {
    private const int MaxBlockedHomeValue = LWMUtils.RealLifeSecond * 15;

    /// <summary>
    ///     The amount of ticks that must pass before this NPC is capable of trying to go home, even if they really want to.
    /// </summary>
    private BoundedNumber<int> _blockedHomeTimer = new(0, 0, MaxBlockedHomeValue);

    public override int UpdatePriority => -1;

    public Rectangle2D<int>? RoomBoundingBox {
        get;
        private set;
    }

    public Rectangle2D<int>? PrevTickRoomBoundingBox {
        get;
        private set;
        // Set to "invalid" so that the first update cycle we don't trigger a mass town-creation cascade
    } = Rectangle2D<int>.NegativeOne;

    public HomeRestingInfo RestInfo {
        get;
        private set;
    }

    public Point2D<int> CurrentHomeTilePos => new (NPC.homeTileX, NPC.homeTileY);

    public bool WillGoHome {
        get {
            bool wantsToSleep = NPC.TryGetGlobalNPC(out TownNPCSleepModule sleepModule) && sleepModule.WantsToSleep && sleepModule.CanSleep;

            return _blockedHomeTimer <= 0 && (wantsToSleep || Main.eclipse || Main.raining || Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon);
        }
    }

    public override void UpdateModule() {
        _blockedHomeTimer -= 1;

        if (CurrentHomeTilePos == Point2D<int>.NegativeOne && NPC.velocity.Y == 0f && !NPC.shimmering) {
            NPC.UpdateHomeTileState(NPC.homeless, (int)NPC.Center.X / 16, (int)(NPC.position.Y + NPC.height + 4f) / 16);
        }

        if (PrevTickRoomBoundingBox != Rectangle2D<int>.NegativeOne && PrevTickRoomBoundingBox != RoomBoundingBox) {
            if (PrevTickRoomBoundingBox is { } prevTickRoomBoundingBox) {
                TownNPCTownSystem.Instance.RemoveRoomFromTown(prevTickRoomBoundingBox);
            }

            if (RoomBoundingBox is { } roomBoundingBox) {
                TownNPCTownSystem.Instance.AddRoomToTown(roomBoundingBox);
            }
        }

        PrevTickRoomBoundingBox = RoomBoundingBox;

        UpdateRoomBoundingBox();

        HomelessTeleportCheck();
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) {
        _blockedHomeTimer += LWMUtils.RealLifeSecond * 2;
    }

    public Rectangle2D<int>? UpdateRoomBoundingBox() {
        return RoomBoundingBox = !NPC.homeless && WorldGen.StartRoomCheck(NPC.homeTileX, NPC.homeTileY - 1)
            ? new Rectangle2D<int>(WorldGen.roomX1, WorldGen.roomY1, WorldGen.roomX2 - WorldGen.roomX1 + 1, WorldGen.roomY2 - WorldGen.roomY1 + 1)
            : null;
    }

    // Adapted vanilla code
    public HomeRestingInfo GetRestingInfo() {
        Point floorPos = new (NPC.homeTileX, NPC.homeTileY);
        if (RoomBoundingBox is not { } boundingBox) {
            return new HomeRestingInfo(floorPos + new Point(0, -1), floorPos, NPCRestType.Floor);
        }

        while (!WorldGen.SolidOrSlopedTile(floorPos.X, floorPos.Y) && floorPos.Y < Main.maxTilesY - 20) {
            floorPos.Y++;
        }

        List<HomeRestingInfo> possibleRestInfos = [];
        for (int i = boundingBox.X; i <= boundingBox.X + boundingBox.Width; i++) {
            for (int j = boundingBox.Y; j <= boundingBox.Y + boundingBox.Height; j++) {
                Tile tile = Main.tile[i, j];
                bool isSittingTile = TileID.Sets.CanBeSatOnForNPCs[tile.TileType];
                bool isSleepingTile = TileID.Sets.CanBeSleptIn[tile.TileType];
                if (!tile.HasUnactuatedTile) {
                    continue;
                }

                NPCRestType restType;
                BedInfo? bedInfo = null;
                if (isSittingTile) {
                    restType = NPCRestType.Chair;
                }
                else if (isSleepingTile) {
                    restType = NPCRestType.Bed;

                    Point topLeftOfBed = LWMUtils.GetCornerOfMultiTile(tile, i, j, LWMUtils.CornerType.TopLeft);
                    PlayerSleepingHelper.GetSleepingTargetInfo(i, j, out int targetDirection, out Vector2 anchorPosition, out Vector2 visualOffset);

                    bedInfo = new BedInfo(tile.TileType, TileObjectData.GetTileStyle(Main.tile[topLeftOfBed]), targetDirection, anchorPosition, visualOffset);
                }
                else {
                    continue;
                }

                Point restPos = LWMUtils.GetCornerOfMultiTile(tile, i, j, LWMUtils.CornerType.BottomLeft);

                Point pathfindPos = restPos;
                Point npcTileWidthOffset = new (-(int)Math.Ceiling(NPC.width / 16f) + 1, 0);
                bool canStandOnRestPos = TownGlobalNPC.IsValidStandingPosition(NPC, restPos);
                if (isSittingTile && !canStandOnRestPos && TownGlobalNPC.IsValidStandingPosition(NPC, restPos + npcTileWidthOffset) ) {
                    pathfindPos.X += npcTileWidthOffset.X;
                }
                else if (isSleepingTile && !canStandOnRestPos) {
                    continue;
                }

                if (possibleRestInfos.Any(info => info.ActualRestTilePos == restPos)) {
                    continue;
                }

                HomeRestingInfo info = new(pathfindPos, restPos, restType, bedInfo);
                possibleRestInfos.Add(info);
                j = restPos.Y;
            }
        }

        return possibleRestInfos.Count == 0 ? new HomeRestingInfo(floorPos + new Point(0, -1), floorPos, NPCRestType.Floor) : possibleRestInfos.OrderByDescending(info => info.RestType).First();
    }

    public void OnTownNPCAttack(NPC npc) {
        _blockedHomeTimer += LWMUtils.RealLifeSecond * 8;
    }

    private void HomelessTeleportCheck() {
        //Adapted vanilla code
        Point bottomOfNPC = (NPC.Bottom + new Vector2(0, 1f)).ToTileCoordinates();
        // TODO: Make this check not every tick
        RestInfo = GetRestingInfo();

        if (!WorldGen.InWorld(bottomOfNPC.X, bottomOfNPC.Y) || (Main.netMode == NetmodeID.MultiplayerClient && !Main.sectionManager.TileLoaded(bottomOfNPC.X, bottomOfNPC.Y))) {
            return;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient || !WillGoHome) {
            return;
        }

        TownNPCPathfinderModule pathfinderModule = NPC.GetGlobalNPC<TownNPCPathfinderModule>();
        int beAtHomeStateInteger = TownNPCAIState.GetStateInteger<BeAtHomeAIState>();
        if (NPC.ai[0] != beAtHomeStateInteger) {
            TownNPCStateModule.RefreshToState(NPC, beAtHomeStateInteger);
            pathfinderModule.CancelPathfind();
        }

        bool nearbyPlayers = false;
        for (int i = 0; i < 2; i++) {
            Rectangle playerZoneCheck = i == 1
                ? new Rectangle(
                    (int)(NPC.Center.X - NPC.sWidth / 2f - NPC.safeRangeX),
                    (int)(NPC.Center.Y + NPC.height / 2f - NPC.sHeight / 2f - NPC.safeRangeY),
                    NPC.sWidth + NPC.safeRangeX * 2,
                    NPC.sHeight + NPC.safeRangeY * 2
                )
                : new Rectangle(
                    NPC.homeTileX * 16 + 8 - NPC.sWidth / 2 - NPC.safeRangeX,
                    NPC.homeTileY * 16 + 8 - NPC.sHeight / 2 - NPC.safeRangeY,
                    NPC.sWidth + NPC.safeRangeX * 2,
                    NPC.sHeight + NPC.safeRangeY * 2
                );

            foreach (Player player in Main.ActivePlayers) {
                if (!new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height).Intersects(playerZoneCheck)) {
                    continue;
                }


                nearbyPlayers = true;
                goto CheckTeleport_Attempt;
            }
        }

        CheckTeleport_Attempt:
        if (nearbyPlayers) {
            return;
        }

        bool validHouse = false;
        for (int i = -1; i < 2; i++) {
            int tileXByOffset = RestInfo.PathfindEndPos.X + i;
            if (Collision.SolidTiles(tileXByOffset - 1, tileXByOffset + 1, RestInfo.PathfindEndPos.Y - 2, RestInfo.PathfindEndPos.Y)) {
                continue;
            }

            NPC.velocity = Vector2.Zero;
            NPC.BottomLeft = RestInfo.PathfindEndPos.ToWorldCoordinates(8f, 16f);
            pathfinderModule.CancelPathfind();
            NPC.netUpdate = validHouse = true;
            break;
        }

        if (validHouse) {
            return;
        }

        NPC.homeless = true;
        WorldGen.QuickFindHome(NPC.whoAmI);
    }
}