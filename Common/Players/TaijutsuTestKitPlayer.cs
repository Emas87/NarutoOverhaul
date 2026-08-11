using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Accessories;
using NarutoOverhaul.Content.Items.Armor;
using NarutoOverhaul.Content.Items.Consumables;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// TEMPORARY testing aid: hands the player every Taijutsu item on world entry and force-unlocks
	// the story flags Taijutsu's jutsu weapons are gated behind, so the whole class can be tried
	// out immediately without playing through Shukaku/Kakuzu first. Remove once Taijutsu testing
	// is done - this is not meant to ship.
	//
	// DISABLED 2026-08-06 while testing Genjutsu - see GenjutsuTestKitPlayer instead. Re-enable
	// (uncomment the ModPlayer base + OnEnterWorld body) when Taijutsu testing resumes.
	public class TaijutsuTestKitPlayer : ModPlayer
	{
		private static readonly int[] TaijutsuTestItemTypes =
		{
			ModContent.ItemType<TaijutsuHelmetItem>(),
			ModContent.ItemType<TaijutsuBodyItem>(),
			ModContent.ItemType<TaijutsuLegsItem>(),
			ModContent.ItemType<TaijutsuWrapsItem>(),
			ModContent.ItemType<TaijutsuEmblemItem>(),
			ModContent.ItemType<TaijutsuMasteryScrollItem>(),
			ModContent.ItemType<GentleFistItem>(),
			ModContent.ItemType<IronLegItem>(),
			ModContent.ItemType<LeafHurricaneItem>(),
			ModContent.ItemType<FrontLotusItem>(),
		};

		public override void OnEnterWorld()
		{
			// no-op while disabled
		}
	}
}
