using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Guy's Eight Gates (Night Guy) - not Rock Lee's, who only ever opens up to around the 5th
	// gate in canon. Guy is the one who opens all eight, and doing so is normally fatal - so
	// unlike the chakra-only forms, this one costs life per tick via LifeDrainPerTick instead of
	// (or in addition to) chakra, and auto-cancels rather than being allowed to actually kill the
	// player (see TransformationPlayer.PostUpdateMiscEffects).
	public class EightGatesForm : TransformationForm
	{
		public override string DisplayName => "Eight Gates";
		public override int BuffType => ModContent.BuffType<EightGatesBuff>();
		public override int ActivationChakraCost => 40;
		public override float ChakraDrainPerTick => 0f;
		public override float LifeDrainPerTick => 2f;
		public override bool IsUnlocked => StoryProgressSystem.DownedMadara;

		public override void ApplyStatBoosts(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 1.5f;
			player.moveSpeed += 1.0f;
			player.statDefense -= 10; // the body is being pushed past safe limits, not protected
		}
	}
}
