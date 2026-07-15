using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	public class ChakraBarUIState : UIState
	{
		private ChakraBarElement chakraBarElement;

		public override void OnInitialize()
		{
			chakraBarElement = new ChakraBarElement();
			chakraBarElement.Left.Set(20f, 0f);
			chakraBarElement.Top.Set(180f, 0f);
			chakraBarElement.Width.Set(150f, 0f);
			chakraBarElement.Height.Set(20f, 0f);

			Append(chakraBarElement);
		}
	}
}
