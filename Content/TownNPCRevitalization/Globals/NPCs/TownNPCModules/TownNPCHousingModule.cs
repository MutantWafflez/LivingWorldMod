using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.AIStates;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Enums;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;

public class TownNPCHousingModule : TownNPCModule {
    public override int UpdatePriority => -1;

    public Rectangle? RoomBoundingBox {
        get;
        private set;
    }

    public HomeRestingInfo RestInfo {
        get;
        private set;
    }

    public bool ShouldGoHome {
        get {
            TownNPCSleepModule sleepModule = NPC.GetGlobalNPC<TownNPCSleepModule>();
            return (sleepModule.WantsToSleep && sleepModule.CanSleep) || Main.eclipse || Main.raining || Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon;
        }
    }

    public override void UpdateModule() {
        if (NPC.homeTileX == -1 && NPC.homeTileY == -1 && NPC.velocity.Y == 0f && !NPC.shimmering) {
            NPC.UpdateHomeTileState(NPC.homeless, (int)NPC.Center.X / 16, (int)(NPC.position.Y + NPC.height + 4f) / 16);
        }

        HomelessTeleportCheck();

        if (NPC.homeless) {
            RoomBoundingBox = null;
            return;
        }

        if (!WorldGen.StartRoomCheck(NPC.homeTileX, NPC.homeTileY - 1)) {
            return;
        }

        RoomBoundingBox = new Rectangle(WorldGen.roomX1, WorldGen.roomY1, WorldGen.roomX2 - WorldGen.roomX1 + 1, WorldGen.roomY2 - WorldGen.roomY1 + 1);
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (RoomBoundingBox is not { } boundingBox || !LWM.IsDebug) {
            return;
        }

        Rectangle worldBoundingBox = new(boundingBox.X * 16, boundingBox.Y * 16, boundingBox.Width * 16, boundingBox.Height * 16);
        Utils.DrawRect(spriteBatch, worldBoundingBox, Main.DiscoColor);
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
                if (isSittingTile) {
                    restType = NPCRestType.Chair;
                }
                else if (isSleepingTile) {
                    restType = NPCRestType.Bed;
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

                HomeRestingInfo info = new (pathfindPos, restPos, restType);
                possibleRestInfos.Add(info);
                j = restPos.Y;
            }
        }

        return possibleRestInfos.Count == 0 ? new HomeRestingInfo(floorPos + new Point(0, -1), floorPos, NPCRestType.Floor) : possibleRestInfos.OrderByDescending(info => info.RestType).First();
    }

    private void HomelessTeleportCheck() {
        //Adapted vanilla code
        Point bottomOfNPC = (NPC.Bottom + new Vector2(0, 1f)).ToTileCoordinates();
        RestInfo = GetRestingInfo();

        if (!WorldGen.InWorld(bottomOfNPC.X, bottomOfNPC.Y) || (Main.netMode == NetmodeID.MultiplayerClient && !Main.sectionManager.TileLoaded(bottomOfNPC.X, bottomOfNPC.Y))) {
            return;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient || !ShouldGoHome) {
            return;
        }

        TownNPCPathfinderModule pathfinderModule = NPC.GetGlobalNPC<TownNPCPathfinderModule>();
        int beAtHomeStateInt = TownNPCAIState.GetStateInteger<BeAtHomeAIState>();
        if ( /*npc.ai[0] != TownNPCAIState.GetStateInteger<WalkToRandomPosState>() &&*/ !NPC.GetGlobalNPC<TownNPCCombatModule>().IsAttacking && NPC.ai[0] != beAtHomeStateInt) {
            TownNPCStateModule.RefreshToState(NPC, beAtHomeStateInt);
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

            for (int j = 0; j < Main.maxPlayers; j++) {
                if (!Main.player[j].active || !new Rectangle((int)Main.player[j].position.X, (int)Main.player[j].position.Y, Main.player[j].width, Main.player[j].height).Intersects(playerZoneCheck)) {
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