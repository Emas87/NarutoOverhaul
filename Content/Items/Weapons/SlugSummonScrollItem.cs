using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Buffs;
using NarutoOverhaul.Content.Minions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons
{
	public class SlugSummonScrollItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 9;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.knockBack = 1.5f;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = ModContent.BuffType<SlugMinionBuff>();
			Item.shoot = ModContent.ProjectileType<SlugMinionProjectile>();
			Item.shootSpeed = 1f;
		}

		public override bool? UseItem(Player player)
		{
			player.AddBuff(Item.buffType, 2);

			if (player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(player.GetSource_ItemUse(Item, "SlugSummonScroll"), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
			}

			return true;
		}
	}
}
