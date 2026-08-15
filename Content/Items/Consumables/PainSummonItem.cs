using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// "Pain levels the Hidden Leaf Village" - requires being near your own town, mirroring that
	// the fight is meant to threaten a settlement, not just the player.
	public class PainSummonItem : ModItem
	{
		public const float TownRequirementRadius = PainBoss.TownNpcDetectionRadius;

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
			return Main.hardMode
				&& PainBoss.IsNearTownNPC(player.Center, TownRequirementRadius)
				&& !NPC.AnyNPCs(ModContent.NPCType<PainBoss>());
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
				NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X, (int)player.Center.Y - 300, ModContent.NPCType<PainBoss>());
			}

			return true;
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
