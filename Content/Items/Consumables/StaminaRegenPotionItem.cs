using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	public class StaminaRegenPotionItem : ModItem
	{
		public const int BuffDuration = 3600; // 1 minute

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
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.White;
			Item.buffType = ModContent.BuffType<StaminaRegenBuff>();
			Item.buffTime = BuffDuration;
		}

		public override bool? UseItem(Player player)
		{
			player.AddBuff(Item.buffType, Item.buffTime);
			return true;
		}
	}
}
