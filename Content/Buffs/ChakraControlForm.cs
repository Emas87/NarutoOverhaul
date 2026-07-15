using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Chakra control training (tree climbing, water walking) - canonically early-game, not tied to
	// any story boss like the other 3 forms. Cheap on purpose (a fraction of Sage Mode's drain):
	// this is a utility toggle, not a combat cost.
	public class ChakraControlForm : TransformationForm
	{
		private const float ClimbSpeed = 3f;
		private const float WallCheckOffset = 4f;

		public override string DisplayName => "Chakra Control";
		public override int BuffType => ModContent.BuffType<ChakraControlBuff>();
		public override int ActivationCost => 10;
		public override float ChakraDrainPerTick => 0.05f;
		public override bool IsUnlocked => true;

		public override void ApplyStatBoosts(Player player)
		{
			player.waterWalk = true;
		}

		// Wall/tree climbing has no vanilla equivalent to lean on (unlike waterWalk) - this is a
		// first-pass heuristic and the one part of this feature most likely to need tuning after a
		// real playtest. Runs from PreUpdateMovement (not ApplyStatBoosts/ResetEffects) because
		// overriding velocity/gravity has to happen before vanilla's own movement code for the tick,
		// or it just gets stomped.
		public override void PreUpdateMovement(Player player)
		{
			bool touchingWallLeft = Collision.SolidCollision(player.position - new Vector2(WallCheckOffset, 0f), player.width, player.height);
			bool touchingWallRight = Collision.SolidCollision(player.position + new Vector2(WallCheckOffset, 0f), player.width, player.height);
			bool onGround = Collision.SolidCollision(player.position + new Vector2(0f, 2f), player.width, 2);

			if (!onGround && (touchingWallLeft || touchingWallRight) && (player.controlUp || player.controlDown))
			{
				player.velocity.Y = player.controlUp ? -ClimbSpeed : ClimbSpeed;
				player.gravity = 0f;
			}
		}
	}
}
