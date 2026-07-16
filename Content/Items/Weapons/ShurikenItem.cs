using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons
{
	// A step up from Kunai within the same "basic thrown weapon" niche - pierces multiple targets
	// instead of stopping at the first, at a slightly steeper material cost. Ungated like Kunai.
	public class ShurikenItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.damage = 11;
			Item.DamageType = DamageClass.Throwing;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 18;
			Item.useTime = 18;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.White;
			Item.shoot = ModContent.ProjectileType<ShurikenProjectile>();
			Item.shootSpeed = 12f;
			Item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes()
		{
			CreateRecipe(3)
				.AddIngredient(ItemID.IronBar, 2)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
