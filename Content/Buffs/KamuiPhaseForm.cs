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
		public override bool IsUnlocked(Player player) => Main.hardMode;

		public override void ApplyStatBoosts(Player player)
		{
			// Shimmer movement integrates position at only 0.375x velocity (Player.ShimmerCollision),
			// so a +0.3 moveSpeed bump (the original tuning here) still nets out to roughly half of
			// normal walking speed after that dampening - which read as "stuck against a wall" in
			// testing. +2.2 nets out to modestly faster than normal walking once the 0.375x is
			// applied, so phasing actually feels like a deliberate fast dash through the wall.
			player.moveSpeed += 2.2f;
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
