using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.GlobalItems
{
	// Draws every NarutoOverhaul item icon in inventory/equipment slots (and other ItemSlot.Draw
	// contexts - chests, trash slot, etc) at one consistent on-screen pixel size, instead of
	// vanilla's default behavior (native size up to 32px, then shrunk-to-fit above that) which made
	// icons from different source-canvas sizes read as inconsistently sized next to each other -
	// e.g. the 26x26-canvas Taijutsu armor pieces looked smaller than the 34-44px Jutsu icons.
	// TargetPixelSize is that reference size: the 26x26 Armor category's canvas (the smallest used)
	// at the previous 1.5x enlargement, so armor doesn't shrink from where it already looked right -
	// everything else now scales up or down from its own native frame size to match it exactly.
	// Only replaces the base + color-tint draw calls that ItemSlot.DrawItemIcon does by default;
	// deliberately doesn't replicate its glowmask special-cases (Bewitching Table etc) since those
	// are vanilla item IDs, not ours.
	public class InventoryIconScaleGlobalItem : GlobalItem
	{
		private const float TargetPixelSize = 26f * 1.5f;

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (item.ModItem?.Mod != Mod)
			{
				return true;
			}

			float maxDimension = Math.Max(frame.Width, frame.Height);
			float uniformScale = Main.inventoryScale * (TargetPixelSize / maxDimension);
			Texture2D texture = TextureAssets.Item[item.type].Value;

			spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, uniformScale, SpriteEffects.None, 0f);
			if (item.color != Color.Transparent)
			{
				spriteBatch.Draw(texture, position, frame, itemColor, 0f, origin, uniformScale, SpriteEffects.None, 0f);
			}

			return false;
		}
	}
}
