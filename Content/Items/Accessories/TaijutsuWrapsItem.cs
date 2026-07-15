using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	public class TaijutsuWrapsItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(ModContent.GetInstance<TaijutsuDamageClass>()) += 0.1f;
			player.GetAttackSpeed(ModContent.GetInstance<TaijutsuDamageClass>()) += 0.1f;
		}
	}
}
