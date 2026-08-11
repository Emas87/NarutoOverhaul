using NarutoOverhaul.Content.Items.Armor;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// TEMPORARY testing aid: hands the player every armor piece in the mod (Taijutsu, Ninjutsu,
	// Genjutsu, and all 5 village headbands) on world entry, so new/regenerated armor art can be
	// visually checked in-game immediately without farming/crafting each set. Remove once armor
	// art verification across all sets is done - this is not meant to ship.
	//
	// DISABLED 2026-08-06 while testing Genjutsu only - StarterItemPlayer already hands out the
	// Genjutsu set at spawn. Re-enable (uncomment the OnEnterWorld body) once broad armor
	// verification resumes.
	public class AllArmorTestKitPlayer : ModPlayer
	{
		private static readonly int[] AllArmorItemTypes =
		{
			ModContent.ItemType<TaijutsuHelmetItem>(),
			ModContent.ItemType<TaijutsuBodyItem>(),
			ModContent.ItemType<TaijutsuLegsItem>(),
			ModContent.ItemType<NinjutsuHelmetItem>(),
			ModContent.ItemType<NinjutsuBodyItem>(),
			ModContent.ItemType<NinjutsuLegsItem>(),
			ModContent.ItemType<GenjutsuHelmetItem>(),
			ModContent.ItemType<GenjutsuBodyItem>(),
			ModContent.ItemType<GenjutsuLegsItem>(),
			ModContent.ItemType<LeafVillageHeadbandItem>(),
			ModContent.ItemType<CloudVillageHeadbandItem>(),
			ModContent.ItemType<MistVillageHeadbandItem>(),
			ModContent.ItemType<SandVillageHeadbandItem>(),
			ModContent.ItemType<StoneVillageHeadbandItem>(),
		};

		public override void OnEnterWorld()
		{
			// no-op while disabled
		}
	}
}
