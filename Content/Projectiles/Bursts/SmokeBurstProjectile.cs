using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Replaces DustID.Smoke bursts (Shadow Clone spawn/poof).
	public class SmokeBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 22;
		protected override int CanvasSize => 28;
	}
}
