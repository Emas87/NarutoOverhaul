using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Purely informational - the actual reduced-potion-effectiveness logic already lives in
	// StaminaPlayer.GetPotionEffectivenessMultiplier/RegisterPotionUse (a silent stacking penalty
	// with no player-facing indication at all before this). Shows the player WHY Stamina potions
	// feel weaker right now and, via its own normal countdown timer, exactly how long until it
	// clears - unlike RamenComfortBuff this timer is real and meaningful, so it's shown rather than
	// hidden.
	public class StaminaPotionSicknessDebuff : ModBuff
	{
		// No dedicated art - reuses the Stamina Potion's own icon.
		public override string Texture => "NarutoOverhaul/Content/Items/Consumables/StaminaPotionItem";

		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true; // re-applied fresh by RegisterPotionUse on the next drink anyway
		}
	}
}
