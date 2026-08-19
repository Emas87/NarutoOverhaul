using System;
using System.Collections.Generic;
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
	// TargetPixelSize is that reference size, chosen relative to the standard 52x52 slot backing
	// (Terraria.UI.ItemSlot's val2.Size() - see decompile) that vanilla's own DrawItemIcon caps
	// icons to just 32px within, leaving them looking small in the slot. 37px (down 20% from an
	// initial 46px, which read as slightly too big once icons filled the slot uniformly) fills
	// the slot without crowding it, leaving a bit more margin so nothing clips the slot border.
	// Only replaces the base + color-tint draw calls that ItemSlot.DrawItemIcon does by default;
	// deliberately doesn't replicate its glowmask special-cases (Bewitching Table etc) since those
	// are vanilla item IDs, not ours.
	public class InventoryIconScaleGlobalItem : GlobalItem
	{
		private const float TargetPixelSize = 37f;

		// Per-item-canvas padding is wildly inconsistent across our icons (armor pieces are
		// cropped edge-to-edge, most weapon/material icons carry 30-65% dead transparent margin
		// - see the icon audit that led to this change), so scaling the raw texture FRAME to
		// TargetPixelSize still left padded icons reading much smaller than the armor pieces even
		// though their frames were now nominally the same size. Instead we measure each texture's
		// actual non-transparent pixel bounds once (lazily, cached by item type - our icons are
		// single, non-animated frames, so this is stable) and scale that to TargetPixelSize, so
		// the visible art itself - not the surrounding empty canvas - fills the slot uniformly.
		private static readonly Dictionary<int, Rectangle> VisibleBoundsCache = new();

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (item.ModItem?.Mod != Mod)
			{
				return true;
			}

			Texture2D texture = TextureAssets.Item[item.type].Value;
			Rectangle visibleBounds = GetVisibleBounds(item.type, texture, frame);

			float maxDimension = Math.Max(visibleBounds.Width, visibleBounds.Height);
			float uniformScale = Main.inventoryScale * (TargetPixelSize / maxDimension);

			// The inventory UI batch (Main.DrawInterface -> DrawInventory -> ItemSlot.Draw, which
			// is what's currently open when this hook fires) is opened with
			// Main.SamplerStateForCursor, which is SamplerState.LinearClamp (bilinear) - fine for
			// vanilla's modest up-to-32px icons, but our up to ~5x visible-bounds upscale makes
			// that blur obvious on flat-color pixel art. Swap to PointClamp just for these two
			// draws, then restore so nothing else drawn after us in this batch is affected.
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

			spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, uniformScale, SpriteEffects.None, 0f);
			if (item.color != Color.Transparent)
			{
				spriteBatch.Draw(texture, position, frame, itemColor, 0f, origin, uniformScale, SpriteEffects.None, 0f);
			}

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.SamplerStateForCursor, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

			return false;
		}

		private static Rectangle GetVisibleBounds(int itemType, Texture2D texture, Rectangle frame)
		{
			if (VisibleBoundsCache.TryGetValue(itemType, out Rectangle cached))
			{
				return cached;
			}

			Rectangle bounds = ComputeVisibleBounds(texture, frame);
			VisibleBoundsCache[itemType] = bounds;
			return bounds;
		}

		private static Rectangle ComputeVisibleBounds(Texture2D texture, Rectangle frame)
		{
			var pixels = new Color[frame.Width * frame.Height];
			texture.GetData(0, frame, pixels, 0, pixels.Length);

			int minX = frame.Width, minY = frame.Height, maxX = -1, maxY = -1;
			for (int y = 0; y < frame.Height; y++)
			{
				for (int x = 0; x < frame.Width; x++)
				{
					if (pixels[y * frame.Width + x].A == 0)
					{
						continue;
					}

					if (x < minX) minX = x;
					if (x > maxX) maxX = x;
					if (y < minY) minY = y;
					if (y > maxY) maxY = y;
				}
			}

			if (maxX < minX || maxY < minY)
			{
				// Fully transparent frame (shouldn't happen for a real icon) - fall back to the
				// full frame so we don't divide by zero.
				return new Rectangle(0, 0, frame.Width, frame.Height);
			}

			return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
		}
	}
}
