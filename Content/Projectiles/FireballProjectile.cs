using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Player-owned counterpart to ElementalBoltProjectile's Fire variant - kept separate since
	// friendly/hostile projectiles are normally distinct classes even when visually similar.
	public class FireballProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.rotation += 0.25f;

			if (Main.rand.NextBool(2))
			{
				Common.VFX.ChakraVFX.SpawnBurst(Projectile.Center, DustID.Torch, 2, 1f);
			}
		}

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnBurst(Projectile.Center, DustID.Torch, 10, 1.4f);
		}
	}
}
