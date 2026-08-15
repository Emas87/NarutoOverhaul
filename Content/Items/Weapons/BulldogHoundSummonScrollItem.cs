using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Buffs;
using NarutoOverhaul.Content.Minions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons
{
	public class BulldogHoundSummonScrollItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 14;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 8;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = ModContent.BuffType<BulldogHoundMinionBuff>();
			Item.shoot = ModContent.ProjectileType<BulldogHoundMinionProjectile>();
			Item.shootSpeed = 1f;
		}

		public override bool? UseItem(Player player)
		{
			player.AddBuff(Item.buffType, 2);

			if (player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(player.GetSource_ItemUse(Item, "BulldogHoundSummonScroll"), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
			}

			return true;
		}
	}
}
