using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Default chakra/ninjutsu impact - replaces plain DustID.BlueTorch bursts (Rasengan hit,
	// Chakra Potion use, Tailed Beast Mode ambient pulse).
	public class ChakraBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 20;
		protected override int CanvasSize => 28;
	}
}
