using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// Item.useStyle only ever drives the arm (there's no vanilla "kick" use style) - vanilla's own
	// leg-walk-cycle frames are the closest existing poses to a kick, and forcing one during a kick
	// weapon's swing is a known trick (verified against TerrariaOverhaul's QuickSlashMeleeAnimation,
	// which does the same thing via a ModPlayer.PostUpdate legFrame override). Must run in
	// PostUpdate, not FrameEffects - vanilla's own Player.Frame() computes legFrame *after* the
	// FrameEffects hook fires, so an override made there gets clobbered before the tick renders;
	// PostUpdate runs after Frame() has already finished for the tick.
	//
	// This is layered on top of the whole-body rotation each kick weapon's own
	// TaijutsuKickProjectile drives via Player.fullRotation - the frame swap plus the body lean
	// together are what actually sell "kick" (verified against GumGum's AntiMannerKickCourse, a One
	// Piece mod's real Black Leg fighting-style kick, which does both).
	//
	// Each kick weapon gets its own leg pose so they don't all read as the same animation:
	//   - Iron Leg, Front Lotus: a single straight kick -> Jump frame (leg driven straight out).
	//   - Leaf Hurricane: a wide sweep -> Walk8 frame (a more extended stride pose).
	// Gentle Fist is a punch, not a kick, and isn't in this hook at all (arm-only, via
	// ItemUseStyleID.Thrust, no projectile).
	public class TaijutsuKickAnimationPlayer : ModPlayer
	{
		// Player leg spritesheet: 40x56 frames per row.
		private static readonly Rectangle JumpLegFrame = new Rectangle(0, 56 * 5, 40, 56);
		private static readonly Rectangle SweepLegFrame = new Rectangle(0, 56 * 13, 40, 56);

		public override void PostUpdate()
		{
			if (Player.itemAnimation <= 0 || Player.velocity.Y != 0f)
			{
				return;
			}

			switch (Player.HeldItem.ModItem)
			{
				case IronLegItem or FrontLotusItem:
					Player.legFrame = JumpLegFrame;
					break;
				case LeafHurricaneItem:
					Player.legFrame = SweepLegFrame;
					break;
			}
		}
	}
}
