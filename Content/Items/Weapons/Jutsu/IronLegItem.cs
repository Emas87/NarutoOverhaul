using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// A single heavy kick - high damage/knockback, cheap stamina cost, small hitbox. The
	// power-over-reach counterpart to Leaf Hurricane's reach-over-power.
	public class IronLegItem : ModItem
	{
		public const float StaminaCost = 6f;

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.damage = 40;
			Item.DamageType = ModContent.GetInstance<TaijutsuDamageClass>();
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedShukaku && player.GetModPlayer<StaminaPlayer>().Stamina >= StaminaCost;
		}

		public override void UseAnimation(Player player)
		{
			player.GetModPlayer<StaminaPlayer>().TrySpendStamina(StaminaCost);
		}

		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.Knockback *= 1.5f;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "StaminaCost", $"Uses {StaminaCost} Stamina"));
		}
	}
}
