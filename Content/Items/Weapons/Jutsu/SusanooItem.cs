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
	// Madara's avatar technique - Genjutsu's first true late-game weapon (the class previously
	// had nothing past Haku's initial unlock). Slow, heavy, and expensive, matching a giant
	// chakra avatar rather than a quick illusion.
	public class SusanooItem : ModItem
	{
		public const float ChakraCost = 45f;

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.damage = 85;
			Item.DamageType = ModContent.GetInstance<GenjutsuDamageClass>();
			Item.noMelee = true;
			Item.knockBack = 9f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.autoReuse = false;
			Item.shoot = ModContent.ProjectileType<SusanooProjectile>();
			Item.shootSpeed = 1f;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item74;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedMadara && player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
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
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Madara defeated"));
		}
	}
}
