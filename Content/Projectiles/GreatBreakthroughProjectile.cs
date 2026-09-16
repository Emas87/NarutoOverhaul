using Terraria;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.Projectiles
{
	// Wind Style: Great Breakthrough - a short-range gust that pierces through everything in its
	// path with heavy knockback, unlike Fireball's single-target hit.
	public class GreatBreakthroughProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 36;
			Projectile.height = 20;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 20;
			Projectile.tileCollide = false;
			Projectile.knockBack = 7f;
		}

		public override void AI()
		{
			if (Main.rand.NextBool(2))
			{
				Common.VFX.ChakraVFX.SpawnBurstEffect<WindBurstProjectile>(Projectile.Center, 0.5f);
			}
		}

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnBurstEffect<WindBurstProjectile>(Projectile.Center, 1.875f);
		}
	}
}
