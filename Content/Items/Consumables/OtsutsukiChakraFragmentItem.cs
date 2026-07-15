using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Kaguya's signature drop - the progenitor of chakra's power, condensed into a single dose.
	// A permanent, one-time blessing rather than gear: once consumed it can never be used again
	// (tracked by MoonLordBlessingPlayer), unlike an accessory that could be swapped in only for
	// the fight. Its headline effect (weakening Moon Lord specifically) lives in
	// Common/GlobalNPCs/MoonLordWeakenGlobalNPC.cs.
	public class OtsutsukiChakraFragmentItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useTurn = true;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.UseSound = SoundID.Item29;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Purple;
		}

		public override bool CanUseItem(Player player)
		{
			return !player.GetModPlayer<MoonLordBlessingPlayer>().HasKaguyaBlessing;
		}

		public override bool? UseItem(Player player)
		{
			player.GetModPlayer<MoonLordBlessingPlayer>().HasKaguyaBlessing = true;
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "OneTime", "Can only be consumed once - its blessing is permanent"));
			tooltips.Add(new TooltipLine(Mod, "MoonLordNote", "The Moon Lord's power pales before hers"));
		}
	}
}
