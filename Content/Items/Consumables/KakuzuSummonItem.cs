using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// First Shippuden-era boss - Hardmode gated. Forest/river borderlands - no distinctive
	// vanilla biome match, so no biome gate.
	public class KakuzuSummonItem : BossSummonItem
	{
		protected override int BossNPCType => ModContent.NPCType<KakuzuBoss>();

		public override bool CanUseItem(Player player)
		{
			return Main.hardMode && !NPC.AnyNPCs(ModContent.NPCType<KakuzuBoss>());
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<CursedSnakeFangItem>(), 3)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
