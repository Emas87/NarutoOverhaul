using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Reanimated via Edo Tensei - Graveyard is a thematic fit (reanimated undead) even though
	// his canon battlefield isn't literally a graveyard.
	public class MadaraSummonItem : ModItem
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
			// TEMP: biome gate disabled for faster boss testing - see totest.md "Bosses". Restore
			// `player.ZoneGraveyard &&` before release.
			return Main.hardMode && !NPC.AnyNPCs(ModContent.NPCType<MadaraBoss>());
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
				NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X, (int)player.Center.Y - 300, ModContent.NPCType<MadaraBoss>());
			}

			return true;
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
