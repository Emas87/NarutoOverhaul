using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	// A chakra nature-affinity training tool - boosts Ninjutsu damage and crit chance.
	public class ChakraPaperItem : ModItem
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
			player.GetDamage(ModContent.GetInstance<NinjutsuDamageClass>()) += 0.1f;
			player.GetCritChance(ModContent.GetInstance<NinjutsuDamageClass>()) += 8f;
		}
	}
}
