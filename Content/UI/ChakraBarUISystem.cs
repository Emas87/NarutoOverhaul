using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	public class ChakraBarUISystem : ModSystem
	{
		internal ChakraBarUIState ChakraBarUIState;

		private UserInterface chakraBarUserInterface;

		public override void Load()
		{
			if (Main.dedServ)
			{
				return;
			}

			ChakraBarUIState = new ChakraBarUIState();
			chakraBarUserInterface = new UserInterface();
			chakraBarUserInterface.SetState(ChakraBarUIState);
		}

		public override void UpdateUI(GameTime gameTime)
		{
			chakraBarUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int manaBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

			if (manaBarIndex == -1)
			{
				return;
			}

			layers.Insert(manaBarIndex + 1, new LegacyGameInterfaceLayer(
				"NarutoOverhaul: Chakra Bar",
				delegate
				{
					chakraBarUserInterface?.Draw(Main.spriteBatch, new GameTime());
					return true;
				},
				InterfaceScaleType.UI));
		}
	}
}
