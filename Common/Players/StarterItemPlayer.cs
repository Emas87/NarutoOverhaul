using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Consumables;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using NarutoOverhaul.Content.NPCs.Town;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Players
{
	// Gives a new character the Shinobi Handbook (keybind onboarding - see ShinobiHandbookItem) at
	// spawn, same intent as vanilla's own starting Copper Shortsword/Guide item.
	//
	// Root-caused 2026-08-18: this used to hook ModifyStartingInventory and write straight into
	// Player.inventory there, which looked reasonable but never actually worked - decompiling
	// tModLoader's own PlayerLoader.GetStartingItems (via ilspycmd against tModLoader.dll) shows
	// ModifyStartingInventory/AddStartingItems are wired to exactly one call site, Player.DropItems,
	// i.e. they only feed the mediumcore-death "what do I keep" calculation, not actual new-
	// character inventory setup. There is no dedicated "new character created" mod hook at all -
	// the fix is OnEnterWorld (already used below for the story-unlock/NPC-spawn test hooks) gated
	// on a persisted one-time flag, same SaveData/LoadData bool-flag convention as
	// CurseMarkPlayer/MoonLordBlessingPlayer.
	public class StarterItemPlayer : ModPlayer
	{
		// Player.inventory (58 slots) is main inventory + 4 ammo + 4 coin slots - only searching the
		// first 50 keeps granted items out of the ammo/coin slots.
		private const int MainInventorySlots = 50;

		private bool hasGivenStarterItems;

		public override void SaveData(TagCompound tag)
		{
			tag["hasGivenStarterItems"] = hasGivenStarterItems;
		}

		public override void LoadData(TagCompound tag)
		{
			hasGivenStarterItems = tag.GetBool("hasGivenStarterItems");
		}

		// TEMP: normally these four only move in once a valid empty house exists for them (plain
		// vanilla town-NPC housing rules - see totest.md "Testing now"). For testing, spawn one of
		// each directly next to the player like vanilla's Guide (who is unconditionally present from
		// world creation, no house required) instead of building housing first. Remove once NPC
		// testing is done.
		private static readonly int[] TestTownNpcTypes =
		{
			ModContent.NPCType<GuySenseiNPC>(),
			ModContent.NPCType<KakashiNPC>(),
			ModContent.NPCType<ItachiNPC>(),
			ModContent.NPCType<ShinobiVendorNPC>(),
		};

		// TEMP: Guy Sensei/Itachi/Kakashi/Tenten are all present from world start, but most of their
		// shop stock is gated behind StoryProgressSystem's downed-boss flags (see each NPC's
		// AddShops()) - force every flag true so the full shop/dialogue can be tested without
		// replaying the whole boss progression first. Remove once NPC testing is done.
		public override void OnEnterWorld()
		{
			ForceStoryProgressUnlock();
			ForceTownNpcSpawn();
			GiveStarterItemsOnce();
		}

		private void GiveStarterItemsOnce()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			if (hasGivenStarterItems)
			{
				return;
			}

			hasGivenStarterItems = true;

			GiveItem(ModContent.ItemType<ShinobiHandbookItem>());

			// TEMP: the reworked Taijutsu kit (bigger blast VFX/hitbox/knockback/dash - see
			// TaijutsuKickProjectile) needs hands-on testing, so hand all four straight to a new
			// character instead of requiring a fresh Shukaku/Kakuzu grind first (ForceStoryProgressUnlock
			// above already unlocks CanUseItem's story gates). Remove once testing is done.
			GiveItem(ModContent.ItemType<IronLegItem>());
			GiveItem(ModContent.ItemType<FrontLotusItem>());
			GiveItem(ModContent.ItemType<LeafHurricaneItem>());
			GiveItem(ModContent.ItemType<GentleFistItem>());

			// TEMP: same reasoning as the Taijutsu kit above - Genjutsu Nightmare just got the same
			// large blast-VFX treatment (GenjutsuNightmareBlastProjectile) and needs hands-on testing.
			// Remove once testing is done.
			GiveItem(ModContent.ItemType<GenjutsuNightmareItem>());

			// TEMP: Susanoo's SusanooProjectile just had its pivot bug fixed (continuous
			// Projectile.rotation spin on a non-symmetric 170x130 avatar sprite, plus a missing
			// PreDraw override so the default draw anchored on the much-smaller 64x64 hitbox instead
			// of the real frame size) - needs hands-on testing. Remove once testing is done.
			GiveItem(ModContent.ItemType<SusanooItem>());
		}

		private void GiveItem(int itemType)
		{
			for (int i = 0; i < MainInventorySlots; i++)
			{
				if (Player.inventory[i].IsAir)
				{
					Player.inventory[i] = new Item(itemType);
					return;
				}
			}
		}

		private void ForceTownNpcSpawn()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			foreach (int npcType in TestTownNpcTypes)
			{
				if (!NPC.AnyNPCs(npcType))
				{
					NPC.NewNPC(Player.GetSource_Misc("BossTestKit"), (int)Player.Center.X, (int)Player.Center.Y, npcType);
				}
			}
		}

		private static void ForceStoryProgressUnlock()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			StoryProgressSystem.DownedHaku = true;
			StoryProgressSystem.DownedShukaku = true;
			StoryProgressSystem.DownedOrochimaru = true;
			StoryProgressSystem.DownedKakuzu = true;
			StoryProgressSystem.DownedPain = true;
			StoryProgressSystem.DownedMadara = true;
			StoryProgressSystem.DownedKaguya = true;

			StoryProgressSystem.SyncToClients();
		}
	}
}
