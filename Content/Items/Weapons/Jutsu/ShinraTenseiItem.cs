using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// Pain's Almighty Push (Deva Path) - Genjutsu's second late-game weapon, control-flavored
	// (heavy knockback, modest damage) to stay in character with the class's identity rather than
	// just being a bigger number.
	public class ShinraTenseiItem : ModItem
	{
		public const float ChakraCost = 40f;

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.damage = 55;
			Item.DamageType = ModContent.GetInstance<GenjutsuDamageClass>();
			Item.noMelee = true;
			Item.knockBack = 14f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 40;
			Item.useTime = 40;
			Item.autoReuse = false;
			Item.shoot = ModContent.ProjectileType<ShinraTenseiProjectile>();
			Item.shootSpeed = 1f;
			Item.value = Item.sellPrice(gold: 16);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item14;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedPain && player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback)
		{
			if (!player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraCost))
			{
				return false;
			}

			Projectile.NewProjectile(source, player.Center, Microsoft.Xna.Framework.Vector2.Zero, type, damage, knockback, player.whoAmI);
			return false;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraCost", $"Uses {ChakraCost} Chakra"));
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Pain defeated"));
		}
	}
}
