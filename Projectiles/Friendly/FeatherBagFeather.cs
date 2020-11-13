using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Projectiles.Friendly
{
    public class FeatherBagFeather : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_" + ProjectileID.HarpyFeather;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Feather");     
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 2;    
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;        
        }
		public override void SetDefaults()
		{
			projectile.width = 8;               
			projectile.height = 8;              
			projectile.aiStyle = 0;            
			projectile.friendly = true;        
			projectile.ranged = true;         
			projectile.penetrate = 2;          
			projectile.timeLeft = 360;
			projectile.tileCollide = true;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.5f, projectile.height * 0.5f);
			for (int k = 0; k < projectile.oldPos.Length; k++)
			{
				Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, projectile.gfxOffY);
				Color color = projectile.GetAlpha(lightColor) * ((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
				spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, null, color, projectile.rotation, drawOrigin, projectile.scale, SpriteEffects.None, 0f);
			}
			return true;
		}
		public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(270);
        }
        public override void Kill(int timeLeft)
        {
            Gore.NewGore(projectile.Center, Vector2.Zero, Main.rand.Next(11, 14));
        }
    }
}
