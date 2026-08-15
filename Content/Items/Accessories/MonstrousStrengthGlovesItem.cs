using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	// References Tsunade/Sakura's iconic chakra-enhanced strength (canonically used to shatter
	// rock and ground) - applied here to mining and tree-chopping rather than combat. Purely
	// passive QoL, no toggle/keybind needed.
	public class MonstrousStrengthGlovesItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}

		public override void UpdateEquip(Player player)
		{
			player.pickSpeed -= 0.15f;
			player.blockRange += 3;
		}
	}
}
