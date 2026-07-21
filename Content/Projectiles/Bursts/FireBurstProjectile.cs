using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Replaces DustID.Torch bursts (Fireball, Stamina Potion, Kakuzu's fire mask, transformations).
	public class FireBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 18;
		protected override int CanvasSize => 28;
	}
}
