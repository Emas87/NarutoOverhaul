using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Vanilla Mana Crystal equivalent for Chakra - freely obtainable (sold by Tenten, no boss
	// gate), permanent, capped at ChakraPlayer.MaxChakraScrolls just like vanilla's 10-crystal cap.
	public class ChakraScrollItem : ModItem
	{
		public const float ChakraIncrease = 15f;

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 26;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.useTurn = true;
			Item.maxStack = 99;
			Item.consumable = true;
			Item.UseSound = SoundID.Item3;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
		}

		public override bool CanUseItem(Player player)
		{
			return player.GetModPlayer<ChakraPlayer>().ConsumedChakraScrolls < ChakraPlayer.MaxChakraScrolls;
		}

		public override bool? UseItem(Player player)
		{
			ChakraPlayer chakraPlayer = player.GetModPlayer<ChakraPlayer>();
			chakraPlayer.ConsumedChakraScrolls++;
			chakraPlayer.IncreaseBaseMaxChakra(ChakraIncrease);
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraIncrease", $"Permanently increases max Chakra by {ChakraIncrease}"));
			tooltips.Add(new TooltipLine(Mod, "Cap", $"Can only be used {ChakraPlayer.MaxChakraScrolls} times"));
		}
	}
}
