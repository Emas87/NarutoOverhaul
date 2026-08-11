using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Consumables
{
	// Pure onboarding - no stats, no use, just a tooltip listing every transformation keybind so a
	// new player can discover Sage Mode/Tailed Beast Mode/Six Paths Sage Mode/Eight Gates/Chakra
	// Control exist at all. Placed directly in a new character's starting inventory (see
	// Common/Players/StarterItemPlayer.cs) rather than sold, matching vanilla's own guide/starter
	// item convention.
	public class ShinobiHandbookItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 24;
			Item.maxStack = 1;
			Item.rare = ItemRarityID.White;
			Item.value = 0;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Handbook1", "A shinobi's field notes - keybinds for every technique you'll unlock:"));
			tooltips.Add(new TooltipLine(Mod, "Handbook2", "Toggle Sage Mode: '.'  |  Toggle Tailed Beast Mode: '/'"));
			tooltips.Add(new TooltipLine(Mod, "Handbook3", "Toggle Six Paths Sage Mode: ','  |  Toggle Eight Gates: '['"));
			tooltips.Add(new TooltipLine(Mod, "Handbook4", "Toggle Chakra Control (water/wall walking): ']'"));
			tooltips.Add(new TooltipLine(Mod, "Handbook5", "Toggle Kamui Phase: '\\'  |  Toggle Byakugan: 'B'"));
			tooltips.Add(new TooltipLine(Mod, "Handbook6", "Toggle Curse Mark: 'N'  |  Hiraishin Warp: 'H'"));
			tooltips.Add(new TooltipLine(Mod, "Handbook7", "If a keybind does nothing, it may not have bound by default -"));
			tooltips.Add(new TooltipLine(Mod, "Handbook8", "check/set it yourself in Settings > Controls > Keybinds."));
		}
	}
}
