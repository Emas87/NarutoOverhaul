using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.GlobalNPCs
{
	// Kaguya's signature drop reaches into vanilla's own endgame: any character who has ever
	// consumed the Otsutsuki Chakra Fragment (a permanent, one-time blessing - see
	// MoonLordBlessingPlayer) gets periodic homing assist bolts (KaguyaBlessingBoltProjectile) fired
	// at whichever Moon Lord part is currently active, framing her as stronger than he is rather
	// than gated behind him - without spelling that out anywhere the player can read
	// (OtsutsukiChakraFragmentItem's tooltip deliberately doesn't mention this). Replaces an earlier
	// version of this class that just flatly reduced Moon Lord's damage/defense.
	public class MoonLordAssistGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private const int ShotIntervalTicks = 360; // 6 seconds, per Moon Lord part - was 180, cadence halved
		private const int BoltDamage = 100; // was 267

		private int shotCooldown;

		public override void AI(NPC npc)
		{
			if (npc.type != NPCID.MoonLordHead && npc.type != NPCID.MoonLordHand && npc.type != NPCID.MoonLordCore)
			{
				return;
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			Player blessedPlayer = FindBlessedPlayer();

			if (blessedPlayer == null)
			{
				shotCooldown = 0;
				return;
			}

			shotCooldown++;

			if (shotCooldown < ShotIntervalTicks)
			{
				return;
			}

			shotCooldown = 0;

			Vector2 spawnPosition = blessedPlayer.Center + new Vector2(Main.rand.NextFloat(-150f, 150f), -250f);
			Vector2 velocity = (npc.Center - spawnPosition).SafeNormalize(Vector2.UnitY) * 6f;

			Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPosition, velocity, ModContent.ProjectileType<KaguyaBlessingBoltProjectile>(), BoltDamage, 0f, blessedPlayer.whoAmI, ai0: npc.whoAmI);
		}

		private static Player FindBlessedPlayer()
		{
			for (int p = 0; p < Main.maxPlayers; p++)
			{
				Player player = Main.player[p];

				if (player.active && player.GetModPlayer<MoonLordBlessingPlayer>().HasKaguyaBlessing)
				{
					return player;
				}
			}

			return null;
		}
	}
}
