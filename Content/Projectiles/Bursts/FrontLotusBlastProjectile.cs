using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Front Lotus's body-covering kick VFX - a straight-line piercing energy thrust, narrow and
	// intense, matching its armor-penetration + longest dash-through lunge. Lifetime matches
	// FrontLotusItem's 22-tick useAnimation.
	public class FrontLotusBlastProjectile : PlayerAnchoredBurstEffectProjectile
	{
		protected override int FrameCount => 6;
		protected override int LifetimeTicks => 22;
		protected override int CanvasSize => 170;
	}
}
