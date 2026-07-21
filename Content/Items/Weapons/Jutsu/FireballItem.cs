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
	// Fire Style: Fireball - the first elemental jutsu, unlocked after Kakuzu (whose Fire mask
	// is the first of his five). Straightforward ranged jutsu, unlike Rasengan/Gentle Fist's
	// melee-anchored mechanics.
	public class FireballItem : ModItem
	{
		public const float ChakraCost = 15f;

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.damage = 30;
			Item.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 24;
			Item.useTime = 24;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<FireballProjectile>();
			Item.shootSpeed = 9f;
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
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Kakuzu defeated"));
		}
	}
}
