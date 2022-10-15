using System.Linq;
using LivingWorldMod.Common.VanillaOverrides.NPCProfiles;
using LivingWorldMod.Custom.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace LivingWorldMod.Common.GlobalNPCs {
    /// <summary>
    /// GlobalNPC that handles visual updates on town NPCs for various aesthetic purposes.
    /// </summary>
    //TODO: Finish NPC umbrella stuff
    public class TownChangesNPC : GlobalNPC {
        private static RainProfile _rainProfile;

        [CloneByReference]
        public BedData ownedBed;

        [CloneByReference]
        private int _bedPhase = 0;

        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.townNPC && entity.aiStyle == 7;

        public override void Load() {
            _rainProfile = new RainProfile();
        }

        public override void Unload() {
            _rainProfile = null;
        }

        public override ITownNPCProfile ModifyTownNPCProfile(NPC npc) {
            //Rain profiles
            /*
            if (npc.type == NPCID.Guide) {
                return _rainProfile;
            }
            */

            return null;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if (_bedPhase == 2 && ownedBed is not null) {
                TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile);
                Rectangle drawPos = new Rectangle(
                    (int)(ownedBed.bedPosition.X * 16f + (ownedBed.bedDirection < 1 ? 64f : 0f) + 2f - screenPos.X), 
                    (int)(ownedBed.bedPosition.Y * 16f - 4f - screenPos.Y), 
                    npc.frame.Width, 
                    npc.frame.Height
                    );

                spriteBatch.Draw(
                    profile is not null ? profile.GetTextureNPCShouldUse(npc).Value : TextureAssets.Npc[npc.type].Value,
                    drawPos,
                    npc.frame,
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
            //Any bed breakage, player chattage, or damage will reset the NPC and prevent sleep
            if (ownedBed is null || Main.player.Any(player => player.talkNPC == npc.whoAmI) || npc.life < npc.lifeMax) {
                _bedPhase = -1;

                return;
            }

            if (_bedPhase == -1) {
                if (!Main.dayTime) {
                    _bedPhase = 0;
                }
            }
            else if (_bedPhase == 0) {
                //At night, if there is a bed for this NPC, begin to walk towards it
                npc.direction = npc.DirectionTo(ownedBed.bedPosition.ToWorldCoordinates()).X > 0 ? 1 : -1;
                npc.ai[0] = 1f;
                npc.ai[1] = 1f;
                    
                //Upon bed intersection, move to next step
                if (npc.Hitbox.Intersects(new Rectangle((int)(ownedBed.bedPosition.X * 16f), (int)(ownedBed.bedPosition.Y * 16f), 16, 16))) {
                    _bedPhase = 1;

                    //"Touching" ai state
                    npc.ai[0] = 9f;
                    npc.ai[1] = 31f;
                }
            }
            else if (_bedPhase == 1) {
                //"Touching bed" phase over, get into bed
                if (npc.ai[1] <= 1f) {
                    _bedPhase = 2;

                    npc.ai[0] = -5f;
                    npc.ai[1] = 0f;

                    npc.Bottom = ownedBed.bedPosition.ToWorldCoordinates(32f, 16f);
                }
            }

            //If in bed and it becomes daytime, wake up and stand still for 3 seconds
            if (_bedPhase == 2 && Main.dayTime) {
                _bedPhase = -1;

                npc.ai[0] = 0f;
                npc.ai[1] = 180f;
            }
        }

        public override void PostAI(NPC npc) {
            ownedBed = npc.homeless ? null : ownedBed;
        }
    }
}