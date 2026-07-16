using Terraria;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Items.Materials;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Expert-mode bonus bag - dropped alongside Haku's normal loot table, never replacing it.
	public class HakuBossBagItem : BossBagItem
	{
		protected override int MaterialItemType => ModContent.ItemType<IceMirrorShardItem>();
		protected override int MaterialBonusAmount => 5;
		protected override long CoinValue => Item.buyPrice(gold: 5);
	}
}
