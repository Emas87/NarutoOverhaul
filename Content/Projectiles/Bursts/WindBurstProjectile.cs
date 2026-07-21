using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Replaces DustID.Cloud bursts (Great Breakthrough, ElementalBolt Wind).
	public class WindBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 18;
		protected override int CanvasSize => 28;
	}
}
