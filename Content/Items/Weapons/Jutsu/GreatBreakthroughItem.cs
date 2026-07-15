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
	// Wind Style: Great Breakthrough - a short-range piercing gust, unlocked alongside Fireball.
	public class GreatBreakthroughItem : ModItem
	{
		public const float ChakraCost = 18f;

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 18;
			Item.damage = 24;
			Item.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 26;
			Item.useTime = 26;
			Item.shoot = ModContent.ProjectileType<GreatBreakthroughProjectile>();
			Item.shootSpeed = 10f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item20;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedKakuzu && player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback)
		{
			player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraCost);
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraCost", $"Uses {ChakraCost} Chakra"));
		}
	}
}
