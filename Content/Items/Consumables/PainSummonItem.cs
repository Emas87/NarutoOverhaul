using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// "Pain levels the Hidden Leaf Village" - requires being near your own town, mirroring that
	// the fight is meant to threaten a settlement, not just the player.
	public class PainSummonItem : BossSummonItem
	{
		protected override int BossNPCType => ModContent.NPCType<PainBoss>();

		public const float TownRequirementRadius = PainBoss.TownNpcDetectionRadius;

		public override bool CanUseItem(Player player)
		{
			return Main.hardMode
				&& PainBoss.IsNearTownNPC(player.Center, TownRequirementRadius)
				&& !NPC.AnyNPCs(ModContent.NPCType<PainBoss>());
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<KakuzuHeartItem>(), 3)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
