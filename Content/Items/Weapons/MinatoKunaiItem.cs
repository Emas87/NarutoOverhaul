using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons
{
	// Minato's Hiraishin kunai - thrown like a real kunai (see KunaiItem for the convention this
	// mirrors) instead of instantly placed like furniture. The seal only gets planted once
	// MinatoKunaiProjectile actually hits a wall (OnTileCollide), not on use. Gated the same as the
	// warp ability itself (Downed Pain) so seals can't be pre-placed before the jutsu that makes
	// them useful is even unlocked.
	public class MinatoKunaiItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 14;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.shoot = ModContent.ProjectileType<MinatoKunaiProjectile>();
			Item.shootSpeed = 11f;
			Item.UseSound = SoundID.Item1;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedPain;
		}

		public override void AddRecipes()
		{
			CreateRecipe(3)
				.AddIngredient(ModContent.ItemType<RinneganFragmentItem>())
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
