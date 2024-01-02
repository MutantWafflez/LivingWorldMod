using System.Linq;
using LivingWorldMod.Common.VanillaOverrides.NPCProfiles;
using LivingWorldMod.Custom.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Common.GlobalNPCs;

/// <summary>
/// GlobalNPC that handles visual updates on town NPCs for various aesthetic purposes.
/// </summary>
//TODO: Finish NPC umbrella stuff & sleeping testing
[Autoload(false)]
public class TownChangesNPC : GlobalNPC {
    private static RainProfile _rainProfile;

    public override bool InstancePerEntity => true;

    [CloneByReference]
    public BedData ownedBed;

    [CloneByReference]
    public int bedPhase;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.townNPC && entity.aiStyle == 7 && entity.type != NPCID.OldMan;

    public override void Load() {
        _rainProfile = new RainProfile();
    }

    public override void Unload() {
        _rainProfile = null;
    }

    public override ITownNPCProfile ModifyTownNPCProfile(NPC npc) =>
        //Rain profiles
        /*
        if (npc.type == NPCID.Guide) {
            return _rainProfile;
        }
        */
        null;

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (bedPhase == 3 && ownedBed is not null) {
            TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile);
            Rectangle drawPos = new(
                (int)(ownedBed.bedPosition.X * 16f + (ownedBed.bedDirection < 1 ? 66f : 56f) - screenPos.X),
                (int)(ownedBed.bedPosition.Y * 16f - 8f - screenPos.Y),
                npc.frame.Width,
                npc.frame.Height
            );

            Asset<Texture2D> drawTexture = profile is not null ? profile.GetTextureNPCShouldUse(npc) : TextureAssets.Npc[npc.type];
            spriteBatch.Draw(
                drawTexture.Value,
                drawPos,
                drawTexture.Frame(1, Main.npcFrameCount[npc.type], 0, 20),
                drawColor,
                MathHelper.PiOver2,
                Vector2.Zero,
                (SpriteEffects)(ownedBed.bedDirection + 1),
                0f
            );

            return false;
        }

        return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }

    public override void AI(NPC npc) {
        //Any bed breakage, player chattage, damage, or blood moon-age will reset the NPC and prevent sleep
        if (ownedBed is null || Main.player.Any(player => player.talkNPC == npc.whoAmI) || npc.life < npc.lifeMax || Main.bloodMoon) {
            bedPhase = 0;

            return;
        }

        if (bedPhase == 0) {
            if (!Main.dayTime) {
                bedPhase = 1;

                npc.netUpdate = true;
            }
        }
        else if (bedPhase == 1) {
            //At night, if there is a bed for this NPC, begin to walk towards it
            npc.direction = npc.DirectionTo(ownedBed.bedPosition.ToWorldCoordinates()).X > 0 ? 1 : -1;
            npc.ai[0] = 1f;
            npc.ai[1] = 1f;
            //When an NPC sits, even if they aren't in the right AI state, they stand still. Forcing their velocity un-anchors their sitting position
            if (npc.velocity.X == 0f) {
                npc.velocity.X = 1f;
            }

            //Upon bed intersection, move to next step
            if (npc.Hitbox.Intersects(new Rectangle((int)(ownedBed.bedPosition.X * 16f), (int)(ownedBed.bedPosition.Y * 16f), 16, 16))) {
                bedPhase = 2;

                //"Touching" ai state
                npc.ai[0] = 9f;
                npc.ai[1] = 61f;

                npc.netUpdate = true;
            }
        }
        else if (bedPhase == 2) {
            //"Touching bed" phase over, get into bed
            if (npc.ai[1] <= 1f) {
                bedPhase = 3;

                npc.ai[0] = -5f;
                npc.ai[1] = 0f;

                npc.Bottom = ownedBed.bedPosition.ToWorldCoordinates(32f, 31f);
                Main.sleepingManager.AddNPC(npc.whoAmI, ownedBed.bedPosition);

                npc.netUpdate = true;
            }
        }

        if (bedPhase == 3) {
            npc.Bottom = ownedBed.bedPosition.ToWorldCoordinates(32f, 31f);

            //If in bed and it becomes daytime, wake up and stand still for 3 seconds
            if (Main.dayTime) {
                bedPhase = 0;

                npc.ai[0] = 0f;
                npc.ai[1] = 180f;
            }
        }
    }

    public override void PostAI(NPC npc) {
        if (npc.homeless || ownedBed is null || ownedBed is { } data && (!Main.tile[data.bedPosition].HasTile || Main.tile[data.bedPosition].TileType != TileID.Beds)) {
            ownedBed = null;

            if (bedPhase > 0) {
                bedPhase = 0;

                npc.ai[0] = 0f;
                npc.ai[1] = 180f;
            }
        }
    }

    public override void SaveData(NPC npc, TagCompound tag) {
        tag["bedPos"] = ownedBed?.bedPosition.ToVector2() ?? Vector2.Zero;
        tag["bedPhase"] = bedPhase;
    }

    public override void LoadData(NPC npc, TagCompound tag) {
        Point potentialBed = tag.Get<Vector2>("bedPos").ToPoint();
        if (potentialBed != Point.Zero) {
            ownedBed = new BedData(potentialBed);
        }

        bedPhase = tag.GetInt("bedPhase");
    }
}