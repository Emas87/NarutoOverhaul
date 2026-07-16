using System.Collections.Generic;
using NarutoOverhaul.Content.Items.Consumables;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// Gives a new character the Shinobi Handbook (keybind onboarding - see ShinobiHandbookItem) at
	// spawn, same convention as vanilla's own starting Copper Shortsword/Guide item.
	public class StarterItemPlayer : ModPlayer
	{
		public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
		{
			if (mediumCoreDeath)
			{
				return;
			}

			for (int i = 0; i < 50; i++)
			{
				if (Player.inventory[i].IsAir)
				{
					Player.inventory[i] = new Item(ModContent.ItemType<ShinobiHandbookItem>());
					return;
				}
			}
		}
	}
}
