using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Player-owned counterpart to ElementalBoltProjectile's Fire variant - kept separate since
	// friendly/hostile projectiles are normally distinct classes even when visually similar.
	public class FireballProjectile : ModProjectile
	{
		private const int FrameCount = 6;
		private const int TicksPerFrame = 6;
		private int animFrame;
		private int animTicks;

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
			Main.projFrames[Projectile.type] = FrameCount;
		}

		public override void AI()
		{
			Projectile.rotation += 0.25f;

			if (Main.rand.NextBool(2))
			{
				Common.VFX.ChakraVFX.SpawnFireBurst(Projectile.Center, 0.6f);
			}

			animTicks++;
			if (animTicks >= TicksPerFrame)
			{
				animTicks = 0;
				animFrame = (animFrame + 1) % FrameCount;
			}
			Projectile.frame = animFrame;
		}

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnFireBurst(Projectile.Center, 1.75f);
		}
	}
}
