using NarutoOverhaul.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Placeable
{
	public class RamenStandItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 20;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<RamenStandTile>();
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.White;
		}
	}
}
