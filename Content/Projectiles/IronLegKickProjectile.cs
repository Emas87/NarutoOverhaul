using NarutoOverhaul.Common.VFX;
using Terraria;

namespace NarutoOverhaul.Content.Projectiles
{
	public class IronLegKickProjectile : TaijutsuKickProjectile
	{
		protected override float SweepLimitDegrees => 40f;
		protected override int SweepTicks => 20;
		protected override float DashSpeed => 7f;

		protected override void ModifyKickHit(ref NPC.HitModifiers modifiers)
		{
			// Shoot() already gates spawning this projectile on a successful stamina spend, so the
			// bonus always applies now (the item used to track a staminaSpent flag to guard this -
			// no longer needed once the spend check moved to Shoot()).
			modifiers.Knockback *= 1.5f;
		}

		protected override void OnKickHit(Player owner, NPC target)
		{
			ChakraVFX.SpawnImpactBurst(target.Center, 1.7f);
			// Ground-impact-style secondary flash (distinct sprite from the plain impact burst) -
			// sells the "heavy stomping kick" weight.
			ChakraVFX.SpawnEarthBurst(target.Center, 1.1f);
		}
	}
}
