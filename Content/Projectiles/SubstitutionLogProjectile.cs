using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// The log left behind by a Substitution dodge (see SubstitutionPlayer.FreeDodge): pops up from
	// the player's hit position, tumbles under gravity, lands, sits for a moment, then fades out.
	// Purely cosmetic like the BurstEffectProjectile family, but needs real tile-collision physics
	// (not just a synced-lifetime animation) so it convincingly falls to the ground instead of
	// floating in place - so it's a plain ModProjectile rather than a BurstEffectProjectile.
	public class SubstitutionLogProjectile : ModProjectile
	{
		// ~2 seconds fully visible (so the player actually sees the log land) plus a short fade.
		private const int TotalLifetime = 140;
		private const int FadeTicks = 20;
		private const float Gravity = 0.35f;
		private const float MaxFallSpeed = 9f;

		// Spawns overlapping the player's own hitbox - same spawn-clipping problem KunaiProjectile/
		// ExplosiveKunaiProjectile solve with this helper.
		private readonly DelayedTileCollide delayedTileCollide = new(clearanceDistance: 12f, fallbackTicks: 6);

		private int ticksAlive;
		private bool landed;

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 12;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = TotalLifetime;
		}

		public override bool? CanDamage() => false;

		public override bool? CanCutTiles() => false;

		public override void AI()
		{
			delayedTileCollide.Update(Projectile);
			ticksAlive++;

			if (!landed)
			{
				Projectile.velocity.Y = System.Math.Min(Projectile.velocity.Y + Gravity, MaxFallSpeed);
				// Tumble while airborne; the fall speed drives the spin rate.
				Projectile.rotation += 0.12f + Projectile.velocity.X * 0.04f;

				// Once tile collision has zeroed out the vertical velocity (after giving the initial
				// hop a few ticks to clear the apex), the log has come to rest on the ground.
				if (ticksAlive > 8 && Projectile.velocity.Y == 0f)
				{
					landed = true;
					Projectile.velocity = Vector2.Zero;
				}
			}

			if (Projectile.timeLeft <= FadeTicks)
			{
				Projectile.alpha = (int)MathHelper.Lerp(0, 255, 1f - Projectile.timeLeft / (float)FadeTicks);
			}
		}
	}
}
