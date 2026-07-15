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
	public class WaterDragonItem : ModItem
	{
		public const float ChakraCost = 28f;

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.damage = 32;
			Item.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 34;
			Item.useTime = 34;
			Item.shoot = ModContent.ProjectileType<WaterDragonProjectile>();
			Item.shootSpeed = 6f;
			Item.value = Item.sellPrice(gold: 6);
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
