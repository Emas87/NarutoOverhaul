using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// First boss in the story roster - craftable from common early-game materials rather than
	// gated behind another boss's drop, since nothing precedes Haku in the chain.
	public class HakuSummonItem : ModItem
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
			// Land of Waves is a misty coastal region - Beach is the closest vanilla biome match.
			return player.ZoneBeach && !NPC.AnyNPCs(ModContent.NPCType<HakuBoss>());
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<HakuBoss>());
			}

			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ItemID.IceBlock, 10)
				.AddIngredient(ItemID.IronBar, 5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
