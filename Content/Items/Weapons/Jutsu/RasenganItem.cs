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
	public class RasenganItem : ModItem
	{
		public const float ChakraCost = 20f;

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.damage = 45;
			Item.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
			Item.noMelee = true; // all damage comes from the projectile, not the swing
			Item.knockBack = 6f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.autoReuse = false;
			Item.shoot = ModContent.ProjectileType<RasenganProjectile>();
			Item.shootSpeed = 1f; // unused by the projectile's own AI, but required to be nonzero to fire
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item8;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedOrochimaru && player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback)
		{
			player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraCost);
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraCost", $"Uses {ChakraCost} Chakra"));
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Orochimaru defeated"));
		}
	}
}
