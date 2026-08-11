using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	// Six Paths-themed mobility - the mod's first wings equivalent, filling the "no mobility
	// endgame" gap. Gated one tier before the true final boss (Kaguya), same placement logic as
	// the endgame weapons' Madara-tier gates - a reward for reaching the back half of the story
	// rather than "you've basically already won."
	[AutoloadEquip(EquipType.Wings)]
	public class ChakraWingsItem : ModItem
	{
		// Small enough to not compete with real jutsu (cheapest of those, GenjutsuSleep, is 16 -
		// see the ChakraCost consts across Content/Items/Weapons/Jutsu) but non-zero, so flight
		// isn't entirely free chakra-wise. Only charged while actively flying (ChakraDrainPerTick
		// name/scale matches TransformationForm's forms), not while just gliding/falling.
		public const float ChakraDrainPerTick = 0.1f;

		// Effectively unlimited - flight is meant to be gated by Chakra (see WingUpdate and
		// UpdateEquip below), not by vanilla's own flyTime countdown. A real "infinite" isn't
		// representable (WingStats.flyTime is a plain int countdown), so this is just large
		// enough that no realistic chakra pool outlasts it.
		private const int EffectivelyInfiniteFlyTime = 999999;

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 30);
			Item.rare = ItemRarityID.Red;
		}

		public override void SetStaticDefaults()
		{
			// flySpeedOverride, accelerationMultiplier, hasHoldDownHoverFeatures,
			// hoverFlySpeedOverride, hoverAccelerationMultiplier - mid-tier Hardmode wings, roughly
			// between vanilla's Fairy Wings and Betsy's Wings. flyTime is EffectivelyInfiniteFlyTime,
			// not a real tuned value - see that const's comment.
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(EffectivelyInfiniteFlyTime, 8f, 0.2f, true, 4f, 0.1f);
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
			ascentWhenFalling = 0.85f;
			ascentWhenRising = 0.15f;
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.1f;
		}

		public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
		{
			speed = 9f;
			acceleration = 0.16f;
		}

		// inUse is true only while actually flapping (jump held, wingTime remaining) - gliding after
		// wingTime runs out doesn't cost anything, same as vanilla wings' flight-time gate already
		// costing nothing to glide down. Returning false keeps vanilla's own flight dust/animation.
		//
		// With flyTime effectively infinite (see SetStaticDefaults), Chakra is the real fuel gauge:
		// once it's empty, force wingTime to 0 so vanilla's own flight code cuts the player off this
		// same frame exactly like real flyTime running out would.
		public override bool WingUpdate(Player player, bool inUse)
		{
			if (inUse && !player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraDrainPerTick))
			{
				player.wingTime = 0f;
			}

			return false;
		}

		// Chakra normally regenerates in midair too (e.g. jumping around casting jutsu) - these
		// wings are the one exception: while equipped and airborne (flying OR just falling, so
		// coasting down doesn't passively refuel either), Chakra can't regenerate at all. Landing is
		// the only way to refuel, same as the wing's own flyTime only refilling once grounded.
		// UpdateEquip runs once per tick while equipped, before ChakraPlayer.PostUpdateMiscEffects
		// consumes the flag (ResetEffects clears it back to false first each frame).
		public override void UpdateEquip(Player player)
		{
			if (player.velocity.Y != 0f)
			{
				player.GetModPlayer<ChakraPlayer>().SuppressRegenAirborne = true;
			}
		}
	}
}
