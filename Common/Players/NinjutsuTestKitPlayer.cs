using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Accessories;
using NarutoOverhaul.Content.Items.Consumables;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// TEMPORARY testing aid: hands the player every Ninjutsu item on world entry and force-unlocks
	// every story flag Ninjutsu's jutsu weapons are gated behind (Orochimaru/Kakuzu/Pain/Kaguya -
	// Fireball, Great Breakthrough and Water Dragon share Kakuzu), so the whole class can be tried
	// out immediately without playing through the story. Remove once Ninjutsu testing is done -
	// this is not meant to ship.
	//
	// DISABLED 2026-08-10 - Ninjutsu is fully tested (see totest.md), moving on to
	// TransformationTestKitPlayer. Re-enable (uncomment the OnEnterWorld body) if Ninjutsu ever
	// needs retesting.
	public class NinjutsuTestKitPlayer : ModPlayer
	{
		private static readonly int[] NinjutsuTestItemTypes =
		{
			ModContent.ItemType<NinjutsuEmblemItem>(),
			ModContent.ItemType<NinjutsuFocusSealItem>(),
			ModContent.ItemType<NinjutsuMasteryScrollItem>(),
			ModContent.ItemType<FireballItem>(),
			ModContent.ItemType<GreatBreakthroughItem>(),
			ModContent.ItemType<WaterDragonItem>(),
			ModContent.ItemType<RasenganItem>(),
			ModContent.ItemType<ChidoriItem>(),
			ModContent.ItemType<ShadowCloneItem>(),
			ModContent.ItemType<AllKillingAshBonesItem>(),
		};

		public override void OnEnterWorld()
		{
			// no-op while disabled
		}
	}
}
