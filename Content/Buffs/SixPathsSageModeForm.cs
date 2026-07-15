using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Third registry entry - the postgame reward for defeating Kaguya, the strongest form.
	// Same story as TailedBeastModeForm: one subclass, one buff, one registration line.
	public class SixPathsSageModeForm : TransformationForm
	{
		public override string DisplayName => "Six Paths Sage Mode";
		public override int BuffType => ModContent.BuffType<SixPathsSageModeBuff>();
		public override int ActivationChakraCost => 60;
		public override float ChakraDrainPerTick => 0.5f;
		public override bool IsUnlocked => StoryProgressSystem.DownedKaguya;

		public override void ApplyStatBoosts(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.9f;
			player.moveSpeed += 0.5f;
			player.statDefense += 15;
		}
	}
}
