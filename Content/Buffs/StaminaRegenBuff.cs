using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Marker/timer only - the actual regen-rate bonus is applied in StaminaPlayer.ResetEffects,
	// right after StaminaRegenRate is staged from BaseStaminaRegenRate each frame.
	public class StaminaRegenBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = false;
		}
	}
}
