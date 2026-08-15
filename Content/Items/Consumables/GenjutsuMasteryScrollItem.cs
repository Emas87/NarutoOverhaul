using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Crafted from Pain's and Madara's drops - both illusion/dimension-adjacent bosses (Rinnegan,
	// Susanoo/Sharingan), fitting Genjutsu. A permanent, one-time consumable like
	// OtsutsukiChakraFragmentItem, not gear: the actual bonus lives in ClassMasteryPlayer, applied
	// every tick once the flag is set.
	public class GenjutsuMasteryScrollItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useTurn = true;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.UseSound = SoundID.Item29;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.LightRed;
		}

		public override bool CanUseItem(Player player)
		{
			return !player.GetModPlayer<ClassMasteryPlayer>().HasGenjutsuMastery;
		}

		public override bool? UseItem(Player player)
		{
			player.GetModPlayer<ClassMasteryPlayer>().HasGenjutsuMastery = true;
			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<RinneganFragmentItem>(), 5)
				.AddIngredient(ModContent.ItemType<SusanooCoreItem>(), 3)
				.AddTile(TileID.Anvils)
				.Register();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "OneTime", "Can only be consumed once - its blessing is permanent"));
			tooltips.Add(new TooltipLine(Mod, "MasteryNote", $"Permanently increases Genjutsu damage by {ClassMasteryPlayer.MasteryDamageBonus * 100:0.##}%"));
		}
	}
}
