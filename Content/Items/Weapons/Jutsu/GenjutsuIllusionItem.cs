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
	// Genjutsu's first ability, establishing the class: modest direct damage, but puppets the
	// target's movement for a few seconds (see GenjutsuGlobalNPC) rather than relying on damage
	// to carry the item like Taijutsu/Ninjutsu do.
	public class GenjutsuIllusionItem : ModItem
	{
		public const float ChakraCost = 18f;

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 22;
			Item.damage = 8;
			Item.DamageType = ModContent.GetInstance<GenjutsuDamageClass>();
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 24;
			Item.useTime = 24;
			Item.shoot = ModContent.ProjectileType<GenjutsuIllusionProjectile>();
			Item.shootSpeed = 8f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item43;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedHaku && player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
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
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Haku defeated"));
		}
	}
}
