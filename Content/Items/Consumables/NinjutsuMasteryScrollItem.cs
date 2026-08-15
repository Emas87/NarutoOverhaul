using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Crafted from Haku's and Orochimaru's drops - both elemental/jutsu-focused bosses, fitting
	// Ninjutsu. A permanent, one-time consumable like OtsutsukiChakraFragmentItem, not gear:
	// the actual bonus lives in ClassMasteryPlayer, applied every tick once the flag is set.
	public class NinjutsuMasteryScrollItem : ModItem
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
			return !player.GetModPlayer<ClassMasteryPlayer>().HasNinjutsuMastery;
		}

		public override bool? UseItem(Player player)
		{
			player.GetModPlayer<ClassMasteryPlayer>().HasNinjutsuMastery = true;
			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<IceMirrorShardItem>(), 5)
				.AddIngredient(ModContent.ItemType<CursedSnakeFangItem>(), 3)
				.AddTile(TileID.Anvils)
				.Register();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "OneTime", "Can only be consumed once - its blessing is permanent"));
			tooltips.Add(new TooltipLine(Mod, "MasteryNote", $"Permanently increases Ninjutsu damage by {ClassMasteryPlayer.MasteryDamageBonus * 100:0.##}%"));
		}
	}
}
