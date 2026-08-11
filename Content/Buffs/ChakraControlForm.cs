using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Chakra control training (tree climbing, water walking) - canonically early-game, not tied to
	// any story boss like the other 3 forms. Cheap on purpose (a fraction of Sage Mode's drain):
	// this is a utility toggle, not a combat cost.
	public class ChakraControlForm : TransformationForm
	{
		public override string DisplayName => "Chakra Control";
		public override int BuffType => ModContent.BuffType<ChakraControlBuff>();
		public override int ActivationCost => 10;
		public override float ChakraDrainPerTick => 0.05f;
		public override bool IsUnlocked(Player player) => true;

		// Wall climbing was originally a hand-rolled SolidCollision heuristic (checking boxes near
		// the player) - it had real bugs (wrong box positions meant "on ground" read true while
		// pressed against a wall, so climbing never triggered) and duplicated a mechanic vanilla
		// already ships: Player.spikedBoots is exactly the Climbing Claws/Tiger Climbing Gear stat
		// (1 = slow slide down, 2 = full stick + climb via controlUp/controlDown), driven by
		// Player.WallslideMovement() using slideDir, which vanilla derives from real tile-collision
		// results rather than a manual box check. Setting spikedBoots = 2 here every tick gets the
		// same robust wall-cling for free, dust effects included, instead of reinventing it.
		public override void ApplyStatBoosts(Player player)
		{
			player.waterWalk = true;
			player.spikedBoots = 2;
		}
	}
}
