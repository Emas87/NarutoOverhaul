using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Makes SharinganPlayer's passive dodge (see that class) something the player actually learns
	// about and chooses to unlock, instead of a silent flag flip on downing Haku. Crafted from
	// Haku's own Ice Mirror Shards - the reflective mirrors are the closest thing this mod has to
	// an "eye/reflection" material, and it keeps the unlock tied to the same boss that canonically
	// triggers Sasuke's Sharingan awakening.
	public class SharinganAwakeningItem : ModItem
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
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
		}

		public override bool CanUseItem(Player player)
		{
			return !player.GetModPlayer<SharinganPlayer>().HasSharingan;
		}

		public override bool? UseItem(Player player)
		{
			player.GetModPlayer<SharinganPlayer>().HasSharingan = true;
			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<IceMirrorShardItem>(), 5)
				.AddTile(TileID.Anvils)
				.Register();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "OneTime", "Can only be consumed once - its awakening is permanent"));
			tooltips.Add(new TooltipLine(Mod, "SharinganNote", $"Grants a {SharinganPlayer.DodgeChance * 100:0.##}% chance to dodge a hit entirely and briefly boost damage"));
		}
	}
}
