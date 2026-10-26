using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace NarutoOverhaul.Content.NPCs.Bosses
{
	// Universal "don't stay wedged/bouncing in place forever" safety net, shared by every walking
	// boss so a fix to it can no longer land in only one hand-copied version (the stuck-escape
	// mid-jump bug of 2026-10-24 had to be fixed separately in Kakuzu and Orochimaru). "Stuck"
	// covers two cases: (1) the hitbox literally overlapping solid tiles (bad teleport/knockback),
	// and (2) repeatedly attempting to move (nonzero velocity - e.g. a Jump landing at the same spot
	// against a wall it can't clear) without ever making real net progress. Being stationary while
	// genuinely idle (casting/recovering, near-zero velocity) is NOT stuck and must not trigger
	// this, or a boss calmly casting could suddenly sink through the floor for no reason. Once
	// either condition holds for ToleranceTicks (3s) with less than MovementThreshold net
	// displacement, nudge it to a nearby clear-air spot and let gravity/collision (which stay ON
	// throughout - never touching NPC.noTileCollide) settle it onto whatever ground is below, same
	// as any normal fall/landing. An earlier version used NPC.noTileCollide to phase through
	// terrain, but that disables collision in EVERY direction including straight down, so a stuck
	// boss fell clean through the floor instead of escaping sideways/upward while still landing on
	// solid ground - this reposition approach can't do that since normal collision never turns off.
	//
	// A struct on purpose: tModLoader clones ModNPC instances with MemberwiseClone, so a class-typed
	// field would be shared between every live copy of the same boss.
	public struct BossStuckEscape
	{
		private const int ToleranceTicks = 180;
		private const float MovementThreshold = 60f;
		private const float MovingVelocityThreshold = 1.5f;
		private const float EscapeSearchRadius = 150f;
		private const int ClearSpotAttempts = 8;

		private int timer;
		private Vector2 windowStartPosition;

		// Call once per AI tick. onEscaped runs right after the teleport + velocity reset: a zeroed
		// velocity.Y reads as "grounded" to the velocity-based IsGrounded() landing checks, so a
		// boss with a Jump state must end that jump there (pass null if it has none).
		public void Update(NPC npc, Action onEscaped)
		{
			bool embedded = !IsAreaClear(npc.position, npc.width, npc.height);
			bool attemptingMovement = npc.velocity.LengthSquared() > MovingVelocityThreshold * MovingVelocityThreshold;

			if (!embedded && !attemptingMovement)
			{
				timer = 0;
				return;
			}

			if (timer == 0)
			{
				windowStartPosition = npc.Center;
			}

			timer++;

			if (Vector2.Distance(npc.Center, windowStartPosition) >= MovementThreshold)
			{
				timer = 0;
				return;
			}

			if (embedded || timer >= ToleranceTicks)
			{
				npc.Center = FindClearCenter(npc, npc.Center, EscapeSearchRadius);
				npc.velocity = Vector2.Zero;
				onEscaped?.Invoke();
				timer = 0;
			}
		}

		public static bool IsAreaClear(Vector2 topLeft, int width, int height)
		{
			int tileX1 = (int)(topLeft.X / 16f);
			int tileY1 = (int)(topLeft.Y / 16f);
			int tileX2 = (int)((topLeft.X + width) / 16f);
			int tileY2 = (int)((topLeft.Y + height) / 16f);

			for (int x = tileX1; x <= tileX2; x++)
			{
				for (int y = tileY1; y <= tileY2; y++)
				{
					if (WorldGen.SolidTile(x, y))
					{
						return false;
					}
				}
			}

			return true;
		}

		// Retries several random offsets around targetCenter and only commits to one whose full
		// hitbox footprint is clear, falling back to straight above targetCenter (virtually always
		// open air mid-fight) rather than ever risking a blind teleport into terrain.
		public static Vector2 FindClearCenter(NPC npc, Vector2 targetCenter, float radius)
		{
			for (int i = 0; i < ClearSpotAttempts; i++)
			{
				Vector2 candidateCenter = targetCenter + Main.rand.NextVector2CircularEdge(radius, radius);
				Vector2 candidateTopLeft = candidateCenter - new Vector2(npc.width / 2f, npc.height / 2f);

				if (IsAreaClear(candidateTopLeft, npc.width, npc.height))
				{
					return candidateCenter;
				}
			}

			return targetCenter - new Vector2(0f, 150f);
		}
	}
}
