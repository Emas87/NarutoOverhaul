using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NarutoOverhaul.Content.Tiles
{
	// Ichiraku-style ramen stand - a placeable comfort station: standing near it keeps a small
	// regen bonus topped up, like a campfire for food. Decor with a small mechanical hook.
	public class RamenStandTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.addTile(Type);

			DustType = DustID.WoodFurniture;
			AddMapEntry(new Color(190, 50, 50));
		}

		// Uses its own dedicated RamenComfortBuff instead of vanilla's BuffID.WellFed - WellFed is a
		// "fed state" buff, and Player.AddBuff (confirmed via decompile) DELETES any existing
		// fed-state buff before adding a new one regardless of remaining time, so reusing it here
		// previously let this tile's own continuous refresh wipe out even a real 10-minute Well Fed
		// buff from eating actual food (like RamenItem) down to a fraction of a second. A dedicated
		// buff ID can't clobber WellFed at all (it isn't in the fed-state set), so this can go back to
		// a plain continuous refresh with no special-casing - same pattern vanilla's own Sunflower
		// uses (its own dedicated BuffID.Happy, not WellFed either).
		private const int ProximityBuffTime = 120; // 2 seconds - refreshed continuously while nearby

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (closer)
			{
				Main.LocalPlayer.AddBuff(ModContent.BuffType<RamenComfortBuff>(), ProximityBuffTime);
			}
		}
	}
}
