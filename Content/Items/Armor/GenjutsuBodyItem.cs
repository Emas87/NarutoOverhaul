using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class GenjutsuBodyItem : ModItem
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
			player.GetDamage(ModContent.GetInstance<GenjutsuDamageClass>()) += 0.05f;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ModContent.ItemType<GenjutsuHelmetItem>() && legs.type == ModContent.ItemType<GenjutsuLegsItem>();
		}

		public override void UpdateArmorSet(Player player)
		{
			ChakraPlayer chakraPlayer = player.GetModPlayer<ChakraPlayer>();
			chakraPlayer.MaxChakra += 40f;
			chakraPlayer.GenjutsuControlDurationBonus += 90; // +1.5 seconds
			player.GetDamage(ModContent.GetInstance<GenjutsuDamageClass>()) += 0.05f;

			player.setBonus = "Set Bonus: +40 max Chakra, +1.5s Genjutsu control duration, +5% Genjutsu damage";
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "SetBonusHint", "Part of the Genjutsu armor set"));
		}
	}
}
