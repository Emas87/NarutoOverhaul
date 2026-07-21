using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Replaces DustID.Shadowflame bursts (ElementalBolt Core/default - Kakuzu's 5th mask).
	public class CoreBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 6;
		protected override int LifetimeTicks => 20;
		protected override int CanvasSize => 26;
	}
}
