using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// First boss in the story roster - craftable from common early-game materials rather than
	// gated behind another boss's drop, since nothing precedes Haku in the chain.
	public class HakuSummonItem : BossSummonItem
	{
		protected override int BossNPCType => ModContent.NPCType<HakuBoss>();

		public override bool CanUseItem(Player player)
		{
			// Land of Waves is a misty coastal region - Beach is the closest vanilla biome match.
			// TEMP: biome gate disabled for faster boss testing - see totest.md "Bosses". Restore
			// `player.ZoneBeach &&` before release.
			return !NPC.AnyNPCs(ModContent.NPCType<HakuBoss>());
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
