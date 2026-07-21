using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Replaces DustID.Electric bursts (Chidori).
	public class LightningBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 4;
		protected override int LifetimeTicks => 14;
		protected override int CanvasSize => 24;
	}
}
