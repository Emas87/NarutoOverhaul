using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	// Placeholder-art bar: solid-color rectangles via TextureAssets.MagicPixel.
	// Swap the two spriteBatch.Draw calls for a real frame/fill texture once art exists (see art pipeline in the plan).
	public class ChakraBarElement : UIElement
	{
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			ChakraPlayer chakraPlayer = Main.LocalPlayer.GetModPlayer<ChakraPlayer>();

			CalculatedStyle dimensions = GetDimensions();
			var backgroundRect = new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height);

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, backgroundRect, Color.Black * 0.6f);

			float fillPercent = chakraPlayer.MaxChakra > 0f ? chakraPlayer.Chakra / chakraPlayer.MaxChakra : 0f;
			fillPercent = MathHelper.Clamp(fillPercent, 0f, 1f);

			var fillRect = new Rectangle(
				backgroundRect.X + 2,
				backgroundRect.Y + 2,
				(int)((backgroundRect.Width - 4) * fillPercent),
				backgroundRect.Height - 4);

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, new Color(40, 130, 220));
		}
	}
}
