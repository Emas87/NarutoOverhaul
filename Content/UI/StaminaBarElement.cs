using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	// Placeholder-art bar, same technique as ChakraBarElement but a different color (yellow)
	// so it visibly reads as a distinct resource from the blue Chakra bar.
	public class StaminaBarElement : UIElement
	{
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (!ModContent.GetInstance<NarutoOverhaulConfig>().ShowResourceBars)
			{
				return;
			}

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

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, new Color(235, 210, 30));

			string text = $"{(int)staminaPlayer.Stamina}/{(int)staminaPlayer.MaxStamina}";
			Vector2 center = new Vector2(backgroundRect.X + (backgroundRect.Width / 2f), backgroundRect.Y + (backgroundRect.Height / 2f));
			Utils.DrawBorderString(spriteBatch, text, center, Color.White, 0.8f, 0.5f, 0.5f);
		}
	}
}
