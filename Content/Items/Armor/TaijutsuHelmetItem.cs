using System.Collections.Generic;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Armor
{
	// Taijutsu's first armor set - fills the "no armor at all" gap. Each piece gives a small
	// Taijutsu damage bump on its own; the real payoff is the full-set bonus on TaijutsuBodyItem.
	[AutoloadEquip(EquipType.Head)]
	public class TaijutsuHelmetItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 26;
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(ModContent.GetInstance<TaijutsuDamageClass>()) += 0.05f;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "SetBonusHint", "Part of the Taijutsu armor set"));
		}
	}
}
