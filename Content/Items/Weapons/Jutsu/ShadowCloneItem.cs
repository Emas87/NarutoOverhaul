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
		// Unlike other minions, ShadowCloneItem had no cooldown beyond its Chakra cost and no
		// check for existing clones before spawning more - re-casting quickly could stack clones
		// (and their per-tick target scans) without bound. Cap total concurrent clones per owner.
		public const int MaxConcurrentClones = 3;

		private static int CountOwnedClones(int owner)
		{
			int type = ModContent.ProjectileType<ShadowCloneProjectile>();
			int count = 0;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.type == type && p.owner == owner)
				{
					count++;
				}
			}
			return count;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
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
			return player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost
				&& CountOwnedClones(player.whoAmI) < MaxConcurrentClones;
		}

		public override bool? UseItem(Player player)
		{
			int existing = CountOwnedClones(player.whoAmI);
			if (existing >= MaxConcurrentClones)
			{
				return false;
			}

			if (!player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraCost))
			{
				return false;
			}

			if (player.whoAmI == Main.myPlayer)
			{
				int toSpawn = System.Math.Min(CloneCount, MaxConcurrentClones - existing);
				for (int i = 0; i < toSpawn; i++)
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
			tooltips.Add(new TooltipLine(Mod, "CloneCap", $"Max {MaxConcurrentClones} clones active at once"));
		}
	}
}
