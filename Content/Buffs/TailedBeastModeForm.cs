using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Second entry in the transformation registry - unlocked after Madara, stronger and hungrier
	// than Sage Mode. Adding this required exactly one new subclass + one registration line in
	// TransformationSystem, no changes to TransformationPlayer's core toggle/drain/draw logic.
	public class TailedBeastModeForm : TransformationForm
	{
		public override string DisplayName => "Tailed Beast Mode";
		public override int BuffType => ModContent.BuffType<TailedBeastModeBuff>();
		public override int ActivationCost => 50;
		public override float ChakraDrainPerTick => 0.6f;
		public override bool IsUnlocked => StoryProgressSystem.DownedMadara;

		public override void ApplyStatBoosts(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.6f;
			player.moveSpeed += 0.35f;
			player.statDefense += 10;
		}
	}
}
