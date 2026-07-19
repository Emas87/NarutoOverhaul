using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace NarutoOverhaul.Content.Tiles
{
	// Ichiraku-style ramen stand - a placeable comfort station: standing near it keeps the
	// Well Fed buff topped up, like a campfire for food. Decor with a small mechanical hook.
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

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (closer)
			{
				Main.LocalPlayer.AddBuff(BuffID.WellFed, 20);
			}
		}
	}
}
