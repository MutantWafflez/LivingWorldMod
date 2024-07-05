using System;
using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.Projectiles;
using LivingWorldMod.Globals.BaseTypes.StatusEffects;
using LivingWorldMod.Globals.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace LivingWorldMod.Content.Villages.HarpyVillage.Pets;

//Thanks Trivaxy for the code! :-)
public class NimbusInABottle : BaseItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.damage = 0;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.width = 20;
        Item.height = 26;
        Item.UseSound = SoundID.Item44;
        Item.useAnimation = 20;
        Item.useTime = 20;
        Item.rare = ItemRarityID.Blue;
        Item.noMelee = true;
        Item.value = Item.sellPrice(silver: 60);
        Item.buffType = ModContent.BuffType<NimbusPetBuff>();
    }

    public override bool? UseItem(Player player) {
        if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
            player.AddBuff(Item.buffType, 20000);
            return true;
        }

        return base.UseItem(player);
    }
}

public class NimbusPetBuff : BaseStatusEffect {
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.buffTime[buffIndex] = 20000;                // stop the buff from expiring on its own
        player.GetModPlayer<PetPlayer>().nimbusPet = true; // keep the bool active

        int nimbusProjectileID = ModContent.ProjectileType<NimbusPetProjectile>();
        bool nimbusSpawned = player.ownedProjectileCounts[nimbusProjectileID] > 0;

        if (!nimbusSpawned && player.whoAmI == Main.myPlayer) {
            for (int i = 0; i < 15; i++) {
                Dust.NewDustPerfect(player.Center, 16, Main.rand.NextVector2Unit() * 3, Scale: Main.rand.NextFloat(0.8f, 1.5f));
            }

            Projectile.NewProjectile(new EntitySource_Buff(player, Type, buffIndex), player.Center - Vector2.UnitY * 5, Vector2.UnitX * player.direction * 5, nimbusProjectileID, 0, 0, player.whoAmI);
        }
    }
}

public class NimbusPetProjectile : BaseProjectile {
    private int _animationTimer;

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
            if (++_animationTimer % animationSpeedModulo == 0) {
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
        Rectangle sourceRectangle = new(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
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
            spriteEffects
        );

        return false;
    }
}