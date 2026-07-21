using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Pain's Almighty Push - a stationary burst centered on the caster. Deliberately simple: a
	// large hitbox with a short lifetime and high Item.knockBack, letting vanilla's own
	// projectile-vs-NPC collision/knockback pipeline do the "everyone nearby gets shoved" work
	// rather than a hand-written NPC-radius loop.
	public class ShinraTenseiProjectile : ModProjectile
	{
		// Expanding-ring sheet: frame 0 is a tiny ring, frame 5 is the huge faded shockwave at
		// full extent. Driven directly off timeLeft rather than an independent tick counter, so
		// the visual ring size always matches how far into its (short, one-shot) 12-tick life it is.
		private const int FrameCount = 6;
		private const int LifetimeTicks = 12;

		public override void SetDefaults()
		{
			Projectile.width = 220;
			Projectile.height = 220;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<GenjutsuDamageClass>();
			Projectile.penetrate = -1;
			Projectile.timeLeft = LifetimeTicks;
			Projectile.tileCollide = false;
			Main.projFrames[Projectile.type] = FrameCount;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == LifetimeTicks)
			{
				Common.VFX.ChakraVFX.SpawnGenjutsuBurst(Projectile.Center, 2.5f);
				SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			}

			int elapsed = LifetimeTicks - Projectile.timeLeft;
			Projectile.frame = Utils.Clamp(elapsed * FrameCount / LifetimeTicks, 0, FrameCount - 1);
		}
	}
}
