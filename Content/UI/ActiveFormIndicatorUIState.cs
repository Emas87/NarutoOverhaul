using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	public class ActiveFormIndicatorUIState : UIState
	{
		private ActiveFormIndicatorElement activeFormIndicatorElement;

		public override void OnInitialize()
		{
			activeFormIndicatorElement = new ActiveFormIndicatorElement();
			// See ChakraBarUIState - centered in the gap between inventory and minimap.
			activeFormIndicatorElement.Left.Set(-20f, 0.5f);
			activeFormIndicatorElement.Top.Set(70f, 0f);
			activeFormIndicatorElement.Width.Set(150f, 0f);
			activeFormIndicatorElement.Height.Set(20f, 0f);

			Append(activeFormIndicatorElement);
		}
	}
}
