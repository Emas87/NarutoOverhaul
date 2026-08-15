using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons
{
	// The oversized four-blade shuriken - the heavy end of the basic-thrown-weapon line:
	// slower to throw than Shuriken but hits harder and cuts through more targets.
	public class FumaShurikenItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.damage = 24;
			Item.DamageType = DamageClass.Throwing;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 26;
			Item.useTime = 26;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.value = Item.sellPrice(silver: 4);
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<FumaShurikenProjectile>();
			Item.shootSpeed = 13f;
			Item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes()
		{
			CreateRecipe(3)
				.AddIngredient(ItemID.IronBar, 4)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
