using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	public class ChakraBarUIState : UIState
	{
		private ChakraBarElement chakraBarElement;

		public override void OnInitialize()
		{
			chakraBarElement = new ChakraBarElement();
			// Anchored at 50% of screen width (minus half this element's own width, to center it),
			// landing in the open gap between the inventory panel (top-left) and the minimap/health
			// display (top-right) regardless of resolution/UI scale.
			chakraBarElement.Left.Set(-20f, 0.5f);
			chakraBarElement.Top.Set(20f, 0f);
			chakraBarElement.Width.Set(150f, 0f);
			chakraBarElement.Height.Set(20f, 0f);

			Append(chakraBarElement);
		}
	}
}
