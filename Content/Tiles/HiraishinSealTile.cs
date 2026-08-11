using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NarutoOverhaul.Content.Tiles
{
	// Minato's Hiraishin marker - a permanent, world-shared fast-travel point (like a vanilla
	// Teleporter/Pylon): any player can warp to any placed seal via TransformationPlayer's
	// Hiraishin Warp keybind, and it stays part of the network until physically mined out.
	// HiraishinMarkerSystem is the single source of truth for which tiles count as marks -
	// PlaceInWorld/KillTile just keep that registry in sync with the actual world tiles.
	public class HiraishinSealTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.addTile(Type);

			DustType = DustID.GoldFlame;
			AddMapEntry(new Color(255, 215, 100));
		}

		public override void PlaceInWorld(int i, int j, Item item)
		{
			HiraishinMarkerSystem.AddMark(new Point16(i, j));
		}

		// KillMultiTile only fires for tiles LARGER than 1x1 (per ModTile's own doc comment) - this
		// is a Style1x1 tile, so mining it never called that hook at all, meaning RemoveMark never
		// ran and a mined seal stayed warpable forever. KillTile is the correct hook for a 1x1 tile.
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (!fail)
			{
				HiraishinMarkerSystem.RemoveMark(new Point16(i, j));
			}
		}
	}
}
