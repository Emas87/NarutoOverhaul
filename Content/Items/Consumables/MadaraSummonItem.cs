using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Reanimated via Edo Tensei - Graveyard is a thematic fit (reanimated undead) even though
	// his canon battlefield isn't literally a graveyard.
	public class MadaraSummonItem : BossSummonItem
	{
		protected override int BossNPCType => ModContent.NPCType<MadaraBoss>();

		public override bool CanUseItem(Player player)
		{
			// TEMP: biome gate disabled for faster boss testing - see totest.md "Bosses". Restore
			// `player.ZoneGraveyard &&` before release.
			return Main.hardMode && !NPC.AnyNPCs(ModContent.NPCType<MadaraBoss>());
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<RinneganFragmentItem>(), 3)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
