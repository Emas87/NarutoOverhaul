using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Armor
{
	// Pure vanity - the iconic forehead protector, no stats. A quick, low-risk single vanity item
	// rather than a full per-village headband set (descoped for the same reason as trophies/relics
	// - decorative value doesn't justify much more scope here).
	[AutoloadEquip(EquipType.Head)]
	public class LeafVillageHeadbandItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.accessory = true;
			Item.vanity = true;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Blue;
		}
	}
}
