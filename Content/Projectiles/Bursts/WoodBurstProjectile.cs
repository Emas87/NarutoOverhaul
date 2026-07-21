using NarutoOverhaul.Common.VFX;

namespace NarutoOverhaul.Content.Projectiles.Bursts
{
	// Replaces DustID.WoodFurniture bursts (Substitution log swap).
	public class WoodBurstProjectile : BurstEffectProjectile
	{
		protected override int FrameCount => 5;
		protected override int LifetimeTicks => 20;
		protected override int CanvasSize => 28;
	}
}
