using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Base for the 7 story-order Chakra Scrolls (ChakraScroll1Item..ChakraScroll7Item), one
	// dropped by each story boss in kill order. Strictly sequential: scroll N can only be
	// consumed once ChakraPlayer.ConsumedChakraScrolls == N-1, reusing that counter as the
	// sequencing gate rather than adding a separate flag per scroll. Abstract, so tModLoader's
	// autoloader skips this and only registers the 7 concrete subclasses (same idiom as
	// AnimalMinionProjectile).
	public abstract class NumberedChakraScrollItem : ModItem
	{
		public const float ChakraIncreasePerScroll = 20f;

		protected abstract int ScrollNumber { get; }

		public override string Texture => "NarutoOverhaul/Content/Items/Consumables/ChakraCrystal";

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
			return player.GetModPlayer<ChakraPlayer>().ConsumedChakraScrolls == ScrollNumber - 1;
		}

		public override bool? UseItem(Player player)
		{
			ChakraPlayer chakraPlayer = player.GetModPlayer<ChakraPlayer>();
			chakraPlayer.ConsumedChakraScrolls = ScrollNumber;
			chakraPlayer.IncreaseBaseMaxChakra(ChakraIncreasePerScroll);
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraIncrease", $"Permanently increases max Chakra by {ChakraIncreasePerScroll}"));
			tooltips.Add(new TooltipLine(Mod, "Sequence", ScrollNumber == 1
				? $"Scroll {ScrollNumber} of {ChakraPlayer.MaxChakraScrolls}"
				: $"Scroll {ScrollNumber} of {ChakraPlayer.MaxChakraScrolls} - requires having read Scroll {ScrollNumber - 1} first"));
		}
	}
}
