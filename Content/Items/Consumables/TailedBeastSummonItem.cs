using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	public class TailedBeastSummonItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 20;
			Item.consumable = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item44;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}

		public override bool CanUseItem(Player player)
		{
			// Gaara's One-Tail transformation happens during the Chunin Exams invasion, Sand
			// Village territory - Desert is the explicit biome match.
			return player.ZoneDesert && !NPC.AnyNPCs(ModContent.NPCType<TailedBeastBoss>());
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<TailedBeastBoss>());
			}

			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<IceMirrorShardItem>(), 3)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
