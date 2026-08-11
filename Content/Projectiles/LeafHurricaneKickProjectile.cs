using NarutoOverhaul.Common.VFX;
using Terraria;

namespace NarutoOverhaul.Content.Projectiles
{
	public class LeafHurricaneKickProjectile : TaijutsuKickProjectile
	{
		// Widest sweep of the 3 kicks, matching its wide melee hitbox/reach.
		protected override float SweepLimitDegrees => 70f;
		protected override int SweepTicks => 26;
		// A wide-reach weapon doesn't need as much dash - it already covers ground via hitbox size.
		protected override float DashSpeed => 5f;

		protected override void OnKickHit(Player owner, NPC target)
		{
			ChakraVFX.SpawnImpactBurst(target.Center, 1.4f);
		}
	}
}
