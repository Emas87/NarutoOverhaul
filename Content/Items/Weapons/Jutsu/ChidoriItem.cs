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
	// Kakashi's signature technique - the first Ninjutsu weapon gated behind Pain rather than
	// Kakuzu, filling the Hardmode-tier gap in Ninjutsu's weapon progression.
	public class ChidoriItem : ModItem
	{
		public const float ChakraCost = 32f;

		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 34;
			Item.damage = 62;
			Item.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
			Item.noMelee = true;
			Item.knockBack = 7f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 16;
			Item.useTime = 16;
			Item.autoReuse = false;
			Item.shoot = ModContent.ProjectileType<ChidoriProjectile>();
			Item.shootSpeed = 1f;
			Item.value = Item.sellPrice(gold: 12);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item93;
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

			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraCost", $"Uses {ChakraCost} Chakra"));
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Pain defeated"));
		}
	}
}
