using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Armor
{
	// Pure vanity - same as LeafVillageHeadbandItem, Stone Village colors.
	[AutoloadEquip(EquipType.Head)]
	public class StoneVillageHeadbandItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}

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
