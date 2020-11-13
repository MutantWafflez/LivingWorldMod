using Microsoft.Xna.Framework;
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
