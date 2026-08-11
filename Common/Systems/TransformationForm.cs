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
		public abstract int ActivationCost { get; }
		public abstract float ChakraDrainPerTick { get; }

		// Story-gate for this form, e.g. "must have downed Pain" - default true for forms with no
		// gate. Takes Player (like LifeDrainPerTick below) rather than being a static property,
		// since a per-player gate (e.g. Curse Mark's one-time consumable unlock) needs to check the
		// specific player being evaluated, not whichever player happens to be local.
		public virtual bool IsUnlocked(Player player) => true;

		// Most forms (Sage Mode, Tailed Beast Mode) only cost chakra. A form like Eight Gates
		// needs a real bodily cost on top of that, scaled by per-player state (gate level) - hence
		// a method taking Player rather than a static property. Default 0 so existing forms are unaffected.
		public virtual float LifeDrainPerTick(Player player) => 0f;

		public abstract void ApplyStatBoosts(Player player);

		// Most forms only need passive stats (ApplyStatBoosts, applied from ResetEffects - fine for
		// things like moveSpeed since vanilla's own movement code reads them later in the frame).
		// Chakra Control's wall-climb needs to override velocity/gravity directly, which has to
		// happen from ModPlayer.PreUpdateMovement (before vanilla's own movement/gravity resolves)
		// or it gets overwritten - hence a separate hook. Default no-op so existing forms are unaffected.
		public virtual void PreUpdateMovement(Player player) { }
	}
}
