using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Obito's Kamui intangibility - a Hardmode mobility toggle to walk through walls/floors.
	// The entire "phase through tiles" effect is a single line in PreUpdateMovement: vanilla
	// already has a real, non-hacky "skip tile collision" code path used when the player is
	// submerged in Shimmer liquid (Player.ShimmerCollision with noCollision=true, driven by the
	// player.shimmering flag). Setting that flag ourselves each tick gets us noclip, plus - for
	// free - the near-total damage dodge and translucent fade vanilla already grants while
	// shimmering, both of which read as "intangible" without any extra code.
	public class KamuiPhaseForm : TransformationForm
	{
		public override string DisplayName => "Kamui Phase";
		public override int BuffType => ModContent.BuffType<KamuiPhaseBuff>();
		public override int ActivationCost => 10;
		public override float ChakraDrainPerTick => 0.15f;
		public override bool IsUnlocked => Main.hardMode;

		public override void ApplyStatBoosts(Player player)
		{
			// Shimmer movement integrates position at 0.375x velocity - a small speed bump keeps
			// phasing from feeling sluggish compared to normal movement.
			player.moveSpeed += 0.3f;
		}

		public override void PreUpdateMovement(Player player)
		{
			// player.shimmering is reset to false by vanilla at the end of every tick, so it has
			// to be re-asserted here every tick the form is active - that's how the flag is meant
			// to be driven, not a workaround.
			player.shimmering = true;
		}
	}
}
