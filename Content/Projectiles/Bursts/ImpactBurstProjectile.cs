using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Physical punch/kick impact flash - the Taijutsu counterpart to the elemental bursts (fire,
	// lightning, etc.): a white-hot core flash into an orange starburst, no element/chakra tint.
	public class ImpactBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 6;
		protected override int LifetimeTicks => 16;
		protected override int CanvasSize => 28;
	}
}
