using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	// Placeholder-art bar, same technique as ChakraBarElement but a different color (orange)
	// so it visibly reads as a distinct resource from the blue Chakra bar.
	public class StaminaBarElement : UIElement
	{
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			StaminaPlayer staminaPlayer = Main.LocalPlayer.GetModPlayer<StaminaPlayer>();

			CalculatedStyle dimensions = GetDimensions();
			var backgroundRect = new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height);

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, backgroundRect, Color.Black * 0.6f);

			float fillPercent = staminaPlayer.MaxStamina > 0f ? staminaPlayer.Stamina / staminaPlayer.MaxStamina : 0f;
			fillPercent = MathHelper.Clamp(fillPercent, 0f, 1f);

			var fillRect = new Rectangle(
				backgroundRect.X + 2,
				backgroundRect.Y + 2,
				(int)((backgroundRect.Width - 4) * fillPercent),
				backgroundRect.Height - 4);

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, new Color(220, 120, 30));
		}
	}
}
