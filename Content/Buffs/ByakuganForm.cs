using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// The Hyuga all-seeing eye as a cheap utility form: vanilla already has three "reveal"
	// flags (Hunter/Dangersense/Spelunker), so the entire effect is setting them each tick,
	// plus a small crit bonus for the chakra-point-targeting flavor.
	public class ByakuganForm : TransformationForm
	{
		public override string DisplayName => "Byakugan";
		public override int BuffType => ModContent.BuffType<ByakuganBuff>();
		public override int ActivationCost => 10;
		public override float ChakraDrainPerTick => 0.1f;
		public override bool IsUnlocked => StoryProgressSystem.DownedShukaku;

		public override void ApplyStatBoosts(Player player)
		{
			player.detectCreature = true;
			player.dangerSense = true;
			player.findTreasure = true;
			player.GetCritChance(DamageClass.Generic) += 8;
		}
	}
}
