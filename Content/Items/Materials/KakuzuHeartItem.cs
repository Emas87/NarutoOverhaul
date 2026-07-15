using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Materials
{
	// Dropped by Kakuzu (fittingly - he has multiple stolen hearts) - required to craft
	// PainSummonItem. First material in the Hardmode half of the chain.
	public class KakuzuHeartItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.maxStack = 999;
			Item.material = true;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
	}
}
