using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace LivingWorldMod.Projectiles.Friendly.Pets
{
	public class NimbusPetProjectile : ModProjectile
	{
		private int animationTimer;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Nimbus Pet");
			Main.projFrames[projectile.type] = 5;
			Main.projPet[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.width = 42;
			projectile.height = 28;
			projectile.penetrate = -1;
			projectile.netImportant = true;
			projectile.friendly = true;
			projectile.ignoreWater = true;
			projectile.tileCollide = false;
			projectile.frame = 1;
		}

		public override void AI()
		{
			Player player = Main.player[projectile.owner];

			if (!player.active)
			{
				projectile.active = false;
				return;
			}

			if (player.dead)
				player.GetModPlayer<LWMPlayer>().nimbusPet = false;

			if (player.GetModPlayer<LWMPlayer>().nimbusPet)
				projectile.timeLeft = 2;

			float targetPointXOffset = 80;
			Vector2 targetPoint = player.Center + Vector2.UnitX * (player.direction == 1 ? -targetPointXOffset / 2 : targetPointXOffset);
			projectile.velocity = (targetPoint - projectile.Center) / 13;

			projectile.rotation = (player.Center - projectile.Center).ToRotation();

			float maxDistSQFromDest = 4;
			if (Vector2.DistanceSquared(projectile.Center, targetPoint) <= maxDistSQFromDest && player.velocity == Vector2.Zero)
				projectile.frame = 0;
			else
			{
				int animationSpeedModulo = 5;
				if (++animationTimer % animationSpeedModulo == 0)
					if (++projectile.frame > 4)
						projectile.frame = 1;
			}

			projectile.direction = projectile.spriteDirection = projectile.Center.X > player.Center.X ? -1 : 1;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[projectile.type];

			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle sourceRectangle = new Rectangle(0, frameHeight * projectile.frame, texture.Width, frameHeight);
			SpriteEffects spriteEffects = projectile.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

			float breathScale = (float)Math.Sin(Main.GlobalTime * 2);

			spriteBatch.Draw(
				texture,
				projectile.position - Main.screenPosition + new Vector2(0f, projectile.gfxOffY),
				sourceRectangle,
				lightColor,
				projectile.rotation,
				new Vector2(projectile.width / 2, projectile.height / 2),
				projectile.frame == 0 ? new Vector2(1 + breathScale * 0.1f, 1 - breathScale * 0.1f) : Vector2.One,
				spriteEffects,
				0f);

			return false;
		}
	}
}
