using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons
{
	// Basic starter weapon - no chakra cost, no jutsu mechanic, just a thrown blade to give the
	// player something before Rasengan unlocks later in the story.
	public class KunaiItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.damage = 8;
			Item.DamageType = DamageClass.Throwing;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.White;
			Item.shoot = ModContent.ProjectileType<KunaiProjectile>();
			Item.shootSpeed = 11f;
			Item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes()
		{
			CreateRecipe(3)
				.AddIngredient(ItemID.IronBar)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
