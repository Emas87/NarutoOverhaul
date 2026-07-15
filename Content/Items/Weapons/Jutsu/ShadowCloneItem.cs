using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Minions;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// Kage Bunshin no Jutsu - ungated (Naruto learns this in the very first arc, before any of
	// the selected story bosses) unlike most other jutsu here. Spawns 2 temporary clones that
	// fight with simple AI for a fixed duration (see ShadowCloneProjectile).
	public class ShadowCloneItem : ModItem
	{
		public const float ChakraCost = 35f;
		public const int CloneCount = 2;

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.damage = 12;
			Item.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item44;
		}

		public override bool CanUseItem(Player player)
		{
			return player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
		}

		public override bool? UseItem(Player player)
		{
			if (!player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraCost))
			{
				return false;
			}

			if (player.whoAmI == Main.myPlayer)
			{
				for (int i = 0; i < CloneCount; i++)
				{
					Vector2 offset = new Vector2(Main.rand.NextFloat(-40f, 40f), 0f);
					Projectile.NewProjectile(player.GetSource_ItemUse(Item, "ShadowClone"), player.Center + offset, Vector2.Zero, ModContent.ProjectileType<ShadowCloneProjectile>(), Item.damage, 0f, player.whoAmI);
				}
			}

			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraCost", $"Uses {ChakraCost} Chakra"));
			tooltips.Add(new TooltipLine(Mod, "Clones", $"Summons {CloneCount} shadow clones that fight for a short time"));
		}
	}
}
