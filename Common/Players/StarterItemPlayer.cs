using System.Collections.Generic;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Accessories;
using NarutoOverhaul.Content.Items.Armor;
using NarutoOverhaul.Content.Items.Consumables;
using NarutoOverhaul.Content.Items.Materials;
using NarutoOverhaul.Content.Items.Weapons;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using NarutoOverhaul.Content.NPCs.Town;
using Terraria;
using Terraria.ID;
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

		// TEMP: boss summon items, so the new [AutoloadBossHead] icons can be checked against a real
		// fight (health bar + minimap) without crafting each summon first. Remove once boss head
		// icon testing is done.
		private static readonly int[] BossSummonTestItemTypes =
		{
			ModContent.ItemType<HakuSummonItem>(),
			ModContent.ItemType<KakuzuSummonItem>(),
			ModContent.ItemType<OrochimaruSummonItem>(),
			ModContent.ItemType<PainSummonItem>(),
			ModContent.ItemType<MadaraSummonItem>(),
			ModContent.ItemType<KaguyaSummonItem>(),
			ModContent.ItemType<TailedBeastSummonItem>(),
		};

		// TEMP: every item whose icon was regenerated in the 2026-08-14 icon-clarity pass (see
		// SPRITE_PROMPTS.md), one of each, so all 82 new icons can be checked in the inventory grid
		// at once instead of crafting/finding each individually. Remove once icon testing is done.
		private static readonly int[] IconTestItemTypes =
		{
			// Accessories
			ModContent.ItemType<ChakraPaperItem>(),
			ModContent.ItemType<ChakraWingsItem>(),
			ModContent.ItemType<GenjutsuEmblemItem>(),
			ModContent.ItemType<GenjutsuVeilItem>(),
			ModContent.ItemType<IllusionCharmItem>(),
			ModContent.ItemType<MonstrousStrengthGlovesItem>(),
			ModContent.ItemType<NinjutsuEmblemItem>(),
			ModContent.ItemType<NinjutsuFocusSealItem>(),
			ModContent.ItemType<SubstitutionScrollItem>(),
			ModContent.ItemType<TaijutsuEmblemItem>(),
			ModContent.ItemType<TaijutsuWrapsItem>(),
			ModContent.ItemType<WeightedLegWarmersItem>(),

			// Armor
			ModContent.ItemType<CloudVillageHeadbandItem>(),
			ModContent.ItemType<GenjutsuBodyItem>(),
			ModContent.ItemType<GenjutsuHelmetItem>(),
			ModContent.ItemType<GenjutsuLegsItem>(),
			ModContent.ItemType<LeafVillageHeadbandItem>(),
			ModContent.ItemType<MistVillageHeadbandItem>(),
			ModContent.ItemType<NinjutsuBodyItem>(),
			ModContent.ItemType<NinjutsuHelmetItem>(),
			ModContent.ItemType<NinjutsuLegsItem>(),
			ModContent.ItemType<SandVillageHeadbandItem>(),
			ModContent.ItemType<StoneVillageHeadbandItem>(),
			ModContent.ItemType<TaijutsuBodyItem>(),
			ModContent.ItemType<TaijutsuHelmetItem>(),
			ModContent.ItemType<TaijutsuLegsItem>(),

			// Consumables (shared-icon families represented by one member each)
			ModContent.ItemType<HakuBossBagItem>(),
			ModContent.ItemType<ChakraScroll1Item>(),
			ModContent.ItemType<StaminaScroll1Item>(),
			ModContent.ItemType<ChakraPotionItem>(),
			ModContent.ItemType<ChakraRegenPotionItem>(),
			ModContent.ItemType<CursedSealFragmentItem>(),
			ModContent.ItemType<GenjutsuMasteryScrollItem>(),
			ModContent.ItemType<NinjutsuMasteryScrollItem>(),
			ModContent.ItemType<TaijutsuMasteryScrollItem>(),
			ModContent.ItemType<OtsutsukiChakraFragmentItem>(),
			ModContent.ItemType<RamenItem>(),
			ModContent.ItemType<SharinganAwakeningItem>(),
			ModContent.ItemType<StaminaPotionItem>(),
			ModContent.ItemType<StaminaRegenPotionItem>(),

			// Materials
			ModContent.ItemType<CursedSnakeFangItem>(),
			ModContent.ItemType<IceMirrorShardItem>(),
			ModContent.ItemType<KakuzuHeartItem>(),
			ModContent.ItemType<RinneganFragmentItem>(),
			ModContent.ItemType<SandCoreItem>(),
			ModContent.ItemType<SusanooCoreItem>(),

			// Weapons
			ModContent.ItemType<BulldogHoundSummonScrollItem>(),
			ModContent.ItemType<ExplosiveKunaiItem>(),
			ModContent.ItemType<FumaShurikenItem>(),
			ModContent.ItemType<KunaiItem>(),
			ModContent.ItemType<MinatoKunaiItem>(),
			ModContent.ItemType<NinjaHoundSummonScrollItem>(),
			ModContent.ItemType<PaperBombItem>(),
			ModContent.ItemType<ScoutHoundSummonScrollItem>(),
			ModContent.ItemType<ShurikenItem>(),
			ModContent.ItemType<SlugSummonScrollItem>(),
			ModContent.ItemType<SnakeSummonScrollItem>(),
			ModContent.ItemType<ToadSummonScrollItem>(),

			// Weapons/Jutsu
			ModContent.ItemType<AllKillingAshBonesItem>(),
			ModContent.ItemType<ChidoriItem>(),
			ModContent.ItemType<FireballItem>(),
			ModContent.ItemType<FrontLotusItem>(),
			ModContent.ItemType<GenjutsuIllusionItem>(),
			ModContent.ItemType<GenjutsuNightmareItem>(),
			ModContent.ItemType<GenjutsuSleepItem>(),
			ModContent.ItemType<GentleFistItem>(),
			ModContent.ItemType<GreatBreakthroughItem>(),
			ModContent.ItemType<IronLegItem>(),
			ModContent.ItemType<LeafHurricaneItem>(),
			ModContent.ItemType<RasenganItem>(),
			ModContent.ItemType<ShadowCloneItem>(),
			ModContent.ItemType<ShinraTenseiItem>(),
			ModContent.ItemType<SusanooItem>(),
			ModContent.ItemType<WaterDragonItem>(),
		};

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

			foreach (int itemType in BossSummonTestItemTypes)
			{
				GiveIfRoom(itemType);
			}
		}

		private void GiveIfRoom(int itemType, int stack = 1)
		{
			for (int i = 0; i < MainInventorySlots; i++)
			{
				if (Player.inventory[i].IsAir)
				{
					Player.inventory[i] = new Item(itemType, stack);
					return;
				}
			}
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

			foreach (int itemType in BossSummonTestItemTypes)
			{
				if (!Player.HasItem(itemType))
				{
					Player.QuickSpawnItem(Player.GetSource_Misc("BossTestKit"), itemType);
				}
			}

			foreach (int itemType in IconTestItemTypes)
			{
				if (!Player.HasItem(itemType))
				{
					Player.QuickSpawnItem(Player.GetSource_Misc("IconTestKit"), itemType);
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
