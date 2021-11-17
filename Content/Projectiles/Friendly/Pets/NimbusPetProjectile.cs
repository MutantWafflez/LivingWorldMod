using LivingWorldMod.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Content.Projectiles.Friendly.Pets {
    //Thanks Trivaxy for the code! :-)
    public class NimbusPetProjectile : BaseProjectile {
        private int animationTimer;

        public override void SetStaticDefaults() {
            Main.projFrames[Projectile.type] = 5;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults() {
            Projectile.width = 42;
            Projectile.height = 28;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.frame = 1;
        }

        public override void AI() {
            Player player = Main.player[Projectile.owner];

            if (!player.active) {
                Projectile.active = false;
                return;
            }

            if (player.dead) {
                player.GetModPlayer<PetPlayer>().nimbusPet = false;
            }

            if (player.GetModPlayer<PetPlayer>().nimbusPet) {
                Projectile.timeLeft = 2;
            }

            float targetPointXOffset = 80;
            Vector2 targetPoint = player.Center + Vector2.UnitX * (player.direction == 1 ? -targetPointXOffset / 2 : targetPointXOffset);
            Projectile.velocity = (targetPoint - Projectile.Center) / 13;

            Projectile.rotation = (player.Center - Projectile.Center).ToRotation();

            float maxDistSQFromDest = 4;
            if (Vector2.DistanceSquared(Projectile.Center, targetPoint) <= maxDistSQFromDest && player.velocity == Vector2.Zero) {
                Projectile.frame = 0;
            }
            else {
                int animationSpeedModulo = 5;
                if (++animationTimer % animationSpeedModulo == 0) {
                    if (++Projectile.frame > 4) {
                        Projectile.frame = 1;
                    }
                }
            }

            Projectile.direction = Projectile.spriteDirection = Projectile.Center.X > player.Center.X ? -1 : 1;
        }

        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            SpriteEffects spriteEffects = Projectile.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            float breathScale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2);

            Main.EntitySpriteDraw(
                texture,
                Projectile.position - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle,
                lightColor,
                Projectile.rotation,
                new Vector2(Projectile.width / 2, Projectile.height / 2),
                Projectile.frame == 0 ? 1 + breathScale * 0.1f : 1f,
                spriteEffects,
                0);

            return false;
        }
    }
}