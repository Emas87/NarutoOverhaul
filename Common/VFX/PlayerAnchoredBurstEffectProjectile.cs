using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.VFX
{
	// Sibling to BurstEffectProjectile for VFX that needs to visibly track the player through a
	// swing instead of sitting still at a fixed spawn point - the Taijutsu kick/punch "blast"
	// effects that are meant to cover most of the player's body need to stay centered on them (with
	// a facing-direction offset) for their whole lifetime, not just flash once where they spawned.
	// Purely cosmetic, same as BurstEffectProjectile: no damage, no collision.
	public abstract class PlayerAnchoredBurstEffectProjectile : ModProjectile
	{
		protected abstract int FrameCount { get; }
		protected abstract int LifetimeTicks { get; }
		protected abstract int CanvasSize { get; }

		// Offset from the owner's center, in the owner's un-flipped facing direction (+X = in front
		// of the player) - flipped automatically per-tick by the owner's actual direction below.
		private Vector2 Offset => new(Projectile.ai[0], Projectile.ai[1]);

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
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}

			Vector2 offset = Offset;
			offset.X *= owner.direction;
			Projectile.Center = owner.Center + offset;
			Projectile.spriteDirection = owner.direction;

			int elapsed = LifetimeTicks - Projectile.timeLeft;
			Projectile.frame = Utils.Clamp(elapsed * FrameCount / LifetimeTicks, 0, FrameCount - 1);
		}
	}
}
