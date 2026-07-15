using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Guy's Eight Gates (Night Guy) - not Rock Lee's, who only ever opens up to around the 5th
	// gate in canon. Guy is the one who opens all eight, and doing so is normally fatal. This is
	// a leveled form (see TransformationPlayer.EightGatesLevel, 1-8) rather than a flat toggle:
	// gates 1-7 scale up power and life-drain but stay survivable (auto-cancel near 0 HP, same as
	// before), while reaching gate 8 is a deliberate, guaranteed-lethal choice handled directly in
	// TransformationPlayer (a windup then Player.KillMe) - there is no safety net at gate 8.
	public class EightGatesForm : TransformationForm
	{
		public override string DisplayName => "Eight Gates";
		public override int BuffType => ModContent.BuffType<EightGatesBuff>();
		public override int ActivationChakraCost => 40;
		public override float ChakraDrainPerTick => 0f;
		public override bool IsUnlocked => StoryProgressSystem.DownedMadara;

		public override float LifeDrainPerTick(Player player)
		{
			int level = player.GetModPlayer<TransformationPlayer>().EightGatesLevel;
			return level * 1.5f;
		}

		public override void ApplyStatBoosts(Player player)
		{
			int level = player.GetModPlayer<TransformationPlayer>().EightGatesLevel;

			player.GetDamage(DamageClass.Generic) += 0.2f * level;
			player.moveSpeed += 0.15f * level;
			player.statDefense -= 2 * level; // the body is being pushed past safe limits, not protected
		}
	}
}
