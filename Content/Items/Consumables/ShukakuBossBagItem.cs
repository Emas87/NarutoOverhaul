using Terraria;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Items.Materials;

namespace NarutoOverhaul.Content.Items.Consumables
{
	public class ShukakuBossBagItem : BossBagItem
	{
		protected override int MaterialItemType => ModContent.ItemType<SandCoreItem>();
		protected override int MaterialBonusAmount => 5;
		protected override long CoinValue => Item.buyPrice(gold: 8);
	}
}
