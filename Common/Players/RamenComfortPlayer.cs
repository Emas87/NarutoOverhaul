using NarutoOverhaul.Content.Buffs;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// The actual regen bonus for RamenComfortBuff (RamenStandTile's proximity effect) - the buff
	// class itself is just a marker/timer, per this codebase's established ChakraRegenBuff pattern
	// (effect applied in a paired ModPlayer, not the ModBuff itself).
	public class RamenComfortPlayer : ModPlayer
	{
		private const int LifeRegenBonus = 2;

		public override void UpdateLifeRegen()
		{
			if (Player.HasBuff(ModContent.BuffType<RamenComfortBuff>()))
			{
				Player.lifeRegen += LifeRegenBonus;
			}
		}
	}
}
