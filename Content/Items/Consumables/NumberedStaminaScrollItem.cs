using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Base for the 7 story-order Stamina Scrolls (StaminaScroll1Item..StaminaScroll7Item), one
	// dropped by each story boss in kill order. Strictly sequential: scroll N can only be
	// consumed once StaminaPlayer.ConsumedStaminaScrolls == N-1, reusing that counter as the
	// sequencing gate rather than adding a separate flag per scroll. Abstract, so tModLoader's
	// autoloader skips this and only registers the 7 concrete subclasses (same idiom as
	// AnimalMinionProjectile).
	public abstract class NumberedStaminaScrollItem : ModItem
	{
		public const float StaminaIncreasePerScroll = 20f;

		protected abstract int ScrollNumber { get; }

		public override string Texture => "NarutoOverhaul/Content/Items/Consumables/StaminaCrystal";

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 36;
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
			return player.GetModPlayer<StaminaPlayer>().ConsumedStaminaScrolls == ScrollNumber - 1;
		}

		public override bool? UseItem(Player player)
		{
			StaminaPlayer staminaPlayer = player.GetModPlayer<StaminaPlayer>();
			staminaPlayer.ConsumedStaminaScrolls = ScrollNumber;
			staminaPlayer.IncreaseBaseMaxStamina(StaminaIncreasePerScroll);
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "StaminaIncrease", $"Permanently increases max Stamina by {StaminaIncreasePerScroll}"));
			tooltips.Add(new TooltipLine(Mod, "Sequence", ScrollNumber == 1
				? $"Scroll {ScrollNumber} of {StaminaPlayer.MaxStaminaScrolls}"
				: $"Scroll {ScrollNumber} of {StaminaPlayer.MaxStaminaScrolls} - requires having read Scroll {ScrollNumber - 1} first"));
		}
	}
}
