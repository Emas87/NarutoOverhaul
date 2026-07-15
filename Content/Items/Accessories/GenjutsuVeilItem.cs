using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	// Distinct from IllusionCharmItem (damage + control duration): the Veil is about concealment
	// rather than potency - it reduces enemy aggro via the vanilla stealth-camo stat (same field
	// Shroomite armor uses), fitting "veil" more literally than a second copy of Illusion Charm's effect.
	public class GenjutsuVeilItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.aggro -= 400;
		}
	}
}
