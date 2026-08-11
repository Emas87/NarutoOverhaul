using System.IO;
using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.GlobalNPCs
{
	// The mod's first GlobalNPC. Handles all 3 of Genjutsu's control effects - each is its own
	// debuff, checked independently, all sharing the same underlying trick: PreAI returning false
	// skips the NPC's own AI (targeting/attacks/pathing) entirely, and this hook drives its
	// movement directly instead:
	//   - GenjutsuControlDebuff (puppet): mirrors the casting player's own movement input.
	//   - GenjutsuSleepDebuff (sleep): total immobilization, zero velocity.
	//   - GenjutsuFearDebuff (nightmare): flees directly away from the caster.
	// Movement only for now, per the user's explicit scope: no forcing the NPC to deal damage to
	// other NPCs (would need a custom NPC-vs-NPC collision check) and it can't attack the player
	// either, since its AI is fully skipped in all 3 modes.
	public class GenjutsuGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private const float PuppetSpeed = 4f;
		private const float FleeSpeed = 5f;

		// The player who cast the effect - used by both the puppet (reads their input) and the
		// fear (flees from their position) modes. Unused/ignored by the sleep debuff.
		public int ControllingPlayerIndex = -1;

		public override bool PreAI(NPC npc)
		{
			if (npc.HasBuff(ModContent.BuffType<GenjutsuSleepDebuff>()))
			{
				npc.velocity = Vector2.Zero;
				return false;
			}

			if (npc.HasBuff(ModContent.BuffType<GenjutsuControlDebuff>()))
			{
				return PuppetMovement(npc);
			}

			if (npc.HasBuff(ModContent.BuffType<GenjutsuFearDebuff>()))
			{
				return FleeMovement(npc);
			}

			ControllingPlayerIndex = -1;
			return true;
		}

		private bool PuppetMovement(NPC npc)
		{
			if (ControllingPlayerIndex < 0 || !Main.player[ControllingPlayerIndex].active || Main.player[ControllingPlayerIndex].dead)
			{
				return true; // debuff present but no valid controller - let normal AI run this tick
			}

			Player controller = Main.player[ControllingPlayerIndex];
			var direction = new Vector2(
				(controller.controlRight ? 1 : 0) - (controller.controlLeft ? 1 : 0),
				(controller.controlDown ? 1 : 0) - (controller.controlUp ? 1 : 0));

			Vector2 desiredVelocity = direction.SafeNormalize(Vector2.Zero) * PuppetSpeed;
			npc.velocity = Collision.TileCollision(npc.position, desiredVelocity, npc.width, npc.height);
			npc.position += npc.velocity;

			if (npc.velocity.X != 0)
			{
				npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;
			}

			return false;
		}

		private bool FleeMovement(NPC npc)
		{
			if (ControllingPlayerIndex < 0 || !Main.player[ControllingPlayerIndex].active)
			{
				return true;
			}

			Vector2 away = (npc.Center - Main.player[ControllingPlayerIndex].Center).SafeNormalize(Vector2.UnitX);
			Vector2 desiredVelocity = away * FleeSpeed;
			npc.velocity = Collision.TileCollision(npc.position, desiredVelocity, npc.width, npc.height);
			npc.position += npc.velocity;
			npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;

			return false;
		}

		// ControllingPlayerIndex is InstancePerEntity (one copy per NPC instance) - without this,
		// only whichever machine set it (the projectile owner's client, via OnHitNPC) actually
		// knows who's controlling the puppet; the server's own authoritative NPC instance (and
		// every other client) never finds out, so the puppet/flee movement silently does nothing
		// for anyone but the caster. GenjutsuIllusionProjectile.OnHitNPC sets npc.netUpdate = true
		// whenever it changes this, forcing an immediate sync instead of waiting on the natural
		// throttled NPC-sync interval.
		public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			binaryWriter.Write(ControllingPlayerIndex);
		}

		public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
		{
			int received = binaryReader.ReadInt32();
			ControllingPlayerIndex = received >= 0 && received < Main.maxPlayers ? received : -1;
		}
	}
}
