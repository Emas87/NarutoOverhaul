using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.VFX;
using Terraria;

namespace NarutoOverhaul.Content.Projectiles
{
	public class LeafHurricaneKickProjectile : TaijutsuKickProjectile
	{
		// Widest sweep of the 3 kicks, matching its wide melee hitbox/reach.
		protected override float SweepLimitDegrees => 70f;
		protected override int SweepTicks => 26;
		// Widest hit-hitbox in the kit, matching the item's existing 44x44 nominal box and its
		// spin-kick's actual reach.
		protected override int HitboxSize => 60;
		// A wide-reach weapon doesn't need as much dash - it already covers ground via hitbox size.
		protected override float DashSpeed => 7f;

		protected override void OnKickHit(Player owner, NPC target)
		{
			ChakraVFX.SpawnImpactBurst(target.Center, 1.8f);
			ChakraVFX.SpawnDirectionalBurst(target.Center, new Vector2(owner.direction, -0.2f), speed: 5f, scale: 1.3f);
		}
	}
}
