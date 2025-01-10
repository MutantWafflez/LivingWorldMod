using System;
using System.Collections.Generic;
using System.Linq;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.ModTypes;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;

namespace LivingWorldMod.Content.TownNPCRevitalization.Activities;

/// <summary>
///     Activity where the given NPC will look for the nearest campfire and try to roast
///     a marshmallow over it.
/// </summary>
public class RoastMarshmallowActivity : TownNPCActivity {
    private Point _standingLocation;
    private bool _isAtStandingLocation;

    /*public override void FrameNPC(TownGlobalNPC globalNPC, NPC npc, int frameHeight) {
        if (!_isAtStandingLocation) {
            return;
        }

        npc.frame.Y = frameHeight * 17;
    }*/

    /*public override void PostDrawNPC(TownGlobalNPC globalNPC, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!_isAtStandingLocation) {
            return;
        }

        Main.instance.LoadItem(ItemID.MarshmallowonaStick);
        spriteBatch.Draw(
            TextureAssets.Item[ItemID.MarshmallowonaStick].Value,
            npc.Center - screenPos + new Vector2(10f, -8f),
            drawColor
        );
    }*/

    public override void DoState( NPC npc) {
        if ((npc.BottomLeft + new Vector2(0, -2)).ToTileCoordinates() == _standingLocation) {
            _isAtStandingLocation = true;
            npc.direction = 1;
            return;
        }

        npc.GetGlobalNPC<TownNPCPathfinderModule>().RequestPathfind(_standingLocation);
        _isAtStandingLocation = false;
    }

    public override bool CanDoActivity(TownGlobalNPC globalNPC, NPC npc) {
        Point origin = npc.GetGlobalNPC<TownNPCPathfinderModule>().TopLeftOfPathfinderZone;
        List<Point> campfires = [];
        for (int i = origin.X; i < origin.X + TownNPCPathfinderModule.PathfinderSize; i++) {
            for (int j = origin.Y; j < origin.Y + TownNPCPathfinderModule.PathfinderSize; j++) {
                Tile tile = Main.tile[i, j];
                if (!TileID.Sets.Campfire[tile.TileType]) {
                    continue;
                }

                Point bottomLeftOfCampfire = LWMUtils.GetCornerOfMultiTile(tile, i, j, LWMUtils.CornerType.BottomLeft);
                if (campfires.Contains(bottomLeftOfCampfire)) {
                    continue;
                }

                int npcTileWidth = (int)Math.Ceiling(npc.width / 16f);
                int npcTileHeight = (int)Math.Ceiling(npc.height / 16f);

                // Local var for formatting
                bool canNPCFitInSpace = WorldUtils.Find(
                    new Point(bottomLeftOfCampfire.X - 2, bottomLeftOfCampfire.Y - npcTileHeight + 1),
                    Searches.Chain(
                        new Searches.Rectangle(npcTileWidth, npcTileHeight),
                        new Conditions.IsSolid().Not()
                    ),
                    out _
                );

                if (!canNPCFitInSpace) {
                    continue;
                }

                campfires.Add(bottomLeftOfCampfire);
            }
        }

        if (campfires.Count == 0) {
            return false;
        }

        _standingLocation = campfires.MinBy(point => npc.Distance(point.ToWorldCoordinates(0f, 0f))) + new Point(-2, 0);
        return npc.GetGlobalNPC<TownNPCPathfinderModule>().HasPath(_standingLocation);
    }
}