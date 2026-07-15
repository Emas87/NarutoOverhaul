using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	public class ActiveFormIndicatorUISystem : ModSystem
	{
		internal ActiveFormIndicatorUIState ActiveFormIndicatorUIState;

		private UserInterface activeFormIndicatorUserInterface;

		public override void Load()
		{
			if (Main.dedServ)
			{
				return;
			}

			ActiveFormIndicatorUIState = new ActiveFormIndicatorUIState();
			activeFormIndicatorUserInterface = new UserInterface();
			activeFormIndicatorUserInterface.SetState(ActiveFormIndicatorUIState);
		}

		public override void UpdateUI(GameTime gameTime)
		{
			activeFormIndicatorUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int manaBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mana Bar"));

			if (manaBarIndex == -1)
			{
				return;
			}

			layers.Insert(manaBarIndex + 1, new LegacyGameInterfaceLayer(
				"NarutoOverhaul: Active Form Indicator",
				delegate
				{
					activeFormIndicatorUserInterface?.Draw(Main.spriteBatch, new GameTime());
					return true;
				},
				InterfaceScaleType.UI));
		}
	}
}
