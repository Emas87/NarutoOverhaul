using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	// Wall of Flesh drop, alongside vanilla's own Warrior/Ranger/Sorcerer/Summoner Emblem - same
	// single-stat +15% class damage convention as those, just for Genjutsu.
	public class GenjutsuEmblemItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Pink;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(ModContent.GetInstance<GenjutsuDamageClass>()) += 0.15f;
		}
	}
}
