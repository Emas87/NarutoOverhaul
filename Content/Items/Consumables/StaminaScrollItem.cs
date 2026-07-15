using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Vanilla Life Crystal equivalent for Stamina - freely obtainable (sold by Tenten, no boss
	// gate), permanent, capped at StaminaPlayer.MaxStaminaScrolls just like vanilla's 10-crystal cap.
	public class StaminaScrollItem : ModItem
	{
		public const float StaminaIncrease = 15f;

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
			Item.rare = ItemRarityID.Orange;
		}

		public override bool CanUseItem(Player player)
		{
			return player.GetModPlayer<StaminaPlayer>().ConsumedStaminaScrolls < StaminaPlayer.MaxStaminaScrolls;
		}

		public override bool? UseItem(Player player)
		{
			StaminaPlayer staminaPlayer = player.GetModPlayer<StaminaPlayer>();
			staminaPlayer.ConsumedStaminaScrolls++;
			staminaPlayer.IncreaseBaseMaxStamina(StaminaIncrease);
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "StaminaIncrease", $"Permanently increases max Stamina by {StaminaIncrease}"));
			tooltips.Add(new TooltipLine(Mod, "Cap", $"Can only be used {StaminaPlayer.MaxStaminaScrolls} times"));
		}
	}
}
