using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Replaces DustID.Stone bursts (ElementalBolt Earth).
	public class EarthBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 18;
		protected override int CanvasSize => 26;
	}
}
