using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Genjutsu Nightmare's body-covering cast VFX - a warping violet eye-rift with shadowy tendrils
	// curling outward, reading as an illusion/dread pulse rather than a physical blast (unlike the
	// Taijutsu kicks' punchy chakra-energy blasts). Lifetime matches GenjutsuNightmareItem's 30-tick
	// useAnimation, the longest single-swing cast in the mod.
	public class GenjutsuNightmareBlastProjectile : PlayerAnchoredBurstEffectProjectile
	{
		protected override int FrameCount => 6;
		protected override int LifetimeTicks => 30;
		protected override int CanvasSize => 150;
	}
}
