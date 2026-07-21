using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.VFX;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Arcs under gravity, sticks where it lands, then detonates when its fuse (timeLeft) runs
	// out - same oversized-hitbox explosion pattern as ExplosiveKunaiProjectile.
	public class PaperBombProjectile : ModProjectile
	{
		private const int BlastRadius = 110;
		private const int FuseTicks = 180;

		private bool Stuck => Projectile.ai[0] == 1f;
		private bool Exploding => Projectile.ai[1] == 1f;

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = -1;
			Projectile.timeLeft = FuseTicks;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			if (Exploding)
			{
				return;
			}

			if (Stuck)
			{
				Projectile.velocity = Vector2.Zero;
				// fizzing fuse sparks
				if (Main.rand.NextBool(4))
				{
					Dust.NewDust(Projectile.position, Projectile.width, 4, DustID.Torch, Scale: 0.8f);
				}
			}
			else
			{
				Projectile.velocity.Y += 0.25f;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}

			if (Projectile.timeLeft <= 3)
			{
				Explode();
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
			ChakraVFX.SpawnFireBurst(Projectile.Center, scale: 3.5f);
			for (int i = 0; i < 12; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.ai[0] = 1f;
			return false;
		}
	}
}
