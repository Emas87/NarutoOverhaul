using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// Kaguya's All-Killing Ash Bones - the post-Kaguya capstone weapon, and Ninjutsu's second
	// late-game weapon alongside Chidori. Fires a 3-way spread rather than a single shot, matching
	// the "barrage" nature of the technique.
	public class AllKillingAshBonesItem : ModItem
	{
		public const float ChakraCost = 38f;
		private const float SpreadAngle = 0.18f;

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.damage = 48;
			Item.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
			Item.noMelee = true;
			Item.knockBack = 5f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 28;
			Item.useTime = 28;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<AshBoneProjectile>();
			Item.shootSpeed = 11f;
			Item.value = Item.sellPrice(gold: 18);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item1;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedKaguya && player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraCost);

			float[] spreadAngles = { -SpreadAngle, 0f, SpreadAngle };

			foreach (float angle in spreadAngles)
			{
				Vector2 shotVelocity = velocity.RotatedBy(angle);
				Projectile.NewProjectile(source, position, shotVelocity, type, damage, knockback, player.whoAmI);
			}

			return false;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraCost", $"Uses {ChakraCost} Chakra"));
		}
	}
}
