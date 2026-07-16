using System.Collections.Generic;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class TaijutsuLegsItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
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
