using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Flies like a kunai, then detonates on the first thing it touches using the vanilla-grenade
	// pattern: swap into a short-lived oversized hitbox so everything in the blast takes the hit.
	public class ExplosiveKunaiProjectile : ModProjectile
	{
		private const int BlastRadius = 72;

		private bool Exploding => Projectile.ai[1] == 1f;

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 24;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			if (!Exploding)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
		}

		private void Explode()
		{
			if (Exploding)
			{
				return;
			}

			Projectile.ai[1] = 1f;
			Projectile.tileCollide = false;
			Projectile.velocity = Vector2.Zero;
			Projectile.alpha = 255;
			Projectile.Resize(BlastRadius, BlastRadius);
			Projectile.timeLeft = 3;
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			for (int i = 0; i < 20; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Scale: 1.5f);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Explode();
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Explode();
		}
	}
}
