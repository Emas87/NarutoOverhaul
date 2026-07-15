using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// Sasuke's 2-tomoe Sharingan: a passive chance to fully avoid a hit and gain a brief
	// counter-damage window. Unlocked after the Haku fight (canonically awakens there).
	public class SharinganPlayer : ModPlayer
	{
		public const float DodgeChance = 0.15f;
		public const int FocusDuration = 180; // 3 seconds
		public const float FocusDamageBonus = 0.15f;

		public override bool FreeDodge(Player.HurtInfo info)
		{
			if (!StoryProgressSystem.DownedHaku || Main.rand.NextFloat() >= DodgeChance)
			{
				return false;
			}

			ChakraVFX.SpawnBurst(Player.Center, DustID.RedTorch, 6, 1f);
			Player.AddBuff(ModContent.BuffType<SharinganFocusBuff>(), FocusDuration);
			return true;
		}

		public override void ResetEffects()
		{
			if (Player.HasBuff(ModContent.BuffType<SharinganFocusBuff>()))
			{
				Player.GetDamage(DamageClass.Generic) += FocusDamageBonus;
			}
		}
	}
}
