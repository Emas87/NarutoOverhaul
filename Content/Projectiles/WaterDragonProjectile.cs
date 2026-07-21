using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Water Style: Water Dragon Jutsu - large, slow-moving, pierces through several enemies,
	// the "big and sustained" counterpart to Fireball's quick single hit.
	public class WaterDragonProjectile : ModProjectile
	{
		// nano-banana-generated 6-frame sheet of distinct coiling/swimming poses - a directional
		// creature with its own head/tail read, so it cycles poses in place rather than spinning
		// the whole sprite like the symmetric orb/bolt jutsu.
		private const int FrameCount = 6;
		private const int TicksPerFrame = 8;
		private int animFrame;
		private int animTicks;

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 4;
			Projectile.timeLeft = 150;
			Projectile.tileCollide = true;
			Main.projFrames[Projectile.type] = FrameCount;
		}

		public override void AI()
		{
			if (Projectile.velocity.X != 0f)
			{
				Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;
			}

			Common.VFX.ChakraVFX.SpawnWaterBurst(Projectile.Center, 0.5f);

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
			Common.VFX.ChakraVFX.SpawnWaterBurst(Projectile.Center, 2.25f);
		}
	}
}
