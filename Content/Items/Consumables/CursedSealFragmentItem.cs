using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Orochimaru's parting gift - consuming it permanently brands the character with the
	// Curse Mark (unlocks the CurseMarkForm toggle). One-time, like the Otsutsuki Fragment.
	public class CursedSealFragmentItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 22;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useTurn = true;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.UseSound = SoundID.Item29;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightPurple;
		}

		public override bool CanUseItem(Player player)
		{
			return !player.GetModPlayer<CurseMarkPlayer>().HasCurseMark;
		}

		public override bool? UseItem(Player player)
		{
			player.GetModPlayer<CurseMarkPlayer>().HasCurseMark = true;
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "OneTime", "Can only be consumed once - the mark is permanent"));
		}
	}
}
