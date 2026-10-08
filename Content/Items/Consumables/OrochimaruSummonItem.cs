using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Konoha village / Forest of Death - no distinctive vanilla biome match, so no biome gate.
	public class OrochimaruSummonItem : BossSummonItem
	{
		protected override int BossNPCType => ModContent.NPCType<OrochimaruBoss>();

		public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(ModContent.NPCType<OrochimaruBoss>());
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<SandCoreItem>(), 3)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
