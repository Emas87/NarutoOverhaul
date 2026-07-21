using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.VFX
{
	// Shared base for the animated burst-effect family (Content/Projectiles/Bursts/) - the "richer
	// custom particle" replacement for a subset of ChakraVFX.SpawnBurst's plain Dust calls. Purely
	// cosmetic: no damage, no collision, just a one-shot sprite-sheet animation synced to its own
	// short lifetime (same timeLeft-driven approach as ShinraTenseiProjectile's expanding ring),
	// then it kills itself. One subclass per visual family (fire/lightning/genjutsu/etc.) since
	// ModProjectile ties one texture per type - see AnimalMinionProjectile for the same
	// base-class-with-thin-subclasses pattern used for minions.
	public abstract class BurstEffectProjectile : ModProjectile
	{
		protected abstract int FrameCount { get; }
		protected abstract int LifetimeTicks { get; }
		protected abstract int CanvasSize { get; }

		public sealed override void SetDefaults()
		{
			Projectile.width = CanvasSize;
			Projectile.height = CanvasSize;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = LifetimeTicks;
			Main.projFrames[Projectile.type] = FrameCount;
		}

		public sealed override bool? CanDamage() => false;

		public sealed override bool? CanCutTiles() => false;

		public override void AI()
		{
			int elapsed = LifetimeTicks - Projectile.timeLeft;
			Projectile.frame = Utils.Clamp(elapsed * FrameCount / LifetimeTicks, 0, FrameCount - 1);
		}
	}
}
