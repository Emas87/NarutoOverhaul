using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Replaces DustID.HealingPlus bursts (Pain's healing path).
	public class HealingBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 18;
		protected override int CanvasSize => 28;
	}
}
