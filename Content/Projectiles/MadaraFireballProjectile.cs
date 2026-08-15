using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Madara's own ranged attack, previously sharing ElementalBoltProjectile with Kakuzu/Kaguya
	// (all three read as an identical generic orb). Reuses FireballProjectile's existing art/anim
	// via the Texture override below instead of duplicating the PNG - friendly/hostile projectiles
	// are normally distinct classes even when visually similar (see FireballProjectile's own
	// comment), so this is a small hostile sibling rather than repurposing the player-only class.
	public class MadaraFireballProjectile : ModProjectile
	{
		public override string Texture => "NarutoOverhaul/Content/Projectiles/FireballProjectile";

		private const int FrameCount = 6;
		private const int TicksPerFrame = 6;
		private int animFrame;
		private int animTicks;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = true;
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
