using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Placeable
{
	// Places a HiraishinSealTile - the marker Minato's Hiraishin Warp keybind teleports to.
	// Gated the same as the warp ability itself (Downed Pain) so seals can't be pre-placed before
	// the jutsu that makes them useful is even unlocked.
	public class HiraishinSealItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.createTile = ModContent.TileType<HiraishinSealTile>();
			Item.UseSound = SoundID.Item6;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedPain;
		}

		public override void AddRecipes()
		{
			CreateRecipe(3)
				.AddIngredient(ModContent.ItemType<Materials.RinneganFragmentItem>())
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
