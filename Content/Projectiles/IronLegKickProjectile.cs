using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.VFX;
using Terraria;

namespace NarutoOverhaul.Content.Projectiles
{
	public class IronLegKickProjectile : TaijutsuKickProjectile
	{
		protected override float SweepLimitDegrees => 40f;
		protected override int SweepTicks => 20;
		// Heaviest base knockback in the kit gets the strongest hit-hitbox too, so a heavy stomp
		// actually connects at the range its new blast VFX implies instead of whiffing.
		protected override int HitboxSize => 44;
		protected override float DashSpeed => 9f;

		protected override void ModifyKickHit(ref NPC.HitModifiers modifiers)
		{
			// Shoot() already gates spawning this projectile on a successful stamina spend, so the
			// bonus always applies now (the item used to track a staminaSpent flag to guard this -
			// no longer needed once the spend check moved to Shoot()).
			modifiers.Knockback *= 1.5f;
		}

		protected override void OnKickHit(Player owner, NPC target)
		{
			ChakraVFX.SpawnImpactBurst(target.Center, 2.1f);
			// Ground-impact-style secondary flash (distinct sprite from the plain impact burst) -
			// sells the "heavy stomping kick" weight.
			ChakraVFX.SpawnEarthBurst(target.Center, 1.4f);
			// Dust kicked away in the knockback direction, not just a static flash at the hit point -
			// sells the "great knockback" identity even in a still screenshot.
			ChakraVFX.SpawnDirectionalBurst(target.Center, new Vector2(owner.direction, -0.2f), speed: 6f, scale: 1.4f);
		}
	}
}
