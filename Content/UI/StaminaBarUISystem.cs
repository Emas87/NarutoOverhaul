using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace NarutoOverhaul.Content.UI
{
	public class StaminaBarUISystem : ModSystem
	{
		internal StaminaBarUIState StaminaBarUIState;

		private UserInterface staminaBarUserInterface;

		public override void Load()
		{
			if (Main.dedServ)
			{
				return;
			}

			StaminaBarUIState = new StaminaBarUIState();
			staminaBarUserInterface = new UserInterface();
			staminaBarUserInterface.SetState(StaminaBarUIState);
		}

		public override void UpdateUI(GameTime gameTime)
		{
			staminaBarUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int manaBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mana Bar"));

			if (manaBarIndex == -1)
			{
				return;
			}

			layers.Insert(manaBarIndex + 1, new LegacyGameInterfaceLayer(
				"NarutoOverhaul: Stamina Bar",
				delegate
				{
					staminaBarUserInterface?.Draw(Main.spriteBatch, new GameTime());
					return true;
				},
				InterfaceScaleType.UI));
		}
	}
}
