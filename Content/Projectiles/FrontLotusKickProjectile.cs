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
		protected override float DashSpeed => 11f;

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
			ChakraVFX.SpawnImpactBurst(target.Center, 2.2f);
			ChakraVFX.SpawnImpactBurst(target.Center + new Vector2(0f, 12f), 1.4f, 0.3f);
			ChakraVFX.SpawnEarthBurst(target.Center + new Vector2(0f, 16f), 1.5f);
		}
	}
}
