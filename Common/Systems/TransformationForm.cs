using Terraria;

namespace NarutoOverhaul.Common.Systems
{
	// Data-driven "form" contract: adding a new transformation later (Tailed Beast Mode, Six Paths
	// Sage Mode, ...) means one new subclass + one registration call in TransformationSystem -
	// no changes to TransformationPlayer's toggle/drain/draw-layer logic.
	public abstract class TransformationForm
	{
		public abstract string DisplayName { get; }
		public abstract int BuffType { get; }
		public abstract int ActivationChakraCost { get; }
		public abstract float ChakraDrainPerTick { get; }

		public abstract void ApplyStatBoosts(Player player);
	}
}
