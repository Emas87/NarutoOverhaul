using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class NinjutsuBodyItem : ModItem
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
			player.GetDamage(ModContent.GetInstance<NinjutsuDamageClass>()) += 0.05f;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ModContent.ItemType<NinjutsuHelmetItem>() && legs.type == ModContent.ItemType<NinjutsuLegsItem>();
		}

		public override void UpdateArmorSet(Player player)
		{
			ChakraPlayer chakraPlayer = player.GetModPlayer<ChakraPlayer>();
			chakraPlayer.MaxChakra += 40f;
			chakraPlayer.ChakraRegenRate += 0.3f;
			player.GetDamage(ModContent.GetInstance<NinjutsuDamageClass>()) += 0.05f;

			player.setBonus = "Set Bonus: +40 max Chakra, faster Chakra regen, +5% Ninjutsu damage";
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "SetBonusHint", "Part of the Ninjutsu armor set"));
		}
	}
}
