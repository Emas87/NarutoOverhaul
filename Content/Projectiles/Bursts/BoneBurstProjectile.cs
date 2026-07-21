using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// White/bone-dimensional impact - replaces DustID.WhiteTorch (Kaguya) and DustID.Bone
	// (Ash Bone) bursts, both reading as the same "bone-white otherworldly" palette.
	public class BoneBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 20;
		protected override int CanvasSize => 28;
	}
}
