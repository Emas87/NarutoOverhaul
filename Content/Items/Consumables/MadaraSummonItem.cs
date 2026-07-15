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
			Item.width = 24;
			Item.height = 24;
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
			return Main.hardMode && player.ZoneGraveyard && !NPC.AnyNPCs(ModContent.NPCType<MadaraBoss>());
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<MadaraBoss>());
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
