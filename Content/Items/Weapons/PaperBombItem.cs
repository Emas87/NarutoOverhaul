using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons
{
	// Explosive tag on a timer - lobbed in an arc, sticks to the first surface it lands on,
	// then detonates after a short fuse. The delayed-trap identity next to the Explosive
	// Kunai's instant-impact blast.
	public class PaperBombItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 28;
			Item.damage = 30;
			Item.DamageType = DamageClass.Throwing;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 24;
			Item.useTime = 24;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.value = Item.sellPrice(silver: 3);
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<PaperBombProjectile>();
			Item.shootSpeed = 9f;
			Item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes()
		{
			CreateRecipe(3)
				.AddIngredient(ItemID.Gel, 3)
				.AddIngredient(ItemID.Wood, 1)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
