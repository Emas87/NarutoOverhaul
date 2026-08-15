using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	// Boosts Genjutsu damage and extends control/fear/sleep duration - reads the same
	// GenjutsuControlDurationBonus already wired into every Genjutsu item's debuff duration.
	public class IllusionCharmItem : ModItem
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
			player.GetDamage(ModContent.GetInstance<GenjutsuDamageClass>()) += 0.1f;
			player.GetModPlayer<ChakraPlayer>().GenjutsuControlDurationBonus += 60; // +1 second
		}
	}
}
