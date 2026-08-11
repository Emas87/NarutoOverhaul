using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	public class SageModeForm : TransformationForm
	{
		public override string DisplayName => "Sage Mode";
		public override int BuffType => ModContent.BuffType<SageModeBuff>();
		public override int ActivationCost => 30;
		public override float ChakraDrainPerTick => 0.3f;
		public override bool IsUnlocked(Player player) => StoryProgressSystem.DownedPain;

		public override void ApplyStatBoosts(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.3f;
			player.moveSpeed += 0.2f;
		}
	}
}
