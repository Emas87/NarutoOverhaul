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

		// Story-gate for this form, e.g. "must have downed Pain" - default true for forms with no gate.
		public virtual bool IsUnlocked => true;

		// Most forms (Sage Mode, Tailed Beast Mode) only cost chakra. A form like Eight Gates
		// needs a real bodily cost on top of that - default 0 so existing forms are unaffected.
		public virtual float LifeDrainPerTick => 0f;

		public abstract void ApplyStatBoosts(Player player);
	}
}
