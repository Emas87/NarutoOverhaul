using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Gentle Fist's body-covering punch VFX - small precise chakra-point flares along the arm/torso,
	// thinner and more "surgical" than the kicks' big blasts, matching its armor-pen flavor.
	// Lifetime matches GentleFistItem's 18-tick useAnimation, the shortest swing in the kit.
	public class GentleFistBlastProjectile : PlayerAnchoredBurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 18;
		protected override int CanvasSize => 120;
	}
}
