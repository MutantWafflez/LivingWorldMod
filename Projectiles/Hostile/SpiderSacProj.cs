using LivingWorldMod.Gores;
using LivingWorldMod.NPCs.Hostiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LivingWorldMod.Projectiles.Hostile
{
    public class SpiderSacProj : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Falling Spider Sac");
        }

        public override void SetDefaults()
        {
            projectile.CloneDefaults(ProjectileID.BeeHive); //The hives in the jungle
            projectile.hostile = true;
            projectile.friendly = false;
            projectile.ignoreWater = false;
            projectile.width = 32;
            projectile.height = 28; //Is slightly smaller than the actual sprite so that on tile contact it looks a bit more smooth
        }
        
        //We want it to cut tiles (such as webs) but not hit NPCs or players
        public override bool? CanCutTiles() => true;

        public override bool? CanHitNPC(NPC target) => false;

        public override bool CanHitPlayer(Player target) => false;

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Gore.NewGore(projectile.Center, new Vector2(Main.rand.NextFloat(-4, -3), Main.rand.NextFloat(-3, -1)), mod.GetGoreSlot("Gores/SpiderSacGore"));
            Gore.NewGore(projectile.Center, new Vector2(Main.rand.NextFloat(3, 4), Main.rand.NextFloat(-3, -1)), mod.GetGoreSlot("Gores/SpiderSacGore"));
            
            int spiderCount = Main.rand.Next(2, 6);
            for (int i = 0; i < spiderCount; i++)
            {
                int xDisplacement = Main.rand.Next(-12, 14);
                int yDisplacement = Main.rand.Next(-8, 0);
                NPC.NewNPC((int)projectile.Center.X + xDisplacement, (int)projectile.Center.Y + yDisplacement, ModContent.NPCType<SacSpiderWalled>(), ai0: Main.rand.Next(-100, 101));
                //The spider's npc.ai[0] is randomized so that all the spiders don't go the same direction when they are idle
            }
            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/SpiderSacBurst"));
            return true;
        }
    }
}
