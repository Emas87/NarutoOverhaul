using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class TaijutsuBodyItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.defense = 8;
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(ModContent.GetInstance<TaijutsuDamageClass>()) += 0.05f;
		}

		// Full-set bonus, checked/applied from the body piece by convention - boosts max Stamina
		// and its regen rate, staying self-contained (only touches ChakraPlayer/StaminaPlayer's
		// already-existing frame-staged fields, same idiom as every accessory's UpdateEquip) rather
		// than reaching into every Taijutsu weapon's own cost calculation.
		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ModContent.ItemType<TaijutsuHelmetItem>() && legs.type == ModContent.ItemType<TaijutsuLegsItem>();
		}

		public override void UpdateArmorSet(Player player)
		{
			StaminaPlayer staminaPlayer = player.GetModPlayer<StaminaPlayer>();
			staminaPlayer.MaxStamina += 30f;
			staminaPlayer.StaminaRegenRate += 0.6f;
			player.GetDamage(ModContent.GetInstance<TaijutsuDamageClass>()) += 0.05f;

			player.setBonus = "Set Bonus: +30 max Stamina, faster Stamina regen, +5% Taijutsu damage";
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "SetBonusHint", "Part of the Taijutsu armor set"));
		}
	}
}
