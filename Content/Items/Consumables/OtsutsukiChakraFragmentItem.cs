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
	// the fight. Its headline effect (homing assist bolts during the Moon Lord fight specifically)
	// lives in Common/GlobalNPCs/MoonLordAssistGlobalNPC.cs - deliberately never spelled out in the
	// tooltip below.
	public class OtsutsukiChakraFragmentItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
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

		// No hint at what the blessing actually does or who it's for (see MoonLordAssistGlobalNPC) -
		// only that consuming it is permanent and irreversible, same as any other mysterious relic.
		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "OneTime", "Can only be consumed once - its blessing is permanent"));
		}
	}
}
