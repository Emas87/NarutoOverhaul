using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	public class TailedBeastSummonItem : BossSummonItem
	{
		protected override int BossNPCType => ModContent.NPCType<TailedBeastBoss>();

		public override bool CanUseItem(Player player)
		{
			// Gaara's One-Tail transformation happens during the Chunin Exams invasion, Sand
			// Village territory - Desert is the explicit biome match.
			// TEMP: biome gate disabled for faster boss testing - see totest.md "Bosses". Restore
			// `player.ZoneDesert &&` before release.
			return !NPC.AnyNPCs(ModContent.NPCType<TailedBeastBoss>());
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
