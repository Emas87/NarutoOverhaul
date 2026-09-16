using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Common.Players
{
	// Sasuke's 2-tomoe Sharingan: a passive chance to fully avoid a hit and gain a brief
	// counter-damage window. Previously auto-unlocked on downing Haku with no player-facing
	// signal at all - now requires actually consuming SharinganAwakeningItem, so there's a visible
	// moment (and tooltip) telling the player they gained it, same reasoning as the *MasteryScroll
	// items over ClassMasteryPlayer's flags. Once set, HasSharingan is permanent like those flags.
	public class SharinganPlayer : ModPlayer
	{
		public const float DodgeChance = 0.15f;
		public const int FocusDuration = 180; // 3 seconds
		public const float FocusDamageBonus = 0.15f;

		public bool HasSharingan;

		public override bool FreeDodge(Player.HurtInfo info)
		{
			if (!HasSharingan || Main.rand.NextFloat() >= DodgeChance)
			{
				return false;
			}

			ChakraVFX.SpawnBurstEffect<SharinganBurstProjectile>(Player.Center, 0.75f);
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

		public override void SaveData(TagCompound tag)
		{
			tag["hasSharingan"] = HasSharingan;
		}

		public override void LoadData(TagCompound tag)
		{
			HasSharingan = tag.GetBool("hasSharingan");
		}
	}
}
