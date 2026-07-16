using Terraria;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Items.Materials;

namespace NarutoOverhaul.Content.Items.Consumables
{
	public class PainBossBagItem : BossBagItem
	{
		protected override int MaterialItemType => ModContent.ItemType<RinneganFragmentItem>();
		protected override int MaterialBonusAmount => 5;
		protected override long CoinValue => Item.buyPrice(gold: 25);
	}
}
