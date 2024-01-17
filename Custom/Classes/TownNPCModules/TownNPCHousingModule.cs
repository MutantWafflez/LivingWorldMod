using LivingWorldMod.Common.GlobalNPCs;
using LivingWorldMod.Common.ModTypes;
using LivingWorldMod.Content.TownNPCAIStates;
using LivingWorldMod.Custom.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Events;

namespace LivingWorldMod.Custom.Classes.TownNPCModules;

public class TownNPCHousingModule : TownNPCModule {
    public Rectangle? RoomBoundingBox {
        get;
        private set;
    }

    public Point RestPos {
        get;
        private set;
    }

    public static bool ShouldGoHome => ShouldSleep || Main.eclipse || Main.raining;

    public static bool ShouldSleep => !(Main.dayTime || LanternNight.LanternsUp || Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon);

    public TownNPCHousingModule(NPC npc) : base(npc) {
        RoomBoundingBox = null;
    }

    public override void Update() {
        if (npc.homeTileX == -1 && npc.homeTileY == -1 && npc.velocity.Y == 0f && !npc.shimmering) {
            npc.UpdateHomeTileState(npc.homeless, (int)npc.Center.X / 16, (int)(npc.position.Y + npc.height + 4f) / 16);
        }

        HomelessTeleportCheck();

        if (npc.homeless) {
            RoomBoundingBox = null;
            return;
        }

        if (!WorldGen.StartRoomCheck(npc.homeTileX, npc.homeTileY - 1)) {
            return;
        }

        RoomBoundingBox = new Rectangle(WorldGen.roomX1, WorldGen.roomY1, WorldGen.roomX2 - WorldGen.roomX1 + 1, WorldGen.roomY2 - WorldGen.roomY1 + 1);
    }

    public void DebugDraw(SpriteBatch spriteBatch) {
        if (RoomBoundingBox is not { } boundingBox) {
            return;
        }

        Rectangle worldBoundingBox = new(boundingBox.X * 16, boundingBox.Y * 16, boundingBox.Width * 16, boundingBox.Height * 16);
        Utils.DrawRect(spriteBatch, worldBoundingBox, Main.DiscoColor);
    }

    // Adapted vanilla code
    public void FindRestingSpot(out int floorX, out int floorY) {
        floorX = npc.homeTileX;
        floorY = npc.homeTileY;
        if (floorX == -1 || floorY == -1 || RoomBoundingBox is not { } boundingBox) {
            return;
        }

        while (!WorldGen.SolidOrSlopedTile(floorX, floorY) && floorY < Main.maxTilesY - 20) {
            floorY++;
        }

        Point finalRestPos = new(-1, -1);
        bool foundBed = false;
        for (int i = boundingBox.X; i <= boundingBox.X + boundingBox.Width; i++) {
            for (int j = boundingBox.Y; j <= boundingBox.Y + boundingBox.Height; j++) {
                Tile tile = Main.tile[i, j];
                bool isSittingTile = TileID.Sets.CanBeSatOnForNPCs[tile.TileType];
                bool isSleepingTile = TileID.Sets.CanBeSleptIn[tile.TileType];
                if (!tile.HasUnactuatedTile || !isSleepingTile && !isSittingTile) {
                    continue;
                }

                if (isSittingTile && foundBed) {
                    continue;
                }

                if (isSleepingTile) {
                    foundBed = true;
                }

                finalRestPos.X = i;
                finalRestPos.Y = j;
            }
        }

        if (finalRestPos is { X: -1, Y: -1 }) {
            return;
        }

        Tile restTile = Main.tile[finalRestPos.X, finalRestPos.Y];
        if (TileID.Sets.CanBeSleptIn[restTile.TileType]) {
            PlayerSleepingHelper.GetSleepingTargetInfo(finalRestPos.X, finalRestPos.Y, out int targetDirection, out _, out _);
            finalRestPos = LWMUtils.GetCornerOfMultiTile(restTile, finalRestPos.X, finalRestPos.Y, targetDirection == -1 ? LWMUtils.CornerType.BottomRight : LWMUtils.CornerType.BottomLeft);
            finalRestPos.X += targetDirection;
        }
        else {
            finalRestPos = LWMUtils.GetCornerOfMultiTile(restTile, finalRestPos.X, finalRestPos.Y, LWMUtils.CornerType.BottomLeft);
        }
        finalRestPos.Y++;

        floorX = finalRestPos.X;
        floorY = finalRestPos.Y;
    }


    private void HomelessTeleportCheck() {
        //Adapted vanilla code
        Point bottomOfNPC = (npc.Bottom + new Vector2(0, 1f)).ToTileCoordinates();
        FindRestingSpot(out int floorX, out int floorY);
        RestPos = new Point(floorX, floorY - 1);

        if (!WorldGen.InWorld(bottomOfNPC.X, bottomOfNPC.Y) || Main.netMode == NetmodeID.MultiplayerClient && !Main.sectionManager.TileLoaded(bottomOfNPC.X, bottomOfNPC.Y)) {
            return;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient || !ShouldGoHome) {
            return;
        }

        TownNPCPathfinderModule pathfinderModule = GlobalNPC.PathfinderModule;
        int beAtHomeStateInt = TownNPCAIState.GetStateInteger<BeAtHomeAIState>();
        if (npc.ai[0] != beAtHomeStateInt) {
            TownGlobalNPC.RefreshToState(npc, beAtHomeStateInt);
            pathfinderModule.CancelPathfind();
        }

        bool nearbyPlayers = false;
        for (int i = 0; i < 2; i++) {
            Rectangle playerZoneCheck = i == 1
                ? new Rectangle(
                    (int)(npc.Center.X - NPC.sWidth / 2f - NPC.safeRangeX),
                    (int)(npc.Center.Y + npc.height / 2f - NPC.sHeight / 2f - NPC.safeRangeY),
                    NPC.sWidth + NPC.safeRangeX * 2,
                    NPC.sHeight + NPC.safeRangeY * 2
                )
                : new Rectangle(
                    npc.homeTileX * 16 + 8 - NPC.sWidth / 2 - NPC.safeRangeX,
                    npc.homeTileY * 16 + 8 - NPC.sHeight / 2 - NPC.safeRangeY,
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
            int tileXByOffset = floorX + i;
            if (Collision.SolidTiles(tileXByOffset - 1, tileXByOffset + 1, floorY - 3, floorY - 1)) {
                continue;
            }

            npc.velocity = Vector2.Zero;
            npc.BottomLeft = new Vector2(floorX, floorY).ToWorldCoordinates(Vector2.Zero);
            npc.netUpdate = validHouse = true;
            break;
        }

        if (validHouse) {
            return;
        }

        npc.homeless = true;
        WorldGen.QuickFindHome(npc.whoAmI);
    }
}