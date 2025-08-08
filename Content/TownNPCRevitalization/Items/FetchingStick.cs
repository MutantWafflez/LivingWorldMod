using LivingWorldMod.Globals.BaseTypes.Items;
using LivingWorldMod.Globals.BaseTypes.Projectiles;
using Microsoft.Xna.Framework;

namespace LivingWorldMod.Content.TownNPCRevitalization.Items;

/// <summary>
///     A stick that can be thrown by the player, and if there is a Pet Dog nearby, it will attempt to fetch it!
/// </summary>
public class FetchingStick : BaseItem {
    public const int StickWidth = 30;
    public const int StickHeight = 28;

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.Snowball);
        Item.damage = 3;
        Item.shoot = ModContent.ProjectileType<FetchingStickProj>();
        Item.knockBack = 0.2f;
        Item.shootSpeed = 12f;
        Item.ammo = AmmoID.None;
        Item.width = StickWidth;
        Item.height = StickHeight;
    }
}

/// <summary>
///     <see cref="FetchingStick" /> associated projectile.
/// </summary>
public class FetchingStickProj : BaseProjectile {
    private const float FreeFlyingState = 0f;
    private const float FirstTileCollisionState = 1f;
    private const float AtRestState = 2f;

    private const float TerminalVelocity = 16f;

    public override string Texture => ModContent.GetInstance<FetchingStick>().Texture;

    private ref float StickAIState => ref Projectile.ai[0];

    public override void SetDefaults() {
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.width = (int)(FetchingStick.StickWidth * 0.75f);
        Projectile.height = (int)(FetchingStick.StickHeight * 0.75f);
    }

    public override void AI() {
        Projectile.velocity.Y += 0.4f;
        if (Projectile.velocity.Y >= TerminalVelocity) {
            Projectile.velocity.Y = TerminalVelocity;
        }

        if (StickAIState > FirstTileCollisionState) {
            return;
        }

        Projectile.rotation += Projectile.velocity.X * 0.06f;
    }

    public override bool? CanDamage() => Projectile.ai[0] == FreeFlyingState;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        CollideWithObject();
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        CollideWithObject();

        return false;
    }

    private void CollideWithObject() {
        switch (StickAIState) {
            case FreeFlyingState:
                StickAIState = FirstTileCollisionState;

                Projectile.velocity.X = -Projectile.velocity.X * 0.05f;
                Projectile.velocity.Y = -Projectile.velocity.Y * 0.1f;
                break;
            case FirstTileCollisionState:
                StickAIState = AtRestState;

                Projectile.velocity *= 0f;
                break;
        }
    }
}