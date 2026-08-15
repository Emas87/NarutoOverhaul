using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	// A training-weights reference (Rock Lee/Guy) - boosts Taijutsu damage and max Stamina.
	public class WeightedLegWarmersItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(ModContent.GetInstance<TaijutsuDamageClass>()) += 0.1f;

			// MaxStamina, not BaseMaxStamina - the latter is the persistent value only meant to
			// grow via consumables; UpdateEquip runs every tick, so adding there must target the
			// frame-staged value (reset from BaseMaxStamina each frame in StaminaPlayer.ResetEffects).
			player.GetModPlayer<StaminaPlayer>().MaxStamina += 20f;
		}
	}
}
