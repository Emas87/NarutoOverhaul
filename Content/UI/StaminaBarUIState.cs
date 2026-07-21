using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	public class StaminaBarUIState : UIState
	{
		private StaminaBarElement staminaBarElement;

		public override void OnInitialize()
		{
			staminaBarElement = new StaminaBarElement();
			// See ChakraBarUIState - centered in the gap between inventory and minimap.
			staminaBarElement.Left.Set(-20f, 0.5f);
			staminaBarElement.Top.Set(45f, 0f);
			staminaBarElement.Width.Set(150f, 0f);
			staminaBarElement.Height.Set(20f, 0f);

			Append(staminaBarElement);
		}
	}
}
