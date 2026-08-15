using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Dedicated buff for RamenStandTile's proximity effect - deliberately separate from vanilla's
	// BuffID.WellFed (which RamenItem still grants when eaten) so the tile's continuous refresh can
	// never clobber a real 10-minute food buff. WellFed is a vanilla "fed state" buff - AddBuff
	// deletes any existing fed-state buff before adding a new one, regardless of remaining time
	// (confirmed via decompile) - the tile used to share WellFed and that's exactly what caused it to
	// wipe out a real Ramen buff down to a fraction of a second. Also matches vanilla's own precedent
	// for this kind of "as long as you're near X" buff: Sunflower doesn't reuse WellFed either, it
	// grants its own dedicated BuffID.Happy.
	public class RamenComfortBuff : ModBuff
	{
		// No dedicated art - reuses the Ramen item's own icon, matching this session's established
		// "reuse existing art via Texture override" convention.
		public override string Texture => "NarutoOverhaul/Content/Items/Consumables/RamenItem";

		public override void SetStaticDefaults()
		{
			// Per BuffID.Sets' own documentation: a "presence" buff refreshed continuously while
			// nearby (like Sunflower's Happy) should hide its timer and skip save - a duration that's
			// rewritten every tick isn't meaningful to show or persist.
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}
	}
}
