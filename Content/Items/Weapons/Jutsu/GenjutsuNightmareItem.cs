using System.Collections.Generic;
using NarutoOverhaul.Common.GlobalNPCs;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// A short-range AoE fear illusion - melee-hitbox so it naturally catches multiple enemies in
	// one swing (like Leaf Hurricane), applying GenjutsuFearDebuff instead of dealing real damage.
	public class GenjutsuNightmareItem : ModItem
	{
		public const float ChakraCost = 20f;
		public const int FearDuration = 180; // 3 seconds

		public override void SetDefaults()
		{
			Item.width = 36;
			Item.height = 36;
			Item.damage = 6;
			Item.DamageType = ModContent.GetInstance<GenjutsuDamageClass>();
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.knockBack = 1f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item43;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedHaku && player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
		}

		public override void UseAnimation(Player player)
		{
			player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraCost);
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			int duration = FearDuration + player.GetModPlayer<ChakraPlayer>().GenjutsuControlDurationBonus;

			target.AddBuff(ModContent.BuffType<GenjutsuFearDebuff>(), duration);
			target.GetGlobalNPC<GenjutsuGlobalNPC>().ControllingPlayerIndex = player.whoAmI;
			ChakraVFX.SpawnBurst(target.Center, DustID.PurpleTorch, 10, 1.2f);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraCost", $"Uses {ChakraCost} Chakra"));
			tooltips.Add(new TooltipLine(Mod, "Fear", "Terrifies nearby enemies into fleeing"));
		}
	}
}
