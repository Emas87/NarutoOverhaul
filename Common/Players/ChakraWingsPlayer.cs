using NarutoOverhaul.Content.Items.Accessories;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// Chakra Wings only draw while the player is actively flying - grounded OR just falling/gliding
	// both hide the equip sprite entirely, rather than showing a folded-wings idle/fall pose.
	// "Actively flying" mirrors the same condition vanilla uses to compute WingUpdate's own inUse
	// (controlJump held + wingTime remaining - see ChakraWingsItem.WingUpdate's doc reference), not
	// just "airborne", so a player who's simply falling or has burned through their flight doesn't
	// keep showing wings. Scoped to this specific item's wing slot (not the whole
	// PlayerDrawLayers.Wings layer) so it doesn't affect any other wing accessory a modpack might add.
	public class ChakraWingsPlayer : ModPlayer
	{
		public override void HideDrawLayers(PlayerDrawSet drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			bool isChakraWings = drawPlayer.wings == ModContent.GetInstance<ChakraWingsItem>().Item.wingSlot;
			bool isActivelyFlying = drawPlayer.controlJump && drawPlayer.wingTime > 0f;

			if (isChakraWings && !isActivelyFlying)
			{
				global::Terraria.DataStructures.PlayerDrawLayers.Wings.Hide();
			}
		}
	}
}
