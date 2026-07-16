using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Ichiraku-style bowl of ramen - standard vanilla food item shape (Item.buffType/buffTime is
	// the entire mechanic; no custom code needed), granting the vanilla Well Fed buff like any
	// other food.
	public class RamenItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 20;
			Item.useStyle = ItemUseStyleID.EatFood;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.UseSound = SoundID.Item2;
			Item.consumable = true;
			Item.maxStack = 30;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 3600 * 10; // 10 minutes, matching vanilla's low-tier foods
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.White;
		}
	}
}
