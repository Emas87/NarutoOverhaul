using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	// Placeholder-art text label: draws nothing when no TransformationForm is active, otherwise
	// the active form's display name just below the Chakra/Stamina bars - so a toggled mode
	// (Sage Mode, Chakra Control, ...) is visible at a glance instead of only inferable from buffs.
	public class ActiveFormIndicatorElement : UIElement
	{
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			TransformationPlayer transformationPlayer = Main.LocalPlayer.GetModPlayer<TransformationPlayer>();

			if (transformationPlayer.ActiveFormIndex == -1)
			{
				return;
			}

			string label = TransformationSystem.RegisteredForms[transformationPlayer.ActiveFormIndex].DisplayName;
			CalculatedStyle dimensions = GetDimensions();
			Vector2 position = new Vector2(dimensions.X, dimensions.Y);

			Terraria.Utils.DrawBorderString(spriteBatch, label, position, Color.White, 1f, 0f, 0f, -1);
		}
	}
}
