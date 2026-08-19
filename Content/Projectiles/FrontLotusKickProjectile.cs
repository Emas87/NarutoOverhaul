using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.VFX;
using Terraria;

namespace NarutoOverhaul.Content.Projectiles
{
	// The Hardmode flagship kick - biggest arc, strongest dash-through, and the only kick with a
	// mid-swing VFX beat (not just on hit), matching Lee/Guy's Primary Lotus actually driving the
	// target into the ground rather than a single flat strike.
	public class FrontLotusKickProjectile : TaijutsuKickProjectile
	{
		public const float ArmorPenetrationAmount = 25f;

		protected override float SweepLimitDegrees => 50f;
		protected override int SweepTicks => 22;
		// Longest reach in the kit - the widest hit-hitbox to match.
		protected override int HitboxSize => 50;
		protected override float DashSpeed => 13f;

		private bool peakBurstFired;

		protected override void ModifyKickHit(ref NPC.HitModifiers modifiers)
		{
			modifiers.ArmorPenetration += ArmorPenetrationAmount;
			modifiers.Knockback += 0.5f;
		}

		protected override void OnSweep(Player owner, float progress)
		{
			if (!peakBurstFired && progress >= 0.5f)
			{
				peakBurstFired = true;
				ChakraVFX.SpawnImpactBurst(Projectile.Center, 1.3f);
			}
		}

		protected override void OnKickHit(Player owner, NPC target)
		{
			ChakraVFX.SpawnImpactBurst(target.Center, 2.6f);
			ChakraVFX.SpawnImpactBurst(target.Center + new Vector2(0f, 12f), 1.7f, 0.3f);
			ChakraVFX.SpawnEarthBurst(target.Center + new Vector2(0f, 16f), 1.8f);
			ChakraVFX.SpawnDirectionalBurst(target.Center, new Vector2(owner.direction, -0.2f), speed: 7f, scale: 1.5f);
		}
	}
}
