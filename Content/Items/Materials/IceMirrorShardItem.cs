using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Materials
{
	// Dropped by Haku - required to craft ShukakuSummonItem, the first link in the boss
	// material-progression chain.
	public class IceMirrorShardItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.maxStack = 999;
			Item.material = true;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
		}
	}
}
