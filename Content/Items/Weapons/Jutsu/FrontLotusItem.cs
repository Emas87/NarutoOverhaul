using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// Guy/Lee's inner-gate assisted strike - gated on Kakuzu (the first Hardmode boss) rather than
	// Shukaku like the rest of Guy's stock, filling Taijutsu's previously-nonexistent Hardmode tier.
	public class FrontLotusItem : ModItem
	{
		public const float StaminaCost = 16f;
		public const float ArmorPenetration = 25f;

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.damage = 58;
			Item.DamageType = ModContent.GetInstance<TaijutsuDamageClass>();
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 22;
			Item.useTime = 22;
			Item.autoReuse = true;
			Item.knockBack = 6f;
			Item.value = Item.sellPrice(gold: 14);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item1;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedKakuzu && player.GetModPlayer<StaminaPlayer>().Stamina >= StaminaCost;
		}

		public override void UseAnimation(Player player)
		{
			player.GetModPlayer<StaminaPlayer>().TrySpendStamina(StaminaCost);
		}

		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.ArmorPenetration += ArmorPenetration;
			modifiers.Knockback += 0.5f;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "StaminaCost", $"Uses {StaminaCost} Stamina"));
			tooltips.Add(new TooltipLine(Mod, "ArmorPen", $"Strikes ignore {ArmorPenetration} defense and hit with extra force"));
		}
	}
}
