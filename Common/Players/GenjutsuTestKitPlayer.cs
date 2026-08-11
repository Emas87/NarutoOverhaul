using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Accessories;
using NarutoOverhaul.Content.Items.Consumables;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// TEMPORARY testing aid: hands the player every Genjutsu item on world entry and force-unlocks
	// the story flag Genjutsu's jutsu weapons are gated behind, so the whole class can be tried out
	// immediately without playing through Haku first. Remove once Genjutsu testing is done - this
	// is not meant to ship.
	//
	// DISABLED 2026-08-06 while testing Ninjutsu - see NinjutsuTestKitPlayer instead. Re-enable
	// (uncomment the OnEnterWorld body) when Genjutsu testing resumes.
	public class GenjutsuTestKitPlayer : ModPlayer
	{
		private static readonly int[] GenjutsuTestItemTypes =
		{
			ModContent.ItemType<GenjutsuEmblemItem>(),
			ModContent.ItemType<GenjutsuVeilItem>(),
			ModContent.ItemType<GenjutsuMasteryScrollItem>(),
			ModContent.ItemType<GenjutsuSleepItem>(),
			ModContent.ItemType<GenjutsuNightmareItem>(),
			ModContent.ItemType<GenjutsuIllusionItem>(),
		};

		public override void OnEnterWorld()
		{
			// no-op while disabled
		}
	}
}
