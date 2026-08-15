using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	public class TailedBeastSummonItem : ModItem
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
			// Gaara's One-Tail transformation happens during the Chunin Exams invasion, Sand
			// Village territory - Desert is the explicit biome match.
			// TEMP: biome gate disabled for faster boss testing - see totest.md "Bosses". Restore
			// `player.ZoneDesert &&` before release.
			return !NPC.AnyNPCs(ModContent.NPCType<TailedBeastBoss>());
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				// NPC.SpawnOnPlayer's generic fallback (used by every other boss summon item here)
				// picks a random surface tile within a wide radius and drops the NPC's top-left
				// corner exactly on it - fine for a normal-sized boss, but Shukaku's 400x400 hitbox
				// (SizeMultiplier=4 in TailedBeastBoss) then has most of its body buried in solid
				// ground at that point, with no guarantee of a pocket big enough to climb out of -
				// which read as "doesn't show up, have to go find him". Spawning directly above the
				// player instead guarantees he's on-screen and falls into view under normal gravity.
				NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X, (int)player.Center.Y - 300, ModContent.NPCType<TailedBeastBoss>());
			}

			return true;
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
