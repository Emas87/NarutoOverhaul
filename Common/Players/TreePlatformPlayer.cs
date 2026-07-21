using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// Trees aren't solid in vanilla Terraria (Main.tileSolid[TileID.Trees] is false), so there's no
	// built-in way to stand on one. Making them a one-way platform the normal way (Main.tileSolidTop)
	// would affect every player/NPC/monster in the world - this instead fakes solid-top collision
	// bespoke, scoped to this player only, the same general technique ChakraControlForm already uses
	// for wall-climbing in this mod.
	public class TreePlatformPlayer : ModPlayer
	{
		private const float TreeJumpBoostSpeed = 14f; // punchy, ~2-3x base run speed - tune to taste
		private const float LandingToleranceY = 8f;

		private bool wasOnTreePlatform;

		public override void PreUpdateMovement()
		{
			// Gated on LAST tick's platform state, not this tick's - JumpMovement() (vanilla) runs
			// before this hook and already applies the jump's upward velocity once justJumped is
			// true, so re-checking "on a tree" fresh this tick would always see the player already
			// moving upward and say no.
			if (Player.justJumped && wasOnTreePlatform && System.Math.Abs(Player.velocity.X) < TreeJumpBoostSpeed)
			{
				Player.velocity.X = TreeJumpBoostSpeed * Player.direction;
			}

			wasOnTreePlatform = TryLandOnTree();
		}

		private bool TryLandOnTree()
		{
			if (Player.velocity.Y < 0f || Player.controlDown)
			{
				return false; // moving upward (e.g. mid-jump), or dropping through like a vanilla platform
			}

			Point feetTile = (Player.position + new Vector2(Player.width / 2f, Player.height)).ToTileCoordinates();
			Tile tile = Main.tile[feetTile.X, feetTile.Y];

			if (!tile.HasTile || !TileID.Sets.IsATreeTrunk[tile.TileType])
			{
				return false;
			}

			float treeTopWorldY = feetTile.Y * 16f;

			if (Player.position.Y + Player.height > treeTopWorldY + LandingToleranceY)
			{
				return false; // feet are below the tree's top surface - don't snap up into it
			}

			Player.position.Y = treeTopWorldY - Player.height;
			Player.velocity.Y = 0f;
			Player.fallStart = feetTile.Y;
			return true;
		}
	}
}
