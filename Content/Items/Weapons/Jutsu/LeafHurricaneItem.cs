using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// A sweeping spin kick - wide reach so it naturally catches multiple enemies in one swing,
	// unlike Gentle Fist's single precise strike.
	public class LeafHurricaneItem : ModItem
	{
		public const float StaminaCost = 12f;

		public override void SetDefaults()
		{
			Item.width = 44;
			Item.height = 44;
			Item.damage = 26;
			Item.DamageType = ModContent.GetInstance<TaijutsuDamageClass>();
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 26;
			Item.useTime = 26;
			Item.knockBack = 5f;
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

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "StaminaCost", $"Uses {StaminaCost} Stamina"));
		}
	}
}
