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
	// PlaceInWorld/KillMultiTile just keep that registry in sync with the actual world tiles.
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

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			HiraishinMarkerSystem.RemoveMark(new Point16(i, j));
		}
	}
}
