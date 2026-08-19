using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Iron Leg's body-covering kick VFX - a downward-arcing energy crescent/shockwave wrapping the
	// lower half of the player, blunt and punchy to match its power-over-reach, heaviest-knockback
	// identity. Lifetime matches IronLegItem's 20-tick useAnimation so the blast plays out over
	// exactly the swing.
	public class IronLegBlastProjectile : PlayerAnchoredBurstEffectProjectile
	{
		protected override int FrameCount => 6;
		protected override int LifetimeTicks => 20;
		protected override int CanvasSize => 150;
	}
}
