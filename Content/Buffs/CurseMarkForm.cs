using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Orochimaru's Cursed Seal: the risk/reward form - bigger boosts than Sage Mode, paid in
	// life instead of chakra. Unlocked per-player by consuming the Cursed Seal Fragment
	// (Orochimaru's drop), not by a story flag alone.
	public class CurseMarkForm : TransformationForm
	{
		public override string DisplayName => "Curse Mark";
		public override int BuffType => ModContent.BuffType<CurseMarkBuff>();
		public override int ActivationCost => 20;
		public override float ChakraDrainPerTick => 0f;

		// IsUnlocked is only ever read from ToggleForm (ProcessTriggers - client-side, local
		// player), so reading Main.LocalPlayer here is safe.
		public override bool IsUnlocked => Main.LocalPlayer.GetModPlayer<CurseMarkPlayer>().HasCurseMark;

		// The shared drain loop truncates to whole HP per tick, so a sub-1 rate has to be
		// time-sliced: 1 HP every 6th tick = 10 HP/s.
		public override float LifeDrainPerTick(Player player) => player.miscCounter % 6 == 0 ? 1f : 0f;

		public override void ApplyStatBoosts(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.4f;
			player.moveSpeed += 0.15f;
			player.statDefense += 10;
		}
	}
}
