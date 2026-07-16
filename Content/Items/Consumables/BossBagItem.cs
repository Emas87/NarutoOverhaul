using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Base for the mod's 7 Expert-mode boss bags (one per story boss) - each is a bonus consolation
	// prize (coins + extra crafting material), not a replacement for the boss's normal drop table,
	// matching vanilla's own boss bag convention. Registered via `ItemDropRule.BossBag(...)` in
	// each boss's ModifyNPCLoot, which itself only ever triggers in Expert/Master mode. Abstract, so
	// tModLoader's autoloader skips this and only registers the 7 concrete subclasses (same idiom
	// as AnimalMinionProjectile/NumberedChakraScrollItem).
	public abstract class BossBagItem : ModItem
	{
		protected abstract int MaterialItemType { get; }
		protected abstract int MaterialBonusAmount { get; }
		protected abstract long CoinValue { get; }

		public override string Texture => "NarutoOverhaul/Content/Items/Consumables/BossBagIcon";

		public override void SetStaticDefaults()
		{
			ItemID.Sets.BossBag[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.expert = true;
			Item.rare = ItemRarityID.Expert;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot)
		{
			itemLoot.Add(ItemDropRule.Coins(CoinValue, true));

			if (MaterialItemType > 0)
			{
				itemLoot.Add(ItemDropRule.Common(MaterialItemType, 1, MaterialBonusAmount, MaterialBonusAmount));
			}
		}
	}
}
