using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// True final boss - explicitly requires every other story boss down, on top of the material
	// chain already enforcing it transitively (belt-and-suspenders, matching how other unlock
	// rewards double-check flags directly).
	public class KaguyaSummonItem : ModItem
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
			return Main.hardMode
				&& StoryProgressSystem.DownedHaku
				&& StoryProgressSystem.DownedShukaku
				&& StoryProgressSystem.DownedOrochimaru
				&& StoryProgressSystem.DownedKakuzu
				&& StoryProgressSystem.DownedPain
				&& StoryProgressSystem.DownedMadara
				&& !NPC.AnyNPCs(ModContent.NPCType<KaguyaBoss>());
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<KaguyaBoss>());
			}

			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<SusanooCoreItem>(), 3)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
