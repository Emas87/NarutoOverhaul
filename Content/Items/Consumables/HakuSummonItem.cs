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
			Item.width = 32;
			Item.height = 32;
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
			// TEMP: biome gate disabled for faster boss testing - see totest.md "Bosses". Restore
			// `player.ZoneBeach &&` before release.
			return !NPC.AnyNPCs(ModContent.NPCType<HakuBoss>());
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				// NPC.SpawnOnPlayer's generic fallback picks a random surface tile within a wide
				// radius and drops the NPC's top-left corner exactly on it, with no guarantee the
				// spot isn't buried in solid ground (see TailedBeastSummonItem/OrochimaruSummonItem
				// for the original "boss doesn't show up" reports this caused). Spawning directly
				// above the player guarantees he's on-screen and falls into view under normal
				// gravity.
				NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X, (int)player.Center.Y - 300, ModContent.NPCType<HakuBoss>());
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
