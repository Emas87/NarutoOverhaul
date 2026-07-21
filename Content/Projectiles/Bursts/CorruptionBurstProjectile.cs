using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Replaces DustID.Corruption bursts (Orochimaru, Snake minion/projectile).
	public class CorruptionBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 20;
		protected override int CanvasSize => 28;
	}
}
