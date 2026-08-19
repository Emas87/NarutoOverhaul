using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Leaf Hurricane's body-covering kick VFX - a full rotating energy ring/vortex around the
	// player's lower body, matching its wide spin-kick sweep. Lifetime matches LeafHurricaneItem's
	// 26-tick useAnimation, the longest swing in the kit.
	public class LeafHurricaneBlastProjectile : PlayerAnchoredBurstEffectProjectile
	{
		protected override int FrameCount => 8;
		protected override int LifetimeTicks => 26;
		protected override int CanvasSize => 180;
	}
}
