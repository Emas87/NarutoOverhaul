using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons
{
	// Kunai with an exploding tag - trades Kunai's cheapness for a small area blast on impact.
	public class ExplosiveKunaiItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.damage = 16;
			Item.DamageType = DamageClass.Throwing;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 22;
			Item.useTime = 22;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<ExplosiveKunaiProjectile>();
			Item.shootSpeed = 11f;
			Item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes()
		{
			CreateRecipe(3)
				.AddIngredient(ItemID.IronBar)
				.AddIngredient(ItemID.Gel, 2)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
