using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.VFX;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Instant Chakra restore - mirrors vanilla Mana Potion conventions: no cooldown, unlike
	// Healing potions' potion sickness.
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
			chakraPlayer.Chakra = System.Math.Min(chakraPlayer.MaxChakra, chakraPlayer.Chakra + RestoreAmount);
			ChakraVFX.SpawnBurst(player.Center, DustID.BlueTorch, 8, 1f);
			return true;
		}
	}
}
