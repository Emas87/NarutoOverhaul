using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	// Kaguya's signature drop - the progenitor of chakra's power made wearable. Its headline
	// effect (weakening Moon Lord specifically) lives in Common/GlobalNPCs/MoonLordWeakenGlobalNPC.cs;
	// this item is otherwise just a strong postgame accessory.
	public class OtsutsukiChakraFragmentItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Purple;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.15f;
			player.statDefense += 8;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "MoonLordNote", "The Moon Lord's power pales before hers"));
		}
	}
}
