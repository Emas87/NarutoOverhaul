using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Purple genjutsu/dark-chakra impact - replaces DustID.PurpleTorch bursts (Genjutsu jutsu
	// hits, Shinra Tensei's own cast flash, Susanoo, Madara, Pain).
	public class GenjutsuBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 20;
		protected override int CanvasSize => 28;
	}
}
