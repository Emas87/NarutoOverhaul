using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	public class ActiveFormIndicatorUIState : UIState
	{
		private ActiveFormIndicatorElement activeFormIndicatorElement;

		public override void OnInitialize()
		{
			activeFormIndicatorElement = new ActiveFormIndicatorElement();
			activeFormIndicatorElement.Left.Set(20f, 0f);
			activeFormIndicatorElement.Top.Set(230f, 0f);
			activeFormIndicatorElement.Width.Set(150f, 0f);
			activeFormIndicatorElement.Height.Set(20f, 0f);

			Append(activeFormIndicatorElement);
		}
	}
}
