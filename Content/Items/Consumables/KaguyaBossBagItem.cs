using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// No material bonus (Kaguya's own drop, OtsutsukiChakraFragmentItem, is a one-time consumable
	// that a bag copy couldn't ever use) - just a bigger coin reward, matching the final boss's tier.
	public class KaguyaBossBagItem : BossBagItem
	{
		protected override int MaterialItemType => -1;
		protected override int MaterialBonusAmount => 0;
		protected override long CoinValue => Item.buyPrice(gold: 50);
	}
}
