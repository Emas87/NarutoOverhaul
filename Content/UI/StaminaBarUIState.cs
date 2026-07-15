using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	public class StaminaBarUIState : UIState
	{
		private StaminaBarElement staminaBarElement;

		public override void OnInitialize()
		{
			staminaBarElement = new StaminaBarElement();
			staminaBarElement.Left.Set(20f, 0f);
			staminaBarElement.Top.Set(205f, 0f);
			staminaBarElement.Width.Set(150f, 0f);
			staminaBarElement.Height.Set(20f, 0f);

			Append(staminaBarElement);
		}
	}
}
