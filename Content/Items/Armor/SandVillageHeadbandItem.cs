using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Armor
{
	// Pure vanity - same as LeafVillageHeadbandItem, Sand Village colors.
	[AutoloadEquip(EquipType.Head)]
	public class SandVillageHeadbandItem : ModItem
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
