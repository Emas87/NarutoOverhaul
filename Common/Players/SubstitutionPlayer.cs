using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// The classic log-swap: while the Substitution Scroll is equipped, the next hit is fully
	// dodged, leaving a puff of wood behind. Deterministic with a cooldown (tracked via the
	// cooldown debuff's own timer), unlike SharinganPlayer's passive RNG dodge - the two stack
	// as "guaranteed but rationed" vs "unreliable but free".
	public class SubstitutionPlayer : ModPlayer
	{
		public const int CooldownTicks = 45 * 60;

		public bool SubstitutionEquipped;

		public override void ResetEffects()
		{
			SubstitutionEquipped = false;
		}

		public override bool FreeDodge(Player.HurtInfo info)
		{
			if (!SubstitutionEquipped || Player.HasBuff(ModContent.BuffType<SubstitutionCooldownBuff>()))
			{
				return false;
			}

			ChakraVFX.SpawnBurst(Player.Center, DustID.WoodFurniture, 16, 1.2f, noGravity: false);
			// Kick away from the incoming hit instead of teleporting - no tile-clip risk.
			Player.velocity.X = -info.HitDirection * 6f;
			Player.velocity.Y = -4f;
			Player.SetImmuneTimeForAllTypes(60);
			Player.AddBuff(ModContent.BuffType<SubstitutionCooldownBuff>(), CooldownTicks);
			return true;
		}
	}
}
