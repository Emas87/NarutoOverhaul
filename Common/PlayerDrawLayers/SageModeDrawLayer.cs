using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.PlayerDrawLayers
{
	// Placeholder aura: a translucent colored square above the player's head. This is the hardest
	// sprite to eventually replace with real art since an overlay sheet must match the vanilla
	// player frame grid exactly (see the art pipeline in the plan) - keep it a flat placeholder
	// until the rest of the loop (chakra/jutsu/boss) is proven.
	public class SageModeDrawLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition() => new AfterParent(global::Terraria.DataStructures.PlayerDrawLayers.Skin);

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		{
			return drawInfo.drawPlayer.GetModPlayer<TransformationPlayer>().ActiveFormIndex != -1;
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player player = drawInfo.drawPlayer;
			Vector2 position = player.Top - Main.screenPosition + new Vector2(-8f, -20f);

			var auraDrawData = new DrawData(
				TextureAssets.MagicPixel.Value,
				position,
				new Rectangle(0, 0, 1, 1),
				new Color(255, 140, 0) * 0.6f,
				0f,
				Vector2.Zero,
				new Vector2(16f, 16f),
				SpriteEffects.None,
				0);

			drawInfo.DrawDataCache.Add(auraDrawData);
		}
	}
}
