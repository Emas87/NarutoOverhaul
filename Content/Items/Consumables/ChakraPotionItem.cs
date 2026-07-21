using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.VFX;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Instant Chakra restore - unlike vanilla Mana Potions, repeated use in a short window has
	// diminishing returns (see ChakraPlayer.GetPotionEffectivenessMultiplier/RegisterPotionUse),
	// closer in spirit to Healing Potions' sickness but stacking instead of a hard lockout.
	public class ChakraPotionItem : ModItem
	{
		public const float RestoreAmount = 50f;

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 26;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.useTurn = true;
			Item.maxStack = 30;
			Item.consumable = true;
			Item.UseSound = SoundID.Item3;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.White;
		}

		public override bool CanUseItem(Player player)
		{
			ChakraPlayer chakraPlayer = player.GetModPlayer<ChakraPlayer>();
			return chakraPlayer.Chakra < chakraPlayer.MaxChakra;
		}

		public override bool? UseItem(Player player)
		{
			ChakraPlayer chakraPlayer = player.GetModPlayer<ChakraPlayer>();
			float actualRestore = RestoreAmount * chakraPlayer.GetPotionEffectivenessMultiplier();
			chakraPlayer.Chakra = System.Math.Min(chakraPlayer.MaxChakra, chakraPlayer.Chakra + actualRestore);
			chakraPlayer.RegisterPotionUse();
			ChakraVFX.SpawnChakraBurst(player.Center, 1f);
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "RestoreAmount", $"Restores {RestoreAmount} Chakra"));
			tooltips.Add(new TooltipLine(Mod, "PotionSickness", "Repeated use in a short time restores less, down to nothing"));
		}
	}
}
