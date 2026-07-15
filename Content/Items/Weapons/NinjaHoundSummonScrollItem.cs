using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Buffs;
using NarutoOverhaul.Content.Minions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons
{
	public class NinjaHoundSummonScrollItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 11;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 8;
			Item.width = 28;
			Item.height = 28;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = ModContent.BuffType<NinjaHoundMinionBuff>();
			Item.shoot = ModContent.ProjectileType<NinjaHoundMinionProjectile>();
			Item.shootSpeed = 1f;
		}

		public override bool? UseItem(Player player)
		{
			player.AddBuff(Item.buffType, 2);

			if (player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(player.GetSource_ItemUse(Item, "NinjaHoundSummonScroll"), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
			}

			return true;
		}
	}
}
