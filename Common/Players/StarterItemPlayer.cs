using System.Collections.Generic;
using NarutoOverhaul.Content.Items.Accessories;
using NarutoOverhaul.Content.Items.Armor;
using NarutoOverhaul.Content.Items.Consumables;
using NarutoOverhaul.Content.Items.Weapons;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// Gives a new character the Shinobi Handbook (keybind onboarding - see ShinobiHandbookItem) at
	// spawn, same convention as vanilla's own starting Copper Shortsword/Guide item.
	public class StarterItemPlayer : ModPlayer
	{
		// Player.inventory (58 slots) is main inventory + 4 ammo + 4 coin slots - only searching the
		// first 50 keeps the handbook out of the ammo/coin slots.
		private const int MainInventorySlots = 50;

		public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
		{
			if (mediumCoreDeath)
			{
				return;
			}

			for (int i = 0; i < MainInventorySlots; i++)
			{
				if (Player.inventory[i].IsAir)
				{
					Player.inventory[i] = new Item(ModContent.ItemType<ShinobiHandbookItem>());
					break;
				}
			}

			// TEMP: Ninjutsu armor set added here so the PixelLab sprite pilot can be checked
			// in-game directly - remove once the armor regeneration pass is finalized.
			GiveIfRoom<NinjutsuHelmetItem>();
			GiveIfRoom<NinjutsuBodyItem>();
			GiveIfRoom<NinjutsuLegsItem>();

			// TEMP: throwing weapons - all 5 are ungated/craftable so a test kit with story-flag
			// unlocks (like the jutsu classes get) isn't needed, just the items themselves so
			// testing doesn't require farming Iron Bars/Gel/Wood first. See totest.md.
			GiveIfRoom<KunaiItem>(20);
			GiveIfRoom<ExplosiveKunaiItem>(20);
			GiveIfRoom<ShurikenItem>(20);
			GiveIfRoom<FumaShurikenItem>(20);
			GiveIfRoom<PaperBombItem>(20);
		}

		private static readonly int[] ThrowingWeaponTestItemTypes =
		{
			ModContent.ItemType<KunaiItem>(),
			ModContent.ItemType<ExplosiveKunaiItem>(),
			ModContent.ItemType<ShurikenItem>(),
			ModContent.ItemType<FumaShurikenItem>(),
			ModContent.ItemType<PaperBombItem>(),
		};

		private const int ThrowingWeaponRestockAmount = 30;

		// TEMP: accessories normally only come from town NPC shops (Guy Sensei, Kakashi, Itachi,
		// Shinobi Vendor), some behind story-flag gates (downed Shukaku/Kakuzu/Madara/Haku) - none
		// of that is tested/spawned yet, so hand these over directly instead of blocking accessory
		// testing on NPC/story progress. See totest.md.
		private static readonly int[] AccessoryTestItemTypes =
		{
			ModContent.ItemType<ChakraPaperItem>(),
			ModContent.ItemType<ChakraWingsItem>(),
			ModContent.ItemType<IllusionCharmItem>(),
			ModContent.ItemType<MonstrousStrengthGlovesItem>(),
			ModContent.ItemType<WeightedLegWarmersItem>(),
			ModContent.ItemType<SubstitutionScrollItem>(),
		};

		// TEMP: same throwing-weapon kit as ModifyStartingInventory above, but also granted on
		// world entry for already-existing characters (ModifyStartingInventory only fires for
		// brand-new characters). Tops back up to ThrowingWeaponRestockAmount rather than only
		// granting once, so reloading the world (which testing already requires after every build)
		// restocks whatever's been used up. Remove alongside the TEMP block above once testing is
		// done.
		public override void OnEnterWorld()
		{
			foreach (int itemType in ThrowingWeaponTestItemTypes)
			{
				int owned = Player.CountItem(itemType);
				if (owned < ThrowingWeaponRestockAmount)
				{
					Player.QuickSpawnItem(Player.GetSource_Misc("ThrowingWeaponTestKit"), itemType, ThrowingWeaponRestockAmount - owned);
				}
			}

			foreach (int itemType in AccessoryTestItemTypes)
			{
				if (!Player.HasItem(itemType))
				{
					Player.QuickSpawnItem(Player.GetSource_Misc("AccessoryTestKit"), itemType);
				}
			}

			// TEMP: SharinganAwakeningItem is normally crafted from Haku's Ice Mirror Shards - hand
			// one over directly so the new consumable-unlock flow (see SharinganPlayer.HasSharingan)
			// can be tested without farming/crafting. Stays in inventory unused once consumed (its
			// own CanUseItem blocks a second use), so this won't re-spam copies. See totest.md.
			if (!Player.HasItem(ModContent.ItemType<SharinganAwakeningItem>()) && !Player.GetModPlayer<SharinganPlayer>().HasSharingan)
			{
				Player.QuickSpawnItem(Player.GetSource_Misc("AccessoryTestKit"), ModContent.ItemType<SharinganAwakeningItem>());
			}
		}

		private void GiveIfRoom<T>(int stack = 1) where T : ModItem
		{
			for (int i = 0; i < MainInventorySlots; i++)
			{
				if (Player.inventory[i].IsAir)
				{
					Player.inventory[i] = new Item(ModContent.ItemType<T>(), stack);
					return;
				}
			}
		}
	}
}
