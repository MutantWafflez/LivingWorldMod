using LivingWorldMod.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Projectiles.Friendly
{
    public class FeatherBagProjectile : ModProjectile
    {
        public WingID featherType;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Feather");     
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 2;    
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            Main.projFrames[projectile.type] = 26;
        }
		public override void SetDefaults()
		{
			projectile.width = 28;               
			projectile.height = 16;              
			projectile.aiStyle = -1;            
			projectile.friendly = true;        
			projectile.ranged = true;         
			projectile.penetrate = 2;          
			projectile.timeLeft = 360;
			projectile.tileCollide = true;
		}

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation();
            projectile.frame = (int)featherType;
        }

        public override void Kill(int timeLeft)
        {
            Gore.NewGore(projectile.position, Vector2.Zero, Main.rand.Next(11, 14));
        }

        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write((int)featherType);
        }

        public override void ReceiveExtraAI(BinaryReader reader) {
            featherType = (WingID)reader.ReadInt32();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Texture2D textureToDraw = Main.projectileTexture[projectile.type];
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (++projectile.frameCounter <= 5) {
                spriteEffects = SpriteEffects.FlipVertically;
            }
            else if (projectile.frameCounter > 10) {
                projectile.frameCounter = 0;
            }
            spriteBatch.Draw(textureToDraw, projectile.position - Main.screenPosition, new Rectangle(0, projectile.frame * (textureToDraw.Height / Main.projFrames[projectile.type]), textureToDraw.Width, (textureToDraw.Height / Main.projFrames[projectile.type])), lightColor, projectile.rotation, Vector2.Zero, projectile.scale, spriteEffects, 0f);
            return false;
        }
    }
}
