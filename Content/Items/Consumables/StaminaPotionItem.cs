using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.VFX;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	public class StaminaPotionItem : ModItem
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
			StaminaPlayer staminaPlayer = player.GetModPlayer<StaminaPlayer>();
			return staminaPlayer.Stamina < staminaPlayer.MaxStamina;
		}

		public override bool? UseItem(Player player)
		{
			StaminaPlayer staminaPlayer = player.GetModPlayer<StaminaPlayer>();
			staminaPlayer.Stamina = System.Math.Min(staminaPlayer.MaxStamina, staminaPlayer.Stamina + RestoreAmount);
			ChakraVFX.SpawnBurst(player.Center, DustID.Torch, 8, 1f);
			return true;
		}
	}
}
